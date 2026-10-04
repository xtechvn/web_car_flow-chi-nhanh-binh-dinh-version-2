$(document).ready(function () {
    var input_chua_xu_ly = document.getElementById("input_chua_xu_ly");
     input_chua_xu_ly.addEventListener("keyup", function (event) {
         _registeredvehicle.loaddata();
    });
    _registeredvehicle.loaddata()

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

    $(document).on('click', '.dropdown-menu li', function (e) {
        e.stopPropagation();
        $(this).closest('ul').find('li').removeClass('active');
        $(this).addClass('active');
    });

    $(document).on('click', '.dropdown-menu .actions .cancel', function (e) {
        e.stopPropagation();
        closeMenu();
    });

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
               
                $.ajax({
                    url: "/Car/UpdateRegisteredVehicle",
                    type: "post",
                    data: { id: id_row, status: val_TT, note: bv_Note },
                    success: function (result) {
                        status_type = result.status;
                        if (result.status == 0) {
                            _msgalert.success(result.msg)
                            setTimeout(() => {
                                window.location.reload();
                            }, 1000);
                        } 
                    },
                    error: function (XMLHttpRequest, textStatus, errorThrown) {
                        console.log("Status: " + textStatus);
                    }
                });
                


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

    const AllCode = [
        { Description: "Blank", CodeValue: "1" },
        { Description: "Đã đến nhà máy", CodeValue: "0" },
        // Add more objects as needed
    ];

    // Create a new array of objects in the desired format
    const options = AllCode.map(allcode => ({
        text: allcode.Description,
        value: allcode.CodeValue
    }));

    const jsonString = JSON.stringify(options);
    function renderRow(item) {
      
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
        <tr class="CartoFactory_${item.id}" data-queue="${item.createTime}" >
            <td>${item.queueNumber}</td>
            <td>${item.createTime}</td>
            <td class="name-td">${item.name}</td>
            <td>
                <div>${item.gplx}</div>
                <div>${item.phoneNumber}</div>
            </td>
            <td>${item.plateNumber}</td>
            <td>${item.referee}</td>
            <td>${item.camp}</td>
            <td><textarea class="BV_Note" name="BV_Note" ></textarea></td>
            <td>
               ${html_tt}

            </td>
            <td>
                <div class="status-dropdown">
                    <button class="dropdown-toggle " data-options='${jsonString}'>
                        Blank
                    </button>
                </div>

            </td>

        </tr>`;
    }

    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/CarHub", { transport: signalR.HttpTransportType.WebSockets, skipNegotiation: true })
        .withAutomaticReconnect([2000, 5000, 10000])
        .build();
    connection.start()
        .then(() => console.log("✅ SignalR connected"))
        .catch(err => console.error(err));
    // Nhận data mới từ server
    connection.off("ReceiveRegistration_DK");
    connection.on("ReceiveRegistration_DK", function (item) {
        const tbody = document.getElementById("dataBody-1");
        tbody.insertAdjacentHTML("beforeend", renderRow(item));
      
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
});
var _registeredvehicle = {

    loaddata: function () {
        var model = {
            VehicleNumber: $('#input_chua_xu_ly').val() != undefined && $('#input_chua_xu_ly').val() != "" ? $('#input_chua_xu_ly').val().trim() : "", 
            PhoneNumber: $('#input_chua_xu_ly').val() != undefined && $('#input_chua_xu_ly').val() != "" ? $('#input_chua_xu_ly').val().trim() : "", 
            VehicleStatus: 0,
            LoadType: null,
            VehicleWeighingType: null,
            VehicleTroughStatus: null,
            TroughType: null,
            VehicleWeighingStatus: null,
            type: 1,
        }
        $.ajax({
            url: "/Car/ListRegisteredVehicle",
            type: "post",
            data: { SearchModel: model },
            success: function (result) {
                $('#imgLoading').hide();
                $('#data_chua_xu_ly').html(result);
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                console.log("Status: " + textStatus);
            }
        });
    }

}