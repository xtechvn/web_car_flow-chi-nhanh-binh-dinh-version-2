$(document).ready(function () {
    _waiting_room.ApplyFontSize(_waiting_room.GetSavedFontSize());
    $('#font_size_picker').on('click', 'button[data-size]', function (e) {
        e.preventDefault();
        var size = $(this).data('size');
        _waiting_room.ApplyFontSize(size);
        try {
            localStorage.setItem(_waiting_room.FONT_SIZE_KEY, String(size));
        } catch (err) { }
    });

    $('#btn_fullscreen').on('click', function (e) {
        e.preventDefault();
        e.stopPropagation();
        _waiting_room.TogglePresentation();
    });

    // Người dùng thoát fullscreen của trình duyệt (Esc/F11) thì tắt luôn chế độ trình chiếu
    document.addEventListener('fullscreenchange', function () {
        if (!document.fullscreenElement && _waiting_room.isPresenting()) {
            _waiting_room.ExitPresentation();
        }
        _waiting_room.FitTableHeight();
    });
    $(document).on('keydown', function (e) {
        if (e.key === 'Escape' && _waiting_room.isPresenting()) {
            _waiting_room.ExitPresentation();
        }
    });

    $(window).on('resize', _waiting_room.FitTableHeight);
    _waiting_room.FitTableHeight();

    _waiting_room.GetList();
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/CarHub", { transport: signalR.HttpTransportType.WebSockets, skipNegotiation: true })
        .withAutomaticReconnect([2000, 5000, 10000])
        .build();
    connection.start()
        .then(() => console.log("✅ SignalR connected"))
        .catch(err => console.error(err));

    function renderRow(item) {
        var date = new Date(item.vehicleArrivalDate);
        let formatted =
            String(date.getHours()).padStart(2, '0') + ":" +
            String(date.getMinutes()).padStart(2, '0') + " " +
            String(date.getDate()).padStart(2, '0') + "/" +
            String(date.getMonth() + 1).padStart(2, '0') + "/" +
            date.getFullYear();

        return `
        <tr class="waiting_room_${item.id}" data-queue="${formatted}"  >
            <td>${item.recordNumber}</td>
            <td>${formatted}</td>
            <td>${item.vehicleNumber} </td>
            <td>
            <div style="display: flex; align-items: center">
                 <div style="white-space: pre-line;">
                      ${item.customerName} 
                 </div>
            </div>
            </td>
            <td> ${item.csNotes == null ? '' : item.csNotes} </td>
        </tr>`;
    }
    function sortTable() {
        const tbody = document.getElementById("dataBody-0");
        const rows = Array.from(tbody.querySelectorAll("tr"));

        rows.sort((a, b) => {
            const timeA = parseDateTime(a.dataset.queue);
            const timeB = parseDateTime(b.dataset.queue);
            return timeA - timeB; // tăng dần
        });

        tbody.innerHTML = "";
        rows.forEach(r => tbody.appendChild(r));
    }
    function addOrReplaceRow(item) {
        const tbody = document.getElementById("dataBody-0");
        if (!tbody) return;
        $('.waiting_room_' + item.id).remove();
        tbody.insertAdjacentHTML("beforeend", renderRow(item));
        sortTable();
    }

    // Xe đã xử lý: chỉ gỡ khỏi danh sách chờ
    connection.off("ListProcessingIsLoading_Da_SL");
    connection.on("ListProcessingIsLoading_Da_SL", function (item) {
        $('.waiting_room_' + item.id).remove();
    });

    connection.off("ListProcessingIsLoading");
    connection.on("ListProcessingIsLoading", function (item) {
        addOrReplaceRow(item);
    });

    connection.off("ProcessingIsLoading_khoa");
    connection.on("ProcessingIsLoading_khoa", function (item) {
        addOrReplaceRow(item);
    });

    //lấy từ ds xe đến nhà máy
    connection.off("ListCartoFactory_Da_SL");
    connection.on("ListCartoFactory_Da_SL", function (item) {
        addOrReplaceRow(item);
    });

    connection.off("ListCartoFactory");
    connection.on("ListCartoFactory", function (item) {
        $('#dataBody-0').find('.waiting_room_' + item.id).remove();
    });

    connection.onreconnecting(error => {
        console.warn("🔄 Đang reconnect...", error);
    });

    connection.onreconnected(connectionId => {
        console.log("✅ Đã reconnect. Connection ID:", connectionId);
    });

    connection.onclose(error => {
        console.error("❌ Kết nối bị đóng.", error);
    });
    function parseDateTime(str) {
        // "11:33 17/12/2025"
        const [time, date] = str.split(" ");
        const [hour, minute] = time.split(":").map(Number);
        const [day, month, year] = date.split("/").map(Number);

        return new Date(year, month - 1, day, hour, minute).getTime();
    };
});

var _waiting_room = {
    init: function () {
        _waiting_room.GetList();
    },
    FONT_SIZE_KEY: 'waiting_room_font_size',
    FONT_SIZES: [1, 2, 3],
    GetSavedFontSize: function () {
        var saved = null;
        try {
            saved = parseInt(localStorage.getItem(_waiting_room.FONT_SIZE_KEY), 10);
        } catch (err) { }
        return _waiting_room.FONT_SIZES.indexOf(saved) >= 0 ? saved : 1;
    },
    ApplyFontSize: function (size) {
        size = parseInt(size, 10);
        if (_waiting_room.FONT_SIZES.indexOf(size) < 0) size = 1;
        var page = document.querySelector('.waiting-room-page');
        if (!page) return;
        _waiting_room.FONT_SIZES.forEach(function (s) {
            page.classList.remove('font-size-' + s);
        });
        page.classList.add('font-size-' + size);
        $('#font_size_picker button').removeClass('active')
            .filter('[data-size="' + size + '"]').addClass('active');
    },
    isPresenting: function () {
        var page = document.querySelector('.waiting-room-page');
        return !!page && page.classList.contains('is-presenting');
    },
    TogglePresentation: function () {
        if (_waiting_room.isPresenting()) {
            _waiting_room.ExitPresentation();
        } else {
            _waiting_room.EnterPresentation();
        }
    },
    EnterPresentation: function () {
        var page = document.querySelector('.waiting-room-page');
        if (!page) return;
        page.classList.add('is-presenting');
        document.body.style.overflow = 'hidden';
        $('#btn_fullscreen').attr('title', 'Thoát trình chiếu (Esc)');
        _waiting_room.FitTableHeight();

        // Fullscreen API có thể bị chặn (iframe, trình duyệt cũ); khi đó vẫn phủ kín cửa sổ bằng CSS
        var root = document.documentElement;
        var request = root.requestFullscreen || root.webkitRequestFullscreen || root.msRequestFullscreen;
        if (request && !document.fullscreenElement) {
            try {
                var result = request.call(root);
                if (result && typeof result.catch === 'function') {
                    result.catch(function () { });
                }
            } catch (err) { }
        }
    },
    ExitPresentation: function () {
        var page = document.querySelector('.waiting-room-page');
        if (!page) return;
        page.classList.remove('is-presenting');
        document.body.style.overflow = '';
        $('#btn_fullscreen').attr('title', 'Chiếu toàn màn hình');

        if (document.fullscreenElement) {
            var exit = document.exitFullscreen || document.webkitExitFullscreen || document.msExitFullscreen;
            if (exit) {
                try {
                    var result = exit.call(document);
                    if (result && typeof result.catch === 'function') {
                        result.catch(function () { });
                    }
                } catch (err) { }
            }
        }
        _waiting_room.FitTableHeight();
    },
    // Bảng cao vừa khít phần còn lại của màn hình để chỉ bảng có scroll dọc, trang không bị scroll
    FitTableHeight: function () {
        var wrapper = document.getElementById('waiting_room_table');
        var page = document.querySelector('.waiting-room-page');
        if (!wrapper || !page) return;

        var MIN_HEIGHT = 250;
        var presenting = _waiting_room.isPresenting();
        var paddingBottom = parseFloat(window.getComputedStyle(page).paddingBottom) || 0;
        var top = wrapper.getBoundingClientRect().top;
        var height;

        if (presenting) {
            height = window.innerHeight - top - paddingBottom;
        } else {
            var footer = document.querySelector('.footer');
            var footerHeight = footer ? footer.offsetHeight : 0;
            height = window.innerHeight - (top + window.scrollY) - paddingBottom - footerHeight;
        }
        height = Math.max(Math.floor(height), MIN_HEIGHT);
        wrapper.style.height = height + 'px';

        if (!presenting) {
            var overflow = document.documentElement.scrollHeight - window.innerHeight;
            if (overflow > 0) {
                wrapper.style.height = Math.max(height - overflow, MIN_HEIGHT) + 'px';
            }
        }
    },
    GetList: function () {
        var model = {
            VehicleNumber: "",
            PhoneNumber: "",
            VehicleStatus: 0,
            LoadType: null,
            VehicleWeighingType: null,
            VehicleTroughStatus: null,
            TroughType: null,
            VehicleWeighingStatus: null,
            LoadingStatus: null,
            type: 0,
        }
        $.ajax({
            url: "/WaitingRoom/GetList",
            type: "post",
            data: { SearchModel: model },
            success: function (result) {
                $('#imgLoading').hide();
                $('#Processing_Is_Loading').html(result);
                _waiting_room.FitTableHeight();
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                console.log("Status: " + textStatus);
            }
        });
    },
}
