$(document).ready(function () {
    if (window._obdToken) {
        _obdToken.bindEvents();
        _obdToken.onChanged = function () {
            // OBD đổi → reload để stack Ship-to / Net OBD / Check khớp số dòng
            _Weighed_Input.ListWeighedInput();
        };
    }
    _Weighed_Input.init();
    var input_Weighed_Input_Chua_SL = document.getElementById("input_Weighed_Input_Chua_SL");
    input_Weighed_Input_Chua_SL.addEventListener("keypress", function (event) {
        // If the user presses the "Enter" key on the keyboard
        if (event.key === "Enter") {
            // Cancel the default action, if needed
            event.preventDefault();
            // Trigger the button element with a click
            _Weighed_Input.ListWeighedInput();
        }
    });
    input_Weighed_Input_Chua_SL.addEventListener("keyup", function (event) {
        _Weighed_Input.ListWeighedInput();
    });
    var input_Weighed_Input_Da_SL = document.getElementById("input_Weighed_Input_Da_SL");
    input_Weighed_Input_Da_SL.addEventListener("keypress", function (event) {
        // If the user presses the "Enter" key on the keyboard
        if (event.key === "Enter") {
            // Cancel the default action, if needed
            event.preventDefault();
            // Trigger the button element with a click
            _Weighed_Input.ListWeighedInput_Da_SL();
        }
    });
    input_Weighed_Input_Da_SL.addEventListener("keyup", function (event) {
        _Weighed_Input.ListWeighedInput_Da_SL();
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
                const $btn = $currentBtn;
                $.ajax({
                    url: "/Car/UpdateStatus",
                    type: "post",
                    data: { id: id_row, status: val_TT, type: 9 },
                    success: function (result) {
                        if (result.status == 0) {
                            _msgalert.success(result.msg)
                            $btn
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
                        } else {
                            _msgalert.error(result.msg)
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
    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/CarHub", { transport: signalR.HttpTransportType.WebSockets, skipNegotiation: true })
        .withAutomaticReconnect([ 2000, 5000, 10000])
        .build();
    connection.start()
        .then(() => console.log("✅ SignalR connected"))
        .catch(err => console.error(err));
    const AllCode = [
        { Description: "Blank", CodeValue: "1" },
        { Description: "Đã cân xong đầu vào", CodeValue: "0" },
        // Add more objects as needed
    ];
    // Create a new array of objects in the desired format
    const options = AllCode.map(allcode => ({
        text: allcode.Description,
        value: allcode.CodeValue
    }));
    const jsonString = JSON.stringify(options);
    // Hàm render row
    function renderRow(item, isProcessed = false) {
        var date = new Date(item.registerDateOnline);
        let formatted =
            String(date.getHours()).padStart(2, '0') + ":" +
            String(date.getMinutes()).padStart(2, '0') + " " +
            String(date.getDate()).padStart(2, '0') + "/" +
            String(date.getMonth() + 1).padStart(2, '0') + "/" +
            date.getFullYear();
        var date2 = new Date(item.vehicleWeighingTimeComeIn);
        let formatted2 =
            String(date2.getHours()).padStart(2, '0') + ":" +
            String(date2.getMinutes()).padStart(2, '0') + " " +
            String(date2.getDate()).padStart(2, '0') + "/" +
            String(date2.getMonth() + 1).padStart(2, '0') + "/" +
            date2.getFullYear();

        // Realtime row: reload list để đủ SKU/OBD stack (tránh lệch cột)
        return '';
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
    function sortTable_Da_SL2() {
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
            const qa = parseInt(a.getAttribute("data-queue") || 0);
            const qb = parseInt(b.getAttribute("data-queue") || 0);
            return qa - qb;
        });

        tbody.innerHTML = "";
        rows.forEach(r => tbody.appendChild(r));
    }
    function sortTable2() {
        const tbody = document.getElementById("dataBody-0");
        const rows = Array.from(tbody.querySelectorAll("tr"));

        rows.sort((a, b) => {
            const qa = parseInt(a.getAttribute("data-LoadType") || 0);
            const qb = parseInt(b.getAttribute("data-LoadType") || 0);
            return qa - qb;
        });

        tbody.innerHTML = "";
        rows.forEach(r => tbody.appendChild(r));
    }

    // Realtime: reload partial để đủ cột OBD/SKU (tránh lệch cột)
    connection.off("ListWeighedInput_Da_SL");
    connection.on("ListWeighedInput_Da_SL", function () {
        _Weighed_Input.ListWeighedInput_Da_SL();
    });
    connection.off("ListWeighedInput");
    connection.on("ListWeighedInput", function () {
        _Weighed_Input.ListWeighedInput();
    });
    connection.off("ListWeighedInput_GoodsType");
    connection.on("ListWeighedInput_GoodsType", function () {
        _Weighed_Input.ListWeighedInput();
    });
    connection.off("ListCallTheScale_Da_SL");
    connection.on("ListCallTheScale_Da_SL", function () {
        _Weighed_Input.ListWeighedInput();
    });
    connection.off("ListCallTheScale_0");
    connection.on("ListCallTheScale_0", function (item) {
        $('.CartoFactory_' + item.id).remove();
    });
    connection.off("ListCallTheScale_1");
    connection.on("ListCallTheScale_1", function (item) {
        $('.CartoFactory_' + item.id).remove();
    });
    connection.off("ListCarCall_Da_SL");
    connection.on("ListCarCall_Da_SL", function (item) {
        $('.CartoFactory_' + item.id).remove();
    });
    connection.off("ListCarCall");
    connection.on("ListCarCall", function () {
        _Weighed_Input.ListWeighedInput_Da_SL();
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
    }

    $(document).on('blur', 'input.VehicleWeightIn', function (e) {
        var element = $(this);
        var originalValue = element.attr('data-original') || "";
        var currentValue = element.val();

        if (currentValue === originalValue) {
            return;
        }

        var id = element.attr('data-id');
        var weight_In = currentValue != undefined && currentValue !== "" ? currentValue.replace(/,/g, '') : 0;
        _Weighed_Input.UpdateVehicleWeightIn(id, weight_In, element);
    });

    $(document).on('focus', 'select.goods-type-select', function () {
        $(this).data('prev-goods-type', $(this).val());
    });

    $(document).on('change', 'select.goods-type-select', function () {
        var $sel = $(this);
        var id = parseInt($sel.attr('data-id'), 10);
        var val = $sel.val();
        var prev = $sel.data('prev-goods-type');
        if (!id || val === '') return;

        var GOODS_BAG = '0';
        var GOODS_BULK = '1';

        // Hàng xá → Hàng bao: hỏi xác nhận nếu đã nhập SKU, rồi xóa sạch về ô trống
        if (String(prev) === GOODS_BULK && String(val) === GOODS_BAG) {
            var $panel = $sel.closest('tr').find('.sku-panel');
            var hasSkuData = false;
            $panel.find('input.sku-weight').each(function () {
                if (_Weighed_Input.ParseViDecimal($(this).val()) > 0) {
                    hasSkuData = true;
                    return false;
                }
            });
            if (hasSkuData) {
                if (!confirm('Khi bạn đổi sang hàng bao dữ liệu SKU sẽ bị xóa, bạn có đồng ý không')) {
                    $sel.val(prev);
                    return;
                }
            }
            $.ajax({
                url: '/Car/ClearWeighInSkus',
                type: 'post',
                data: { vehicleInspectionId: id },
                success: function (clearResult) {
                    if (clearResult.status != 0) {
                        _msgalert.error(clearResult.msg || 'Không xóa được SKU');
                        $sel.val(prev);
                        return;
                    }
                    _Weighed_Input.UpdateGoodsType(id, val, $sel);
                },
                error: function () {
                    _msgalert.error('Không xóa được SKU');
                    $sel.val(prev);
                }
            });
            return;
        }

        _Weighed_Input.UpdateGoodsType(id, val, $sel);
    });

    $(document).on('blur', 'textarea.WI_Note', function () {
        var $ta = $(this);
        var id = parseInt($ta.attr('data-id'), 10);
        if (!id) return;
        $.ajax({
            url: '/Car/UpdateStatus',
            type: 'post',
            data: { id: id, status: 0, type: 13, Note: $ta.val() },
            success: function (result) {
                if (result.status == 0) _msgalert.success(result.msg);
                else _msgalert.error(result.msg);
            }
        });
    });

    $(document).on('click', '.sku-add', function () {
        var $panel = $(this).closest('.sku-panel');
        var vehicleId = parseInt($panel.attr('data-vehicle-id'), 10);
        if (!vehicleId) return;
        $.ajax({
            url: '/Car/InsertWeighInSku',
            type: 'post',
            data: { vehicleInspectionId: vehicleId, weight: 0 },
            success: function (result) {
                if (result.status == 0) _Weighed_Input.ListWeighedInput();
                else _msgalert.error(result.msg || 'Không thêm được SKU');
            }
        });
    });

    $(document).on('click', '.sku-remove', function () {
        var skuId = parseInt($(this).closest('.sku-row').attr('data-sku-id'), 10);
        if (!skuId) return;
        if (!confirm('Xóa SKU này?')) return;
        $.ajax({
            url: '/Car/DeleteWeighInSku',
            type: 'post',
            data: { id: skuId },
            success: function (result) {
                if (result.status == 0) _Weighed_Input.ListWeighedInput();
                else _msgalert.error(result.msg || 'Không xóa được SKU');
            }
        });
    });

    $(document).on('blur', 'input.sku-weight', function () {
        var $input = $(this);
        var skuId = parseInt($input.attr('data-sku-id'), 10);
        if (!skuId) return;
        var weight = _Weighed_Input.ParseViDecimal($input.val());
        if (weight > 0) {
            $input.val(Math.round(weight).toLocaleString('en-US'));
        }
        $.ajax({
            url: '/Car/UpdateWeighInSku',
            type: 'post',
            data: { id: skuId, weight: weight },
            success: function (result) {
                if (result.status == 0) {
                    var $panel = $input.closest('.sku-panel');
                    _Weighed_Input.SyncSkuTotal($panel);
                } else {
                    _msgalert.error(result.msg || 'Không lưu được SKU');
                }
            }
        });
    });
});
var _Weighed_Input = {
    init: function () {
        _Weighed_Input.ListWeighedInput();
        _Weighed_Input.ListWeighedInput_Da_SL();
    },
    UpdateGoodsType: function (id, val, $sel) {
        $.ajax({
            url: '/Car/UpdateStatus',
            type: 'post',
            data: { id: id, status: parseInt(val, 10), type: 14 },
            success: function (result) {
                if (result.status == 0) {
                    _msgalert.success(result.msg);
                    _Weighed_Input.ListWeighedInput();
                } else {
                    _msgalert.error(result.msg);
                    if ($sel && $sel.data('prev-goods-type') != null) {
                        $sel.val($sel.data('prev-goods-type'));
                    }
                    _Weighed_Input.ListWeighedInput();
                }
            },
            error: function () {
                if ($sel && $sel.data('prev-goods-type') != null) {
                    $sel.val($sel.data('prev-goods-type'));
                }
            }
        });
    },
    ParseViDecimal: function (text) {
        var s = String(text || '').trim();
        if (!s) return 0;
        // Format hàng nghìn en-US (1,510) hoặc vi-VN (1.510,5)
        if (s.indexOf(',') >= 0 && s.indexOf('.') >= 0) {
            if (s.lastIndexOf(',') > s.lastIndexOf('.')) {
                s = s.replace(/\./g, '').replace(',', '.');
            } else {
                s = s.replace(/,/g, '');
            }
        } else if (s.indexOf(',') >= 0) {
            // Chỉ có dấu phẩy → coi là hàng nghìn (1,510)
            s = s.replace(/,/g, '');
        } else {
            s = s.replace(/\./g, '');
        }
        var n = parseFloat(s);
        return isNaN(n) || n < 0 ? 0 : n;
    },
    SyncSkuTotal: function ($panel) {
        var sum = 0;
        $panel.find('input.sku-weight').each(function () {
            sum += _Weighed_Input.ParseViDecimal($(this).val());
        });
        $panel.find('.sku-total-value').text(Math.round(sum).toLocaleString('en-US'));
        var $tr = $panel.closest('tr');
        var count = $panel.find('.sku-row').length;
        $tr.find('[data-progress]').text(count + ' SKU').removeClass('muted');
    },
    ListWeighedInput: function () {
        var model = {
            VehicleNumber: $('#input_Weighed_Input_Chua_SL').val() != undefined && $('#input_Weighed_Input_Chua_SL').val() != "" ? $('#input_Weighed_Input_Chua_SL').val().trim() : "",
            PhoneNumber: $('#input_Weighed_Input_Chua_SL').val() != undefined && $('#input_Weighed_Input_Chua_SL').val() != "" ? $('#input_Weighed_Input_Chua_SL').val().trim() : "", 
            VehicleStatus: 0,
            LoadType: null,
            VehicleWeighingType: 0,
            VehicleTroughStatus: null,
            TroughType: null,
            VehicleWeighingStatus: null,
            LoadingStatus: 0,
            VehicleWeighedstatus: null,
            type: 0,
        }
        $.ajax({
            url: "/Car/ListWeighedInput",
            type: "post",
            data: { SearchModel: model },
            success: function (result) {
                $('#imgLoading').hide();
                $('#Weighed_Input_Chua_SL').html(result);
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                console.log("Status: " + textStatus);
            }
        });
    },
    ListWeighedInput_Da_SL: function () {
        var model = {
            VehicleNumber: $('#input_Weighed_Input_Da_SL').val() != undefined && $('#input_Weighed_Input_Da_SL').val() != "" ? $('#input_Weighed_Input_Da_SL').val().trim() : "",
            PhoneNumber: $('#input_Weighed_Input_Da_SL').val() != undefined && $('#input_Weighed_Input_Da_SL').val() != "" ? $('#input_Weighed_Input_Da_SL').val().trim() : "",
            VehicleStatus: 0,
            LoadType: null,
            VehicleWeighingType: 0,
            VehicleTroughStatus: null,
            TroughType: null,
            VehicleWeighingStatus: null,
            LoadingStatus: 0,
            VehicleWeighedstatus: 0,
            type: 1,
        }
        $.ajax({
            url: "/Car/ListWeighedInput",
            type: "post",
            data: { SearchModel: model },
            success: function (result) {
                $('#imgLoading').hide();
                $('#Weighed_Input_Da_SL').html(result);
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                console.log("Status: " + textStatus);
            }
        });
    },
    UpdateStatus: function (id, status, type) {
        var status_type = 0
        $.ajax({
            url: "/Car/UpdateStatus",
            type: "post",
            data: { id: id, status: status, type: type },
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
    UpdateVehicleWeightIn: function (id, vehicleWeightIn, element) {
        var status_type = 0
        $.ajax({
            url: "/Car/UpdateVehicleWeightIn",
            type: "post",
            data: { id: id, VehicleWeightIn: vehicleWeightIn },
            success: function (result) {
                status_type = result.status;
                if (result.status == 0) {
                    _msgalert.success(result.msg)
                    if (element) {
                        element.attr('data-original', element.val());
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