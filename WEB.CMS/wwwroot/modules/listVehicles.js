$(document).ready(function () {
    _listVehicles.init();
    var input_chua_xu_ly = document.getElementById("input_chua_xu_ly");
    input_chua_xu_ly.addEventListener("keypress", function (event) {
        // If the user presses the "Enter" key on the keyboard
        if (event.key === "Enter") {
            // Cancel the default action, if needed
            event.preventDefault();
            // Trigger the button element with a click
            _listVehicles.ListCartoFactory();
        }
    });
    input_chua_xu_ly.addEventListener("keyup", function (event) {
        _listVehicles.ListCartoFactory();
    });
    var input_da_xu_ly = document.getElementById("input_da_xu_ly");
    input_da_xu_ly.addEventListener("keypress", function (event) {
        // If the user presses the "Enter" key on the keyboard
        if (event.key === "Enter") {
            // Cancel the default action, if needed
            event.preventDefault();
            // Trigger the button element with a click
            _listVehicles.ListCartoFactory_Da_SL();
        }
    });
    input_da_xu_ly.addEventListener("keyup", function (event) {
        _listVehicles.ListCartoFactory_Da_SL();
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

                var weight = $('.CartoFactory_' + id_row + '_weight').val();
                var note = "";



                const cls = $active.attr('class').split(/\s+/)
                    .filter(c => c !== 'active')[0] || '';


                $currentBtn
                    .text(text)
                    .removeClass(function (_, old) {
                        return (old.match(/(^|\s)status-\S+/g) || []).join(' ');
                    }) // xoá các class status- cũ
                    .addClass(cls); // gắn class mới (status-arrived, status-blank…)

                _listVehicles.UpdateStatus(id_row, val_TT, 7, weight, note);
                if (val_TT == 0) {
                    $('#dataBody-0').find('.CartoFactory_' + id_row).remove();

                } else {
                    $('#dataBody-1').find('.CartoFactory_' + id_row).remove();
                    $('#dataBody-0').find('.CartoFactory_' + id_row).remove();
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
        .withAutomaticReconnect([2000, 5000, 10000])
        .build();
    connection.start()
        .then(() => console.log("✅ SignalR connected"))
        .catch(err => console.error(err));

    const TroughOptions = [
        { text: "Máng 1", value: "1" },
        { text: "Máng 2", value: "2" },
        { text: "Máng 3", value: "3" },
        { text: "Máng 4", value: "4" },
        { text: "Máng 5", value: "5" },
        { text: "Máng 6", value: "6" },
        { text: "Máng 7", value: "7" },
        { text: "Máng 8", value: "8" }
    ];
    const StatusOptions = [
        { text: "Blank", value: "2" },
        { text: "Đã cân ra", value: "0" }
    ];

    const jsonTrough = JSON.stringify(TroughOptions);
    const jsonStatus = JSON.stringify(StatusOptions);
    // Hàm render row
    function renderRow(item, isProcessed) {
        var date = new Date(item.vehicleWeighingTimeComplete);
        let formatted = item.vehicleWeighingTimeComplete ?
            String(date.getHours()).padStart(2, '0') + ":" +
            String(date.getMinutes()).padStart(2, '0') + " " +
            String(date.getDate()).padStart(2, '0') + "/" +
            String(date.getMonth() + 1).padStart(2, '0') + "/" +
            date.getFullYear() : "";

        var date2 = new Date(item.vehicleTroughTimeComeOut);
        let formatted2 = item.vehicleTroughTimeComeOut ?
            String(date2.getHours()).padStart(2, '0') + ":" +
            String(date2.getMinutes()).padStart(2, '0') + " " +
            String(date2.getDate()).padStart(2, '0') + "/" +
            String(date2.getMonth() + 1).padStart(2, '0') + "/" +
            date2.getFullYear() : "";

        var html = "";
        if (item.listTroughWeight != null && item.listTroughWeight.length > 0) {
            for (var i = 0; i < item.listTroughWeight.length; i++) {
                let tw = item.listTroughWeight[i];
                let troughDisplay = tw.vehicleTroughWeight !== null ?
                    `<p style="font-size:13px!important">Máng ${tw.troughType || ""}</p>` :
                    `<div class="status-dropdown">
                        <button class="dropdown-toggle ${isProcessed ? "disabled" : ""} ${tw.vehicleTroughWeight !== null ? "CartoFactory_" + item.id + "_troughWeight" : ""}"
                                data-type="1" data-options='${jsonTrough}' ${isProcessed ? "disabled" : ""}>
                            ${item.troughTypeName || ""}
                        </button>
                    </div>`;

                let actionIcons = "";

                var weightInVal = item.vehicleWeightIn !== null && item.vehicleWeightIn > 0 ? item.vehicleWeightIn.toLocaleString('en-US') : "0";
                var weightOutVal = item.vehicleWeightOut !== null && item.vehicleWeightOut > 0 ? item.vehicleWeightOut.toLocaleString('en-US') : "";
                var netWeightVal = (item.vehicleWeightOut !== null && item.vehicleWeightIn !== null) ?
                    (item.vehicleWeightOut - item.vehicleWeightIn).toLocaleString('en-US') : "";

                var weightOutDisplay = isProcessed ?
                    weightOutVal :
                    `<input class="VehicleWeightIn" id="VehicleWeightIn_${item.id}" value="${item.vehicleWeightIn || ""}" style="display:none;" />
                     <input type="text" style="width:100%!important"
                            class="input-form currency VehicleWeightOut weight CartoFactory_${item.id} _weight"
                            maxlength="8"
                            oninput="this.value = this.value.replace(/[^0-9]/g, '').slice(0,8);"
                            value="${weightOutVal}"
                            data-original="${weightOutVal}"
                            data-id="${item.id}"
                            placeholder="Vui lòng nhập" />`;

                html += `
                <tr class="CartoFactory_${item.id}" data-queue="${formatted2}">
                    <td>${item.recordNumber}</td>
                    <td>${item.vehicleNumber}</td>
                    <td>${item.customerName}</td>
                    <td>${item.driverName}</td>    
                    <td>${formatted}</td>
                    <td>
                        <div style="display: flex; align-items: center">
                            <div style="white-space: pre-line;">
                                ${item.note}
                            </div>
                        </div>
                    </td>
                    <td>${troughDisplay}</td>

                    <td>
                        <input class="TroughWeightId" value="${tw.id}" style="display:none;" />
                        <input type="text" style="width:100%!important" class="input-form currency CartoFactory_TroughWeight weight CartoFactory_${item.id}_weight ${isProcessed ? "disabled" : ""}"
                               maxlength="8" oninput="this.value = this.value.replace(/[^0-9]/g, '').slice(0,8);"
                               value="${tw.vehicleTroughWeight !== null ? tw.vehicleTroughWeight.toLocaleString('en-US') : ""}"
                               data-original="${tw.vehicleTroughWeight !== null ? tw.vehicleTroughWeight.toLocaleString('en-US') : ""}"
                               placeholder="Vui lòng nhập" ${isProcessed ? "disabled" : ""} />
                    </td>
                    <td>${weightInVal}</td>
                    <td>${weightOutDisplay}</td>
                    <td id="NetWeight_${item.id}">${netWeightVal}</td>
                    <td>
                        <div class="status-dropdown">
                            <button class="dropdown-toggle" data-options='${jsonStatus}'>
                                ${item.vehicleWeighingStatusName || ""}
                            </button>
                        </div>
                    </td>
                </tr>`;
            }
        } else {
            html = `
            <tr class="CartoFactory_${item.id}" data-queue="${formatted2}">
                <td>${item.recordNumber}</td>
                <td>${item.vehicleNumber}</td>
                <td>${item.customerName}</td>
                <td>${item.driverName}</td>    
                <td>${formatted}</td>
                <td>
                    <div style="display: flex; align-items: center">
                        <div style="white-space: pre-line;">
                            ${item.note}
                        </div>
                    </div>
                </td>
                <td></td>
                <td></td>
                <td></td>
                <td></td>
                <td></td>
                <td>
                    <div class="status-dropdown">
                        <button class="dropdown-toggle" data-options='${jsonStatus}'>
                            ${item.vehicleWeighingStatusName || ""}
                        </button>
                    </div>
                </td>
            </tr>`;
        }
        return html;
    }

    // Hàm sắp xếp lại tbody theo QueueNumber tăng dần
    function sortTable_Da_SL() {
        const tbody = document.getElementById("dataBody-1");
        if (!tbody) return;
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
        if (!tbody) return;
        const rows = Array.from(tbody.querySelectorAll("tr"));
        rows.sort((a, b) => {
            const timeA = parseDateTime(a.dataset.queue);
            const timeB = parseDateTime(b.dataset.queue);
            return timeA - timeB;
        });

        tbody.innerHTML = "";
        rows.forEach(r => tbody.appendChild(r));
    }

    // Nhận data mới từ server
    connection.off("ListVehicles_Da_SL");
    connection.on("ListVehicles_Da_SL", function (item) {
        $('.CartoFactory_' + item.id).remove();
        const tbody = document.getElementById("dataBody-1");
        if (tbody) {
            tbody.insertAdjacentHTML("beforeend", renderRow(item, true));
            sortTable_Da_SL();
            _listVehicles.autoRowspanWithCondition("ListCarCall-1", [0, 1, 2, 3, 4, 5, 8, 9, 10, 11], [0, 1, 2, 3, 4, 5]);
        }
    });

    connection.off("ListVehicles");
    connection.on("ListVehicles", function (item) {
        $('.CartoFactory_' + item.id).remove();
        const tbody = document.getElementById("dataBody-0");
        if (tbody) {
            tbody.insertAdjacentHTML("beforeend", renderRow(item, false));
            sortTable();
            _listVehicles.autoRowspanWithCondition("ListCarCall-0", [0, 1, 2, 3, 4, 5, 8, 9, 10, 11], [0, 1, 2, 3, 4, 5]);
        }
    });

    // Nhận data mới từ gọi xe cân đầu vào
    connection.off("ListCarCall_Da_SL");
    connection.on("ListCarCall_Da_SL", function (item) {
        $('.CartoFactory_' + item.id).remove();
        const tbody = document.getElementById("dataBody-0");
        if (tbody) {
            tbody.insertAdjacentHTML("beforeend", renderRow(item, false));
            sortTable();
            _listVehicles.autoRowspanWithCondition("ListCarCall-0", [0, 1, 2, 3, 4, 5, 8, 9, 10, 11], [0, 1, 2, 3, 4, 5]);
        }
    });

    connection.off("ListCarCall");
    connection.on("ListCarCall", function (item) {
        $('#dataBody-0').find('.CartoFactory_' + item.id).remove();
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
        if (!str) return 0;
        // "11:33 17/12/2025"
        const parts = str.split(" ");
        if (parts.length < 2) return 0;
        const [time, date] = parts;
        const hourMin = time.split(":");
        const dayMonthYear = date.split("/");
        if (hourMin.length < 2 || dayMonthYear.length < 3) return 0;

        const [hour, minute] = hourMin.map(Number);
        const [day, month, year] = dayMonthYear.map(Number);

        return new Date(year, month - 1, day, hour, minute).getTime();
    }

    $(document).on('blur', 'input.CartoFactory_TroughWeight', function (e) {
        var element = $(this);
        var originalValue = element.attr('data-original') || "";
        var currentValue = element.val();

        if (currentValue === originalValue) {
            return;
        }

        var TroughWeightId = element.closest('td').find('input.TroughWeightId').val();
        var weight_TroughWeightId = currentValue != undefined ? currentValue.replace(',', '') : 0;
        _listVehicles.UpdateTroughWeight(TroughWeightId, weight_TroughWeightId, element);
    });

    $(document).on('input keyup change', 'input.VehicleWeightOut', function (e) {
        var element = $(this);
        var id = element.attr('data-id');
        var currentValue = element.val() || "";

        // Tính toán lại NET WEIGHT: Tải trọng cân ra - VehicleWeightIn
        var vehicleWeightInVal = element.closest('td').find('input.VehicleWeightIn').val() || 0;
        var weightIn = parseFloat(vehicleWeightInVal.toString().replace(/,/g, '')) || 0;
        var weightOut = parseFloat(currentValue.replace(/,/g, '')) || 0;
        var netWeight = weightOut - weightIn;

        // Hiển thị NET WEIGHT đã định dạng có dấu phẩy phân tách
        $('#NetWeight_' + id).text(netWeight.toLocaleString('en-US'));
    });

    $(document).on('blur', 'input.VehicleWeightOut', function (e) {
        var element = $(this);
        var originalValue = element.attr('data-original') || "";
        var currentValue = element.val();

        if (currentValue === originalValue) {
            return;
        }

        var id = element.attr('data-id');
        var weight_Out = currentValue != undefined && currentValue !== "" ? currentValue.replace(/,/g, '') : 0;
        _listVehicles.UpdateVehicleWeightOut(id, weight_Out, element);
    });
});
var _listVehicles = {
    init: function () {
        _listVehicles.ListCartoFactory();
        _listVehicles.ListCartoFactory_Da_SL();
    },
    ListCartoFactory: function () {
        var model = {
            VehicleNumber: $('#input_chua_xu_ly').val(),
            PhoneNumber: $('#input_chua_xu_ly').val(),
            VehicleStatus: 0,
            LoadType: null,
            VehicleWeighingType: 0,
            VehicleTroughStatus: 0,
            TroughType: null,
            VehicleWeighingStatus: null,
            LoadingStatus: 0,
            VehicleWeighedstatus: 0,
            type: 0,
        }
        $.ajax({
            url: "/ListCar/ListVehiclesisLoading",
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
    },
    ListCartoFactory_Da_SL: function () {
        var model = {
            VehicleNumber: $('#input_da_xu_ly').val(),
            PhoneNumber: $('#input_da_xu_ly').val(),
            VehicleStatus: 0,
            LoadType: null,
            VehicleWeighingType: 0,
            VehicleTroughStatus: 0,
            TroughType: null,
            VehicleWeighingStatus: 0,
            LoadingStatus: 0,
            VehicleWeighedstatus: 0,
            type: 1,

        }
        $.ajax({
            url: "/ListCar/ListVehiclesisLoading",
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
    UpdateStatus: function (id, status, type, weight, note) {
        $.ajax({
            url: "/Car/UpdateStatus",
            type: "post",
            data: { id: id, status: status, type: type, weight: 0, Note: note },
            success: function (result) {
                if (result.status == 0) {
                    _msgalert.success(result.msg)

                } else {
                    _msgalert.error(result.msg)
                }
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                console.log("Status: " + textStatus);
            }
        });
    },
    UpdateTroughWeight: function (id, weight_TroughWeightId, element) {
        $.ajax({
            url: "/Car/UpdateTroughWeight",
            type: "post",
            data: { id: id, VehicleTroughWeight: weight_TroughWeightId },
            success: function (result) {
                if (result.status == 0) {
                    _msgalert.success(result.msg);
                    // Update data-original with the newly saved value
                    if (element) {
                        element.closest('td').find('input.CartoFactory_TroughWeight').attr('data-original', weight_TroughWeightId);
                    }
                } else {
                    _msgalert.error(result.msg);
                }
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                console.log("Status: " + textStatus);
            }
        });
    },
    CancelTroughWeight: function (id, element) {
        var $input = element.closest('td').find('input.CartoFactory_TroughWeight');
        var originalValue = $input.data('original') || "";
        $input.val(originalValue);
        _msgalert.success("Đã hủy thao tác và khôi phục số liệu ban đầu.");

        // Optionally still call the server API if it performs necessary cleanup
        $.ajax({
            url: "/Car/CancelTroughWeight",
            type: "post",
            data: { id: id },
            success: function (result) {
                // If the server returns a definitive 'original' value, we can sync it here too
                if (result.status == 0 && result.data !== undefined) {
                    $input.val(result.data);
                    $input.attr('data-original', result.data);
                }
            }
        });
    },
    autoRowspanWithCondition: function (tableId, colIndexes, compareCols) {
        const table = document.getElementById(tableId);
        if (!table) return;

        const rows = Array.from(table.querySelectorAll("tbody tr"));

        // Khởi tạo object lưu text các cột compare của hàng trước
        let prevRowCompareText = {};

        rows.forEach(row => {
            colIndexes.forEach(colIndex => {
                if (row.cells[colIndex]) {
                    row.cells[colIndex].style.display = "";
                    row.cells[colIndex].rowSpan = 1;
                }
            });
        });

        colIndexes.forEach(colIndex => {
            let prevCell = null;
            let prevText = "";
            let span = 1;

            rows.forEach(row => {
                const cell = row.cells[colIndex];
                if (!cell) return;

                // Lấy text sạch
                const text = cell.innerText.replace(/\s+/g, ' ').trim();

                // Kiểm tra điều kiện các cột compareCols có giống nhau
                const sameAll = compareCols.every(c => {
                    const compareCell = row.cells[c];
                    if (!compareCell) return false;
                    const compareText = compareCell.innerText.replace(/\s+/g, ' ').trim();
                    return compareText === prevRowCompareText[c];
                });

                if (text === prevText && sameAll && prevCell != null) {
                    span++;
                    prevCell.rowSpan = span;
                    cell.style.display = "none"; // ẩn ô trùng
                } else {
                    prevText = text;
                    prevCell = cell;
                    span = 1;

                    // Lưu text của các cột so sánh cho hàng này
                    compareCols.forEach(c => {
                        const compareCell = row.cells[c];
                        prevRowCompareText[c] = compareCell ? compareCell.innerText.replace(/\s+/g, ' ').trim() : '';
                    });
                }
            });
        });
    },
    UpdateVehicleWeightOut: function (id, vehicleWeightOut, element) {
        var status_type = 0
        $.ajax({
            url: "/Car/UpdateVehicleWeightOut",
            type: "post",
            data: { id: id, VehicleWeightOut: vehicleWeightOut },
            success: function (result) {
                status_type = result.status;
                if (result.status == 0) {
                    _msgalert.success(result.msg)
                    if (element) {
                        element.attr('data-original', element.val());
                        // Tính toán lại NET WEIGHT: Tải trọng cân ra - VehicleWeightIn
                        var vehicleWeightInVal = element.closest('td').find('input.VehicleWeightIn').val() || 0;
                        var weightIn = parseFloat(vehicleWeightInVal.toString().replace(/,/g, '')) || 0;
                        var weightOut = parseFloat(vehicleWeightOut) || 0;
                        var netWeight = weightOut - weightIn;

                        // Hiển thị NET WEIGHT đã định dạng có dấu phẩy phân tách
                        $('#NetWeight_' + id).text(netWeight.toLocaleString('en-US'));
                    }
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