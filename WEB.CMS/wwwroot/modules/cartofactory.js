$(document).ready(function () {
    _cartofactory.init();
    _cartofactory.BindObdTokenEvents();
    var input_chua_xu_ly = document.getElementById("input_chua_xu_ly");
    input_chua_xu_ly.addEventListener("keypress", function (event) {
        // If the user presses the "Enter" key on the keyboard
        if (event.key === "Enter") {
            // Cancel the default action, if needed
            event.preventDefault();
            // Trigger the button element with a click
            _cartofactory.ListCartoFactory();
        }
    });
    input_chua_xu_ly.addEventListener("keyup", function (event) {
        _cartofactory.ListCartoFactory();
    });
    var input_da_xu_ly = document.getElementById("input_da_xu_ly");
    input_da_xu_ly.addEventListener("keypress", function (event) {
        // If the user presses the "Enter" key on the keyboard
        if (event.key === "Enter") {
            // Cancel the default action, if needed
            event.preventDefault();
            // Trigger the button element with a click
            _cartofactory.ListCartoFactory_Da_SL();
        }
    });
    input_da_xu_ly.addEventListener("keyup", function (event) {
        // Kích hoạt hàm khi giá trị thay đổi và người dùng thoát khỏi input
        _cartofactory.ListCartoFactory_Da_SL();
    });

    let $menu = null;
    let $currentBtn = null;

    // 🕵️ Lắng nghe sự kiện toàn cục để đóng menu khi cần
    $(window).on('resize.dropdown', function () {
        if ($menu) closeMenu();
    });

    $(document).on('click', '.status-dropdown .dropdown-toggle', function (e) {
        e.stopPropagation();
        const $btn = $(this);
        if ($btn.hasClass("disabled") || $btn.is(":disabled")) return;

        const optsData = $btn.data('options');
        const options = Array.isArray(optsData) ? optsData : JSON.parse(optsData);
        const currentText = $.trim($btn.text());

        if ($menu) closeMenu();
        $currentBtn = $btn;

        $menu = $('<div class="dropdown-menu"><ul></ul></div>');
        const $ul = $menu.find('ul');

        options.forEach(opt => {
            $('<li>')
                .text(opt.text)
                .addClass('status-option')
                .attr('data-value', opt.value)
                .toggleClass('active', opt.text === currentText)
                .appendTo($ul);
        });

        const $actions = $('<div class="actions"></div>');
        $('<button class="cancel">Bỏ qua</button>').appendTo($actions);
        $('<button class="confirm">Xác nhận</button>').appendTo($actions);
        $menu.append($actions);
        
        // --- 🔧 Tính toán vị trí Absolute Portal ---
        $menu.css({
            position: 'absolute',
            display: 'block',
            visibility: 'hidden',
            zIndex: 999999
        }).appendTo('body');

        const offset = $btn.offset();
        const btnHeight = $btn.outerHeight();
        const menuHeight = $menu.outerHeight() || 250;
        const menuWidth = $menu.outerWidth();
        const winHeight = $(window).height();
        const scrollByWindow = $(window).scrollTop();
        const winWidth = $(window).width();
        const paddingScreen = 20;

        let cssTop = offset.top + btnHeight;
        let cssLeft = offset.left;

        if (offset.left + menuWidth + paddingScreen > winWidth) {
            cssLeft = offset.left + $btn.outerWidth() - menuWidth;
        }

        // Tọa độ tương đối so với viewport để tính Drop-up
        const rectTop = offset.top - scrollByWindow;
        const spaceBelow = winHeight - (rectTop + btnHeight);
        const spaceAbove = rectTop;

        if (spaceBelow < menuHeight + paddingScreen && spaceAbove > spaceBelow) {
            cssTop = offset.top - menuHeight;
            $menu.addClass('drop-up');
        } else {
            $menu.removeClass('drop-up');
        }

        $menu.css({
            top: cssTop,
            left: cssLeft,
            visibility: 'visible' 
        });

        // 🕵️ Lắng nghe cuộn từ tất cả các thẻ cha để đóng menu ngay khi cuộn
        $btn.parents().on('scroll.dropdown', function () {
            closeMenu();
        });
        $(window).on('scroll.dropdown', function () {
            closeMenu();
        });
    });

    // Click chọn item
    $(document).on('click', '.dropdown-menu li', function (e) {
        e.stopPropagation();
        $(this).closest('ul').find('li').removeClass('active');
        $(this).addClass('active');
    });

    // Bỏ qua
    $(document).on('click', '.dropdown-menu .actions .cancel', function (e) {
        e.stopPropagation();
        closeMenu();
    });

    // ✅ Xác nhận – đổi text + class cho button
    $(document).on('click', '.dropdown-menu .actions .confirm', function (e) {
        e.stopPropagation();
        if ($menu && $currentBtn) {
            const $active = $menu.find('li.active');
            if ($active.length) {
                const text = $active.text();
                const val_TT = $active.attr('data-value');
                const $row = $currentBtn.closest('tr');
                let id_row = 0;
                if ($row.length) {
                    const classAttr = $row.attr('class');
                    const match = classAttr.match(/CartoFactory_(\d+)/);
                    if (match && match[1]) {
                        id_row = match[1];
                    }
                }
                var bv_Note = $row.find(".BV_Note").val();
                const cls = $active.attr('class').split(/\s+/)
                    .filter(c => c !== 'active')[0] || '';

                var type = $currentBtn.attr('data-type');
                if (type == '1') {
                    _cartofactory.UpdateStatus(id_row, val_TT, 10);
                    $currentBtn
                        .text(text)
                        .removeClass(function (_, old) {
                            return (old.match(/(^|\s)status-\S+/g) || []).join(' ');
                        }) // xoá các class status- cũ
                        .addClass(cls); // gắn class mới (status-arrived, status-blank…)
                } else {
                    var Status_type = _cartofactory.UpdateStatus(id_row, val_TT, 1, bv_Note);
                    if (Status_type == 0) {
                        $currentBtn
                            .text(text)
                            .removeClass(function (_, old) {
                                return (old.match(/(^|\s)status-\S+/g) || []).join(' ');
                            }) // xoá các class status- cũ
                            .addClass(cls); // gắn class mới (status-arrived, status-blank…)


                        if (val_TT == 1) {
                            $('#dataBody-1').find('.CartoFactory_' + id_row).remove();

                        } else {
                            $('#dataBody-0').find('.CartoFactory_' + id_row).remove();
                        }
                    }
                }
                
                
            }
        }
        closeMenu();
    });

    // Đóng menu khi click ra ngoài
    $(document).on('click', function () {
        closeMenu();
    });

    function closeMenu() {
        if ($menu) {
            $menu.remove();
            $menu = null;
            if ($currentBtn) $currentBtn.parents().off('scroll.dropdown');
            $(window).off('scroll.dropdown');
            $currentBtn = null;
        }
    }

    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/CarHub", { transport: signalR.HttpTransportType.WebSockets, skipNegotiation: true })
        .withAutomaticReconnect([ 2000, 5000, 10000])
        .build();
    connection.start()
        .then(() => console.log("✅ SignalR connected"))
        .catch(err => console.error(err));
    const AllCode = [
        { Description: "Blank", CodeValue: "1" },
        { Description: "Đã đến nhà máy", CodeValue: "0" },
        { Description: "Xe rác", CodeValue: "2" },
        // Add more objects as needed
    ];
    const AllCode2 = [
        { Description: "Blank", CodeValue: "1" },
        { Description: "Đã xử lý", CodeValue: "0" },
        // Add more objects as needed
    ];
    // Create a new array of objects in the desired format
    const options = AllCode.map(allcode => ({
        text: allcode.Description,
        value: allcode.CodeValue
    }));
    const options2 = AllCode2.map(allcode2 => ({
        text: allcode2.Description,
        value: allcode2.CodeValue
    }));
    const jsonString = JSON.stringify(options);
    const jsonString2 = JSON.stringify(options2);
    // Hàm render row
    function renderRow(item) {
        var date = new Date(item.registerDateOnline);
        let formatted =
            String(date.getHours()).padStart(2, '0') + ":" +
            String(date.getMinutes()).padStart(2, '0') + " " +
            String(date.getDate()).padStart(2, '0') + "/" +
            String(date.getMonth() + 1).padStart(2, '0') + "/" +
            date.getFullYear();
        var html_tt = ``;
        switch (item.trangThai) {
            case 1:
                html_tt ='<span class="badge badge-warning">Chưa có</span>'
                break;
            case 2:
                html_tt = `<span class="badge badge-success">Còn hạn</span>`
                break;
            case 3:
                html_tt = `<span class="badge badge-warning">Sắp hết hạn</span>`
                break;
            case 4:
                html_tt ='<span class="badge badge-danger">Hết hạn</span>'
                break;

        }
        return `
        <tr class="CartoFactory_${item.id}" data-queue="${formatted}" >
            <td class="pin pin-stt">${item.recordNumber}</td>
            <td class="pin pin-time">${formatted}</td>
            <td class="pin pin-plate"><b>${item.vehicleNumber}</b></td>
            <td class="name-td text-left">
                <div style="white-space: pre-line;">${item.customerName}</div>
            </td>
            <td>
                <div>${item.driverName}</div>
                <div>${item.phoneNumber}</div>
            </td>
            <td>${item.vehicleLoad}</td>
            <td class="obd-col">${_cartofactory.RenderObdToken(item)}</td>
            <td>${item.licenseNumber}</td>
            <td><textarea class="BV_Note" name="BV_Note">${item.protectNotes == null ? '' : item.protectNotes}</textarea></td>
            <td>${html_tt}</td>
            <td>
                <div class="status-dropdown">
                    <button class="dropdown-toggle " data-options='${jsonString}'>${item.vehicleStatusName}</button>
                </div>
            </td>
        </tr>`;
    }

    function renderRow2(item) {
        var html_tt = ``;
        switch (item.trangThai) {
            case 1:
                html_tt = '<span class="badge badge-warning">Chưa có</span>'
                break;
            case 2:
                html_tt = `<span class="badge badge-success">Còn hạn</span>`
                break;
            case 3:
                html_tt = `<span class="badge badge-warning">Sắp hết hạn</span>`
                break;
            case 4:
                html_tt = '<span class="badge badge-danger">Hết hạn</span>'
                break;

        }
        // Cột OBD bắt buộc khớp ListCartoFactory (Chưa xử lý) — thiếu sẽ lệch cột khi ReceiveRegistration
        var vehicleId = item.id != null ? item.id : (item.bookingid != null ? item.bookingid : '');
        var obdItem = { id: vehicleId, obdList: item.obdList || item.ObdList || [] };
        return `
        <tr class="CartoFactory_${vehicleId}" data-queue="${item.createTime}" >
            <td class="pin pin-stt">${item.queueNumber}</td>
            <td class="pin pin-time">${item.createTime}</td>
            <td class="pin pin-plate"><b>${item.plateNumber}</b></td>
            <td class="name-td text-left">
                <div style="white-space: pre-line;">${item.name}</div>
            </td>
            <td>
                <div>${item.gplx}</div>
                <div>${item.phoneNumber}</div>
            </td>
            <td>${item.referee}</td>
            <td class="obd-col">${_cartofactory.RenderObdToken(obdItem)}</td>
            <td>${item.camp}</td>
            <td><textarea class="BV_Note" name="BV_Note"></textarea></td>
            <td>${html_tt}</td>
            <td>
                <div class="status-dropdown">
                    <button class="dropdown-toggle " data-options='${jsonString}'>Blank</button>
                </div>
            </td>
        </tr>`;
    }
    function renderRow3(item) {
        var date = new Date(item.vehicleArrivalDate);
        let formatted =
            String(date.getHours()).padStart(2, '0') + ":" +
            String(date.getMinutes()).padStart(2, '0') + " " +
            String(date.getDate()).padStart(2, '0') + "/" +
            String(date.getMonth() + 1).padStart(2, '0') + "/" +
            date.getFullYear();
        var html_tt = ``;
        switch (item.trangThai) {
            case 1:
                html_tt = '<span class="badge badge-warning">Chưa có</span>'
                break;
            case 2:
                html_tt = `<span class="badge badge-success">Còn hạn</span>`
                break;
            case 3:
                html_tt = `<span class="badge badge-warning">Sắp hết hạn</span>`
                break;
            case 4:
                html_tt = '<span class="badge badge-danger">Hết hạn</span>'
                break;

        }
        return `
        <tr class="CartoFactory_${item.id}" data-queue="${formatted}" >
            <td class="pin pin-stt">${item.recordNumber}</td>
            <td class="pin pin-time">${formatted}</td>
            <td class="pin pin-plate"><b>${item.vehicleNumber}</b></td>
            <td class="name-td text-left">
                <div style="white-space: pre-line;">${item.customerName}</div>
            </td>
            <td>
                <div>${item.driverName}</div>
                <div>${item.phoneNumber}</div>
            </td>
            <td>${item.vehicleLoad}</td>
            <td>${item.licenseNumber}</td>
            <td>${item.protectNotes == null ? '' : item.protectNotes}</td>
            <td>${html_tt}</td>
            <td>
                <div class="status-dropdown">
                    <button class="dropdown-toggle " data-options='${jsonString}'>${item.vehicleStatusName}</button>
                </div>
            </td>
        </tr>`;
    }
    // Hàm sắp xếp lại tbody theo QueueNumber tăng dần
    function sortTable_Da_SL() {
        const tbody = document.getElementById("dataBody-1");
        const rows = Array.from(tbody.querySelectorAll("tr"));

        rows.sort((a, b) => {
            const timeA = parseDateTime(a.dataset.queue);
            const timeB = parseDateTime(b.dataset.queue);
            return timeA - timeB;
        });

        tbody.innerHTML = "";
        rows.forEach(r => tbody.appendChild(r));
    }
    function sortTable() {
        const tbody = document.getElementById("dataBody-0");
        if (!tbody) return;
        const rows = Array.from(tbody.querySelectorAll("tr"));

        // Giờ đăng ký online mới nhất lên đầu
        rows.sort((a, b) => {
            const timeA = parseDateTime(a.dataset.queue);
            const timeB = parseDateTime(b.dataset.queue);
            return timeB - timeA;
        });

        tbody.innerHTML = "";
        rows.forEach(r => tbody.appendChild(r));
    }

    // Nhận data mới từ server
    connection.off("ListCartoFactory_Da_SL");
    connection.on("ListCartoFactory_Da_SL", function (item) {
        const tbody = document.getElementById("dataBody-1");
        $('.CartoFactory_' + item.id).remove();
        tbody.insertAdjacentHTML("beforeend", renderRow3(item));
        sortTable_Da_SL(); // sắp xếp lại ngay khi thêm
    });
    connection.off("ListCartoFactory");
    connection.on("ListCartoFactory", function (item) {
        const tbody = document.getElementById("dataBody-0");
        $('.CartoFactory_' + item.id).remove();
        tbody.insertAdjacentHTML("beforeend", renderRow(item));
        sortTable(); // sắp xếp lại ngay khi thêm
    });
    connection.off("ListCartoFactory_rac");
    connection.on("ListCartoFactory_rac", function (item) {
        const tbody = document.getElementById("dataBody-2");
        $('.CartoFactory_' + item.id).remove();
        tbody.insertAdjacentHTML("beforeend", renderRow(item));
        sortTable(); // sắp xếp lại ngay khi thêm
    });
    connection.off("ReceiveRegistration");
    connection.on("ReceiveRegistration", function (item) {
        // Sau cut-off hôm nay → không hiện trên CartoFactory (chỉ hiện từ 00:00 ngày sau qua SQL)
        if (isRegistrationAfterCutoff(item)) {
            return;
        }
        const tbody = document.getElementById("dataBody-0");
        tbody.insertAdjacentHTML("beforeend", renderRow2(item));
        sortTable(); // sắp xếp lại ngay khi thêm
    });

    function isRegistrationAfterCutoff(item) {
        try {
            var cutoff = (window.CART_CUTOFF_TIME || '17:55').split(':');
            var ch = parseInt(cutoff[0], 10) || 17;
            var cm = parseInt(cutoff[1], 10) || 55;
            var now = new Date();
            var cutoffToday = new Date(now.getFullYear(), now.getMonth(), now.getDate(), ch, cm, 0);
            var reg = null;
            if (item.registrationTime) {
                reg = new Date(item.registrationTime);
            } else if (item.createTime) {
                // "HH:mm dd/MM/yyyy"
                var p = String(item.createTime).trim().split(/[\s/:]+/);
                if (p.length >= 5) {
                    reg = new Date(parseInt(p[4], 10), parseInt(p[3], 10) - 1, parseInt(p[2], 10), parseInt(p[0], 10), parseInt(p[1], 10), 0);
                }
            }
            if (!reg || isNaN(reg.getTime())) return false;
            return reg >= cutoffToday;
        } catch (e) {
            return false;
        }
    }
    connection.off("ListProcessingIsLoading_Da_SL");
    connection.on("ListProcessingIsLoading_Da_SL", function (item) {
        $('.CartoFactory_' + item.id).remove();
    });
    connection.off("ListProcessingIsLoading");
    connection.on("ListProcessingIsLoading", function (item) {
        const tbody = document.getElementById("dataBody-1");
        tbody.insertAdjacentHTML("beforeend", renderRow3(item));
        sortTable_Da_SL(); // sắp xếp lại ngay khi thêm
    });
    //connection.off("ProcessingIsLoading_khoa");
    //connection.on("ProcessingIsLoading_khoa", function (item) {
    //    const tbody = document.getElementById("dataBody-0");
    //    $('.CartoFactory_' + item.id).remove();
    //    tbody.insertAdjacentHTML("beforeend", renderRow(item));
    //    sortTable(); // sắp xếp lại ngay khi thêm
    //});
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
    }
    // 🕵️ Lắng nghe scroll trên toàn bộ trang để đóng menu (tránh bị lệch)
    window.addEventListener('scroll', function () {
        closeMenu();
    }, true);

    // Đóng menu khi click ra ngoài
    $(document).on('click', function () {
        closeMenu();
    });

    function closeMenu() {
        if ($menu) {
            $menu.remove();
            $menu = null;
            $currentBtn = null;
        }
    }
});
var _cartofactory = {

    init: function () {
        _cartofactory.ListCartoFactory();
        _cartofactory.ListCartoFactory_Da_SL();
        _cartofactory.ListCartoFactory_rac();
    },
    ListCartoFactory: function () {
        var model = {
            VehicleNumber: $('#input_chua_xu_ly').val() != undefined && $('#input_chua_xu_ly').val() != "" ? $('#input_chua_xu_ly').val().trim() : "",
            PhoneNumber: $('#input_chua_xu_ly').val() != undefined && $('#input_chua_xu_ly').val() != "" ? $('#input_chua_xu_ly').val().trim() : "",
            VehicleStatus: null,
            LoadType: null,
            VehicleWeighingType: null,
            VehicleTroughStatus: null,
            TroughType: null,
            VehicleWeighingStatus: null,
            type: 0,
        }
        $.ajax({
            url: "/Car/ListCartoFactory",
            type: "post",
            data: { SearchModel: model },
            success: function (result) {
                $('#imgLoading').hide();
                $('#data_chua_xu_ly').html(result);
                $('#data_chua_xu_ly .obd-token').each(function () {
                    _cartofactory.SyncObdTotal($(this));
                });
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                console.log("Status: " + textStatus);
            }
        });
    },
    ListCartoFactory_Da_SL: function () {
        var model = {
            VehicleNumber: $('#input_da_xu_ly').val() != undefined && $('#input_da_xu_ly').val() != "" ? $('#input_da_xu_ly').val().trim() : "",
            PhoneNumber: $('#input_da_xu_ly').val() != undefined && $('#input_da_xu_ly').val() != "" ? $('#input_da_xu_ly').val().trim() : "",
            VehicleStatus: 0,
            LoadType: null,
            VehicleWeighingType: null,
            VehicleTroughStatus: null,
            TroughType: null,
            VehicleWeighingStatus: null,
            type: 1,
        }
        $.ajax({
            url: "/Car/ListCartoFactory",
            type: "post",
            data: { SearchModel: model },
            success: function (result) {
                $('#imgLoading').hide();
                $('#data_da_xu_ly').html(result);
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                console.log("Status: " + textStatus);
            }
        });
    },
    ListCartoFactory_rac: function () {
        var model = {
            VehicleNumber:  "",
            PhoneNumber:  "",
            VehicleStatus: 2,
            LoadType: null,
            VehicleWeighingType: null,
            VehicleTroughStatus: null,
            TroughType: null,
            VehicleWeighingStatus: null,
            type: 2,
        }
        $.ajax({
            url: "/Car/ListCartoFactory",
            type: "post",
            data: { SearchModel: model },
            success: function (result) {
                $('#imgLoading').hide();
                $('#data_rac').html(result);
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                console.log("Status: " + textStatus);
            }
        });
    },
    OpenPopup: function (id) {
        let title = 'Cập nhật trạng thái';
        let url = '/Car/OpenPopup';
        let param = {
            id: id,
            type: 1
        };

        _magnific.OpenSmallPopup(title, url, param);

    },
    UpdateStatus: function (id, status, type, Note) {
       var status_type=0
        $.ajax({
            url: "/Car/UpdateStatus",
            type: "post",
            data: { id: id, status: status, type: type, weight: 0, Note: Note },
            success: function (result) {
                status_type = result.status;
                if (result.status == 0) {
                    _msgalert.success(result.msg)
                    $.magnificPopup.close();
                } else {
                    _msgalert.error(result.msg)
                }
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                console.log("Status: " + textStatus);
            }
        });
        return status_type;
    },
    Export: function () {
        $('#btnExport').prop('disabled', true);
       
        _global_function.AddLoading()
        $.ajax({
            url: "/Car/ExportExcel",
            type: "Post",
            data: {},
            success: function (result) {
                _global_function.RemoveLoading()
                $('#btnExport').prop('disabled', false);
                if (result.isSuccess) {
                    _msgalert.success(result.message);
                    window.location.href = result.path;
                } else {
                    _msgalert.error(result.message);
                }
                $('#icon-export').removeClass('fa-spinner fa-pulse');
                $('#icon-export').addClass('fa-file-excel-o');
            }
        });
    },

    // OBD widget → shared _obdToken (obd-token.js)
    FormatObdChip: function (obdId, delivery, netWeight) { return _obdToken.formatChip(obdId, delivery, netWeight); },
    ParseNetWeight: function (text) { return _obdToken.parseNetWeight(text); },
    FormatNetWeightVi: function (value) { return _obdToken.formatNetWeightVi(value); },
    SyncObdTotal: function ($wrap) { _obdToken.syncTotal($wrap); },
    RenderObdToken: function (item) { return _obdToken.render(item); },
    BindObdTokenEvents: function () { _obdToken.bindEvents(); },
}