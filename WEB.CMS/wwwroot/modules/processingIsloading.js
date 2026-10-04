$(document).ready(function () {
    _processing_is_loading.init();
    var input_Processing_Is_Loading_Chua_SL = document.getElementById("input_Processing_Is_Loading_Chua_SL");
    input_Processing_Is_Loading_Chua_SL.addEventListener("keypress", function (event) {
        // If the user presses the "Enter" key on the keyboard
        if (event.key === "Enter") {
            // Cancel the default action, if needed
            event.preventDefault();
            // Trigger the button element with a click
            _processing_is_loading.ListProcessingIsLoading();
        }
    });
    input_Processing_Is_Loading_Chua_SL.addEventListener("keyup", function (event) {
        _processing_is_loading.ListProcessingIsLoading();
    });
    var input_Processing_Is_Loading_Da_SL = document.getElementById("input_Processing_Is_Loading_Da_SL");
    input_Processing_Is_Loading_Da_SL.addEventListener("keypress", function (event) {
        // If the user presses the "Enter" key on the keyboard
        if (event.key === "Enter") {
            // Cancel the default action, if needed
            event.preventDefault();
            // Trigger the button element with a click
            _processing_is_loading.ListProcessingIsLoading_Da_SL();
        }
    });
    input_Processing_Is_Loading_Da_SL.addEventListener("keyup", function (event) {
        _processing_is_loading.ListProcessingIsLoading_Da_SL();
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
                .addClass(opt.class) // Giữ nguyên class từ opt
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

                const cls = $active.attr('class').split(/\s+/)
                    .filter(c => c !== 'active')[0] || '';

                var type = $currentBtn.attr('data-type');
                // Lấy vehicleloadtaken dùng chung
                var vehicleloadtaken = $currentBtn.closest('tr').find('.VehicleLoadTaken').val();
                if (vehicleloadtaken !== undefined && vehicleloadtaken !== "") {
                    vehicleloadtaken = vehicleloadtaken.replaceAll(",", "");
                } else {
                    vehicleloadtaken = 0;
                }

                switch (type) {
                    case '2':
                        _processing_is_loading.UpdateStatus(id_row, val_TT, 10, vehicleloadtaken);
                        $currentBtn
                            .text(text)
                            .removeClass(function (_, old) {
                                return (old.match(/(^|\s)status-\S+/g) || []).join(' ');
                            })
                            .addClass(cls);
                        break;

                    case '3':
                        _processing_is_loading.UpdateStatus(id_row, val_TT, 11, vehicleloadtaken);
                        $currentBtn
                            .text(text)
                            .removeClass(function (_, old) {
                                return (old.match(/(^|\s)status-\S+/g) || []).join(' ');
                            })
                            .addClass(cls);
                        break;

                    case '1':
                        _processing_is_loading.UpdateStatus(id_row, val_TT, 2, vehicleloadtaken);
                        $currentBtn
                            .text(text)
                            .removeClass(function (_, old) {
                                return (old.match(/(^|\s)status-\S+/g) || []).join(' ');
                            })
                            .addClass(cls);
                        break;

                    default:
                        var Status_type = _processing_is_loading.UpdateStatus(id_row, val_TT, 8, vehicleloadtaken);
                        if (Status_type == 0) {
                            $currentBtn
                                .text(text)
                                .removeClass(function (_, old) {
                                    return (old.match(/(^|\s)status-\S+/g) || []).join(' ');
                                })
                                .addClass(cls);

                            if (val_TT == 1) {
                                $('#dataBody-1').find('.CartoFactory_' + id_row).remove();
                            } else {
                                $('#dataBody-0').find('.CartoFactory_' + id_row).remove();
                            }
                        }
                        break;
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
        { Description: "Thường", CodeValue: "1" },
        { Description: "Xanh", CodeValue: "0" },
        { Description: "Đỏ", CodeValue: "2" },
    ];
    const AllCode2 = [
        { Description: "Blank", CodeValue: "1" },
        { Description: "Hoàn thành", CodeValue: "0" },
    ];
    const AllCode3 = [
        { Description: "Chưa dặt hàng", CodeValue: "0" },
        { Description: "Chưa chuyển tiền", CodeValue: "1" },
        { Description: "Quá tải", CodeValue: "2" },
    ];
    const AllCode4 = [
        { Description: "Bạc", CodeValue: "0" },
        { Description: "Vàng", CodeValue: "1" },
        { Description: "Kim cương", CodeValue: "2" },
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

    const options3 = AllCode3.map(AllCode3 => ({
        text: AllCode3.Description,
        value: AllCode3.CodeValue
    }));
    const options4 = AllCode4.map(AllCode4 => ({
        text: AllCode4.Description,
        value: AllCode4.CodeValue
    }));
    const jsonString = JSON.stringify(options);
    const jsonString2 = JSON.stringify(options2);
    const jsonString3 = JSON.stringify(options3);
    const jsonString4 = JSON.stringify(options4);
    // Hàm render row
    function renderRow(item) {
        var date = new Date(item.vehicleArrivalDate);
        let formatted =
            String(date.getHours()).padStart(2, '0') + ":" +
            String(date.getMinutes()).padStart(2, '0') + " " +
            String(date.getDate()).padStart(2, '0') + "/" +
            String(date.getMonth() + 1).padStart(2, '0') + "/" +
            date.getFullYear();
        var date2 = new Date(item.registerDateOnline);
        let formatted2 =
            String(date2.getHours()).padStart(2, '0') + ":" +
            String(date2.getMinutes()).padStart(2, '0') + " " +
            String(date2.getDate()).padStart(2, '0') + "/" +
            String(date2.getMonth() + 1).padStart(2, '0') + "/" +
            date2.getFullYear();
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
        var html = `     <a class="cursor-pointer" onclick="_processing_is_loading.AddOrUpdateVehicleNumber(${item.id})" title="Chỉnh sửa biển số xe">
                                        <i class="icon-edit"></i>
                                    </a>`
        var html_input = `<div class="">
                                            <p class="">
                                                <input type="text"
                                                       class="input-form VehicleLoadTaken"
                                                       maxlength="8"
                                                       oninput="this.value = this.value.replace(/[^0-9]/g, '').slice(0,8);"
                                                       placeholder="Vui lòng nhập trọng lượng thực tế"value="${item.vehicleLoadTaken == null ? 0 : item.vehicleLoadTaken.toLocaleString('en-US') }" />

                                            </p>
                                        </div>`

        return `
        <tr class="CartoFactory_${item.id}" data-queue="${formatted}"  >
            <td>${item.recordNumber}</td>
            <td>${formatted}</td>
            <td>${item.vehicleNumber} ${item.trangThai == 1 || item.trangThai == 2 ? html : ""}</td>
            <td>
            <div style="display: flex; align-items: center">
                 <div style="white-space: pre-line;">
                      ${item.customerName} 
                 </div>
            </div>
            <a class="cursor-pointer" style="margin-left:10px;" onclick="_processing_is_loading.AddOrUpdateNamePopup(${item.id})" title="Chỉnh sửa biển số khách hàng">
                                                    <i class="icon-edit"></i>
                                                </a>
            </td>
            <td>
                <div>${item.driverName}</div>
                <div>${item.phoneNumber}</div>
            </td>
           
           
          
            <td>${item.licenseNumber}</td>
            <td>${item.protectNotes == null ? '' : item.protectNotes}</td>
            <td> <textarea  class="CS_Note" name="CS_Note" value="${item.csNotes == null ? '' : item.csNotes}">${item.csNotes == null ? '' : item.csNotes}</textarea>  </td>
            <td>${html_tt}
                <a class="cursor-pointer" style="margin-left:10px;" onclick="_processing_is_loading.AddOrUpdateNamePopup(${item.id})" title="Chỉnh sửa"><i class="icon-edit"></i>
                </a>
            </td>
            <td>${item.vehicleWeightMax.toLocaleString('en-US') }</td>
            <td>${html_input} </td>
         
              <td>
                  ${item.rankName == null ? '' : item.rankName}
            </td>
            <td>
                <div class="status-dropdown">
                    <button class="dropdown-toggle "  data-type="1" data-options='${jsonString}'>
                        ${item.loadTypeName == null ? '' : item.loadTypeName}
                    </button>
                </div>

            </td>
            <td>
                <div class="status-dropdown">
                    <button class="dropdown-toggle " data-options='${jsonString2}'>
                        ${item.loadingStatusName == null ? '' : item.loadingStatusName}
                    </button>
                </div>

            </td>

        </tr>`;
    }
    function renderRow_DA_SL(item) {
        var date = new Date(item.vehicleArrivalDate);
        let formatted =
            String(date.getHours()).padStart(2, '0') + ":" +
            String(date.getMinutes()).padStart(2, '0') + " " +
            String(date.getDate()).padStart(2, '0') + "/" +
            String(date.getMonth() + 1).padStart(2, '0') + "/" +
            date.getFullYear();
        var date2 = new Date(item.registerDateOnline);
        let formatted2 =
            String(date2.getHours()).padStart(2, '0') + ":" +
            String(date2.getMinutes()).padStart(2, '0') + " " +
            String(date2.getDate()).padStart(2, '0') + "/" +
            String(date2.getMonth() + 1).padStart(2, '0') + "/" +
            date2.getFullYear();
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
            <td>${item.recordNumber}</td>
            <td>${formatted}</td>
            <td>${item.vehicleNumber}</td>
            <td>
            <div style="display: flex; align-items: center">
                 <div style="white-space: pre-line;">
                      ${item.customerName} 
                 </div>
            </div>
            </td>
            <td>
                  <div>${item.driverName}</div>
                <div>${item.phoneNumber}</div>
            </td>
        
         
            <td>${item.licenseNumber}</td>
            <td>${item.protectNotes == null ? '' : item.protectNotes}</td>
            <td>${item.csNotes == null ? '' : item.csNotes}</td>
            <td>${html_tt}</td>
            <td>${item.vehicleWeightMax.toLocaleString('en-US')}</td>
            <td>${item.vehicleLoadTaken == null ? 0 : item.vehicleLoadTaken.toLocaleString('en-US') }</td>
          
            <td>
                ${item.rankName == null ? '' : item.rankName}
            </td>
            <td>
                <div class="">
                    <p class=" " >
                        ${item.loadTypeName == null ? '' : item.loadTypeName}
                    </p>
                </div>

            </td>
            <td>
                <div class="status-dropdown">
                    <button class="dropdown-toggle " data-options='${jsonString2}'>
                        ${item.loadingStatusName == null ? '' : item.loadingStatusName}
                    </button>
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
            return timeA - timeB; // tăng dần
        });

        tbody.innerHTML = "";
        rows.forEach(r => tbody.appendChild(r));
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

    // Nhận data mới từ server
    connection.off("ListProcessingIsLoading_Da_SL");
    connection.on("ListProcessingIsLoading_Da_SL", function (item) {
        const tbody = document.getElementById("dataBody-1");
        $('.CartoFactory_' + item.id).remove();
        tbody.insertAdjacentHTML("beforeend", renderRow_DA_SL(item));
        sortTable_Da_SL(); // sắp xếp lại ngay khi thêm
    });
    connection.off("ListProcessingIsLoading");
    connection.on("ListProcessingIsLoading", function (item) {
        const tbody = document.getElementById("dataBody-0");
        $('.CartoFactory_' + item.id).remove();
        tbody.insertAdjacentHTML("beforeend", renderRow(item));
        sortTable(); // sắp xếp lại ngay khi thêm
    });
    //lấy từ ds xe đến nhà máy
    connection.off("ListCartoFactory_Da_SL");
    connection.on("ListCartoFactory_Da_SL", function (item) {
        const tbody = document.getElementById("dataBody-0");
        tbody.insertAdjacentHTML("beforeend", renderRow(item));
        sortTable_Da_SL(); // sắp xếp lại ngay khi thêm
    });
    connection.off("ListCartoFactory");
    connection.on("ListCartoFactory", function (item) {
        $('#dataBody-0').find('.CartoFactory_' + item.id).remove();
    });
    connection.off("ListCallTheScale_Da_SL");
    connection.on("ListCallTheScale_Da_SL", function (item) {
        $('#dataBody-0').find('.CartoFactory_' + item.id).remove();
    });
    connection.off("ListCallTheScale_0");
    connection.on("ListCallTheScale_0", function (item) {
        const tbody = document.getElementById("dataBody-1");
        tbody.insertAdjacentHTML("beforeend", renderRow_DA_SL(item));
        sortTable_Da_SL(); // sắp xếp lại ngay khi thêm
    });
    connection.off("ListCallTheScale_1");
    connection.on("ListCallTheScale_1", function (item) {
        const tbody = document.getElementById("dataBody-1");
        tbody.insertAdjacentHTML("beforeend", renderRow_DA_SL(item));
        sortTable_Da_SL(); // sắp xếp lại ngay khi thêm
    });
    connection.off("ProcessingIsLoading_khoa");
    connection.on("ProcessingIsLoading_khoa", function (item) {
        const tbody = document.getElementById("dataBody-0");
        $('.CartoFactory_' + item.id).remove();
        tbody.insertAdjacentHTML("beforeend", renderRow(item));
        sortTable(); // sắp xếp lại ngay khi thêm
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
    $(document).on('click', '.check-VehicleLoadTaken', function (e) {
        var element_btn = $(this);
        var class_tr = element_btn.closest('tr').attr('class');
        var id = 0;
        var match_tr = class_tr.match(/CartoFactory_(\d+)/);
        if (match_tr && match_tr[1]) {
            id = match_tr[1];
        }
        var vehicleloadtaken = element_btn.closest('td').find('.VehicleLoadTaken').val();
        if (vehicleloadtaken != "") {
            vehicleloadtaken = vehicleloadtaken.replaceAll(",","")
        }
        _processing_is_loading.UpdateVehicleLoad(id, vehicleloadtaken)
    });
    $(document).on('blur', '.CS_Note', function () {
        var $textarea = $(this);
        var note = $textarea.val();
        var $row = $textarea.closest('tr');
        var classList = $row.attr('class') || '';
        var match = classList.match(/CartoFactory_(\d+)/);
        if (!match) return;
        var id = match[1];
        $.ajax({
            url: "/Car/UpdateStatus",
            type: "post",
            data: { id: id, status: 0, type: 12, weight: 0, Note: note },
            success: function (result) {
                if (result.status == 0) {
                    _msgalert.success(result.msg);
                } else {
                    _msgalert.error(result.msg);
                }
            },
            error: function () {
                _msgalert.error("Lỗi kết nối");
            }
        });
    });
    function parseDateTime(str) {
        // "11:33 17/12/2025"
        const [time, date] = str.split(" ");
        const [hour, minute] = time.split(":").map(Number);
        const [day, month, year] = date.split("/").map(Number);

        return new Date(year, month - 1, day, hour, minute).getTime();
    }
});
var _processing_is_loading = {
    init: function () {
        _processing_is_loading.ListProcessingIsLoading();
        _processing_is_loading.ListProcessingIsLoading_Da_SL();
    },
    ListProcessingIsLoading: function () {
        var model = {
            VehicleNumber: $('#input_Processing_Is_Loading_Chua_SL').val() != undefined && $('#input_Processing_Is_Loading_Chua_SL').val() != "" ? $('#input_Processing_Is_Loading_Chua_SL').val().trim() : "",
            PhoneNumber: $('#input_Processing_Is_Loading_Chua_SL').val() != undefined && $('#input_Processing_Is_Loading_Chua_SL').val() != "" ? $('#input_Processing_Is_Loading_Chua_SL').val().trim() : "",
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
            url: "/Car/ListProcessingIsLoading",
            type: "post",
            data: { SearchModel: model },
            success: function (result) {
                $('#imgLoading').hide();
                $('#Processing_Is_Loading_Chua_SL').html(result);
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                console.log("Status: " + textStatus);
            }
        });
    },
    ListProcessingIsLoading_Da_SL: function () {
        var model = {
            VehicleNumber: $('#input_Processing_Is_Loading_Da_SL').val() != undefined && $('#input_Processing_Is_Loading_Da_SL').val() != "" ? $('#input_Processing_Is_Loading_Da_SL').val().trim() : "",
            PhoneNumber: $('#input_Processing_Is_Loading_Da_SL').val() != undefined && $('#input_Processing_Is_Loading_Da_SL').val() != "" ? $('#input_Processing_Is_Loading_Da_SL').val().trim() : "",
            VehicleStatus: 0,
            LoadType: null,
            VehicleWeighingType: null,
            VehicleTroughStatus: null,
            TroughType: null,
            VehicleWeighingStatus: null,
            LoadingStatus: 0,
            type: 1,
        }
        $.ajax({
            url: "/Car/ListProcessingIsLoading",
            type: "post",
            data: { SearchModel: model },
            success: function (result) {
                $('#imgLoading').hide();
                $('#Processing_Is_Loading_Da_SL').html(result);
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                console.log("Status: " + textStatus);
            }
        });
    },
    UpdateStatus: function (id, status, type, vehicleloadtaken) {
        var status_type = 0
        $.ajax({
            url: "/Car/UpdateStatus",
            type: "post",
            data: { id: id, status: status, type: type, weight:vehicleloadtaken },
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
    AddOrUpdateName: function () {
        var id = $('#Id').val();
        var name = $('#CustomerName').val();
        var VehicleNumber = $('#VehicleNumber').val();
        var status_type = 1;
        $.ajax({
            url: "/Car/UpdateName",
            type: "post",
            data: { id: id, name: name, VehicleNumber: VehicleNumber },
            success: function (result) {
                status_type = result.status;
                if (result.status == 0) {
                    _msgalert.success(result.msg)
                    setTimeout(
                        window.location.reload()
                        , 1000);

                } else {
                    _msgalert.error(result.msg)
                }
            },

        });
        return status_type;
    },
    AddOrUpdateNamePopup: function (id) {
        let title = `Cập nhật thông tin khách hàng`;
        let url = '/Car/AddOrUpdateNamePopup';
        let param = { id: id };
        _magnific.OpenSmallPopup(title, url, param);
    },
    AddOrUpdateVehicleNumber: function (id) {
        let title = `Cập nhật biển số xe`;
        let url = '/Car/AddOrUpdateVehicleNumber';
        let param = { id: id };
        _magnific.OpenSmallPopup(title, url, param);
    },
    UpdateVehicleLoad: function (id, vehicleloadtaken) {
        var status_type = 0
        $.ajax({
            url: "/Car/UpdateVehicleLoadTaken",
            type: "post",
            data: { id: id, vehicleloadtaken: vehicleloadtaken },
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
}