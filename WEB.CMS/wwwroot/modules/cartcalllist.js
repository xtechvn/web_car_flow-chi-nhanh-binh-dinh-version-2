$(document).ready(function () {
    _cartcalllist.init();
    var input_chua_xu_ly = document.getElementById("input_chua_xu_ly");
    input_chua_xu_ly.addEventListener("keypress", function (event) {
        // If the user presses the "Enter" key on the keyboard
        if (event.key === "Enter") {
            // Cancel the default action, if needed
            event.preventDefault();
            // Trigger the button element with a click
            _cartcalllist.ListCartoFactory();
        }
    });
    input_chua_xu_ly.addEventListener("keyup", function (event) {
        _cartcalllist.ListCartoFactory();
    });
    var input_da_xu_ly = document.getElementById("input_da_xu_ly");
    input_da_xu_ly.addEventListener("keypress", function (event) {
        // If the user presses the "Enter" key on the keyboard
        if (event.key === "Enter") {
            // Cancel the default action, if needed
            event.preventDefault();
            // Trigger the button element with a click
            _cartcalllist.ListCartoFactory_Da_SL();
        }
    });
    input_da_xu_ly.addEventListener("keyup", function (event) {
        _cartcalllist.ListCartoFactory_Da_SL();
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
    $(document).on('click', '.dropdown-menu .actions .confirm', async function (e) {
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

                var type = $currentBtn.attr('data-type');

                if (type == '1') {
                    // update máng xuất
                    var status_type = 0;
                    $.ajax({
                        url: "/Car/UpdateStatus",
                        type: "post",
                        data: { id: id_row, status: val_TT, type: 4, weight: 0 },
                        success: function (result) {
                            status_type = result.status;
                            if (result.status == 0) {
                                _msgalert.success(result.msg);
                                _cartcalllist.initMangStatus();
                                // ✅ chỉ remove row nếu cập nhật thành công
                                if (type == 6) {
                                    if (parseInt(status) == 0) {
                                        $('#dataBody-0').find('.CartoFactory_' + id).remove();
                                    } else {
                                        $('#dataBody-1').find('.CartoFactory_' + id).remove();
                                    }
                                }
                                if ($currentBtn != null)
                                    $currentBtn
                                        .text(text)
                                        .removeClass(function (_, old) {
                                            return (old.match(/(^|\s)status-\S+/g) || []).join(' ');
                                        })
                                        .addClass($active.attr('class').split(/\s+/).filter(c => c !== 'active')[0] || '');

                            } else {
                                _msgalert.error(result.msg);
                            }

                        },
                        error: function (XMLHttpRequest, textStatus, errorThrown) {
                            console.log("Status: " + textStatus);
                        }

                    });

                } else {
                    var weight = $('.CartoFactory_' + id_row).find('input.weight').val() != undefined ? $('.CartoFactory_' + id_row).find('input.weight').val().replaceAll(",", "") : 0;
                    var note = null;
                    var text_type = $('.CartoFactory_' + id_row + '_troughWeight').text().trim();


                    // ✅ Gọi API update
                    var status_type = 0;
                    $.ajax({
                        url: "/Car/UpdateStatus",
                        type: "post",
                        data: { id: id_row, status: val_TT, type: 6, weight: weight > 0 ? weight.replaceAll(",", "") : weight, Note: note },
                        success: function (result) {
                            status_type = result.status;
                            if (result.status == 0) {
                                _msgalert.success(result.msg);

                                // ✅ chỉ remove row nếu cập nhật thành công
                                if (type == 6) {
                                    if (parseInt(status) == 0) {
                                        $('#dataBody-0').find('.CartoFactory_' + id).remove();
                                    } else {
                                        $('#dataBody-1').find('.CartoFactory_' + id).remove();
                                    }
                                }
                                if ($currentBtn != null)
                                    $currentBtn
                                        .text(text)
                                        .removeClass(function (_, old) {
                                            return (old.match(/(^|\s)status-\S+/g) || []).join(' ');
                                        })
                                        .addClass($active.attr('class').split(/\s+/).filter(c => c !== 'active')[0] || '');


                            } else {
                                _msgalert.error(result.msg);
                            }

                        },
                        error: function (XMLHttpRequest, textStatus, errorThrown) {
                            console.log("Status: " + textStatus);
                        }

                    });

                    // ✅ xử lý máng trống / đang xử lý
                    if (val_TT == 0) {
                        let mangName = $row.find('button[data-type="1"]').text().trim();
                        let match = mangName.match(/\d+/);
                        if (match) {
                            let mangIndex = parseInt(match[0]);
                            let stillHasCar = $("#dataBody-0 tr, #dataBody-1 tr").toArray().some(tr => {
                                let btnText = $(tr).find("button[data-type='1']").text().trim();
                                let trangThai = $(tr).find("td:last .dropdown-toggle").text().trim();
                                return btnText === mangName && trangThai !== "Hoàn thành" && trangThai !== "Bỏ lượt";
                            });

                            if (stillHasCar) {
                                $("#input" + mangIndex).val("Đang xử lý")
                                    .removeClass("empty").addClass("processing");
                            } else {
                                $("#input" + mangIndex).val("Trống")
                                    .removeClass("processing").addClass("empty");
                            }
                        }
                    } else {
                        if (val_TT == 4) {
                            let mangName = $row.find('button[data-type="1"]').text().trim();
                            let match = mangName.match(/\d+/);
                            if (match) {
                                let mangIndex = parseInt(match[0]);
                                let stillHasCar = $("#dataBody-0 tr, #dataBody-1 tr").toArray().some(tr => {
                                    let btnText = $(tr).find("button[data-type='1']").text().trim();
                                    let trangThai = $(tr).find("td:last .dropdown-toggle").text().trim();
                                    return btnText === mangName && trangThai !== "Hoàn thành" && trangThai !== "Bỏ lượt";
                                });

                                if (stillHasCar) {
                                    $("#input" + mangIndex).val("Đang xử lý")
                                        .removeClass("empty").addClass("processing");
                                } else {
                                    $("#input" + mangIndex).val("Trống")
                                        .removeClass("processing").addClass("empty");
                                }
                            }
                        } else {
                            let mangName = $row.find('button[data-type="1"]').text().trim();
                            let match = mangName.match(/\d+/);
                            if (match) {
                                let mangIndex = parseInt(match[0]);
                                $("#input" + mangIndex).val("Đang xử lý")
                                    .removeClass("empty").addClass("processing");
                            }
                        }

                    }
                }
            }
        }
        closeMenu();
    });

    // 🚫 Chặn đóng menu khi click vào input phụ
    $(document).on('click', '.dropdown-menu .extra-weight input', function (e) {
        e.stopPropagation();
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
    const AllCode = [
        { Description: "Máng 1", CodeValue: "1" },
        { Description: "Máng 2", CodeValue: "2" },
        { Description: "Máng 3", CodeValue: "3" },
        { Description: "Máng 4", CodeValue: "4" },
        { Description: "Máng 5", CodeValue: "5" },
        { Description: "Máng 6", CodeValue: "6" },
        { Description: "Máng 7", CodeValue: "7" },
        { Description: "Máng 8", CodeValue: "8" },
        { Description: "Hàng xá", CodeValue: "9" },
        // Add more objects as needed
    ];
    const AllCode2 = [
        { Description: "Ngắt máng", CodeValue: "5" },
        
        { Description: "Blank", CodeValue: "3" },
     
        { Description: "Đã gọi", CodeValue: "1" },
        { Description: "Hoàn thành", CodeValue: "0" },
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
    function renderRow(item, isProcessed) {
        var date = new Date(item.vehicleWeighingTimeComeOut);
        let formatted =
            String(date.getHours()).padStart(2, '0') + ":" +
            String(date.getMinutes()).padStart(2, '0') + " " +
            String(date.getDate()).padStart(2, '0') + "/" +
            String(date.getMonth() + 1).padStart(2, '0') + "/" +
            date.getFullYear();
        var html = ``;
        var rankIcon = "";
        switch (item.rank) {
            case 0:
                rankIcon = "<img src='/images/icons/icon-bac.png' class='rank-icon silver' />";
                break;
            case 1:
                rankIcon = "<img src='/images/icons/icon-vang.png' class='rank-icon gold' />";
                break;
            case 2:
                rankIcon = "<img src='/images/icons/icon-kim-cuong.png' class='rank-icon diamond' />";
                break;
        }
        if (item.listTroughWeight != null) {
            for (var itemTroughWeight = 0; itemTroughWeight < item.listTroughWeight.length; itemTroughWeight++) {

                var html_input = ` <div class="status-dropdown">
            ${item.listTroughWeight[itemTroughWeight].vehicleTroughWeight > 0 ? "" : ""}
                <button class="dropdown-toggle ${isProcessed ? "disabled" : ""} ${item.listTroughWeight[itemTroughWeight].vehicleTroughWeight > 0 ? "CartoFactory_" + item.id + "_troughWeight" : ""}"
                        data-type="1"
                        data-options='${jsonString}'
                        ${isProcessed ? "disabled" : ""}>
                  Máng  ${item.listTroughWeight[itemTroughWeight].troughType || ""}
                </button>
            </div>`
                var html_div = ` <div class="status-dropdown"> <p style="font-size:13px!important">Máng ${item.listTroughWeight[itemTroughWeight].troughType || ""}</p></div>`
                var html_icon = `  <a class="cursor-pointer check-troughWeight" title="Lưu" style="margin-left: 6px;">
                                        <i class="icon-check"></i>
                                    </a>
                                    <a class="cursor-pointer cancel-troughWeight" title="Hủy thao tác">
                                        <i class="icon-cancel"></i>
                                    </a>`
                html += `
    <tr class="CartoFactory_${item.id}" data-queue="${item.recordNumber}">
        <td>${item.recordNumber}</td>
        <td class="customer-rank">${rankIcon} <span class="rank-text">${item.customerName}</span></td>
        <td>${item.driverName}</td>
        <td><a class="btn-detail"
                           data-id="${item.id}" style="cursor:pointer">${item.vehicleNumber}</a></td>
        <td>${formatted || ""}</td>
        <td>${item.vehicleLoadTaken > 0 ? item.vehicleLoadTaken.toLocaleString('en-US') : 0}</td>
        <td>
        
            ${item.listTroughWeight[itemTroughWeight].vehicleTroughWeight != null? html_div : html_input}
        </td>
        <td>
            <textarea class="Note" name="Note" value="${item.note || ""}">${item.note || ""}</textarea>
        </td>
        <td>
            <div class="status-dropdown">
                <button class="dropdown-toggle"
                        data-options='${jsonString2}'>
                    ${item.vehicleTroughStatusName || ""}
                </button>
            </div>
        </td>
    </tr>`
            }
        }
        else {
            var html_input = ` <div class="status-dropdown">
           
                <button class="dropdown-toggle ${isProcessed ? "disabled" : ""}  "CartoFactory_" +${item.id} + "_troughWeight" : ""}"
                        data-type="1"
                        data-options='${jsonString}'
                        ${isProcessed ? "disabled" : ""}>
                  Máng  ${item.troughType || ""}
                </button>
            </div>`
            var html_div = ` <div class="status-dropdown"> <p style="font-size:13px!important">Máng ${item.troughType || ""}</p></div>`
            var html_icon = `  <a class="cursor-pointer check-troughWeight" title="Lưu" style="margin-left: 6px;">
                                        <i class="icon-check"></i>
                                    </a>
                                    <a class="cursor-pointer cancel-troughWeight" title="Hủy thao tác">
                                        <i class="icon-cancel"></i>
                                    </a>`
            html += `
    <tr class="CartoFactory_${item.id}" data-queue="${item.recordNumber}">
        <td>${item.recordNumber}</td>
        <td class="customer-rank">${rankIcon} <span class="rank-text">${item.customerName}</span></td>
        <td>${item.driverName}</td>
        <td><a class="btn-detail"
                           data-id="${item.id}" style="cursor:pointer">${item.vehicleNumber}</a></td>
        <td>${formatted || ""}</td>
        <td>${item.vehicleLoadTaken > 0 ? item.vehicleLoadTaken.toLocaleString('en-US') : 0}</td>
        <td>
           ${item.vehicleTroughWeight > 0 ? html_div : html_input}
        </td>
      <td>
        <textarea class="Note" name="Note"  ${isProcessed ? "disabled" : ""} value="${item.note || ""}">${item.note || ""}</textarea>
    </td>
        <td>
            <div class="status-dropdown">
                <button class="dropdown-toggle"
                        data-options='${jsonString2}'>
                    ${item.vehicleTroughStatusName || ""}
                </button>
            </div>
        </td>
    </tr>`
        }
        return html;
    }
    //function renderRow(item, isProcessed) {
    //    var date = new Date(item.vehicleWeighingTimeComeOut);
    //    let formatted =
    //        String(date.getHours()).padStart(2, '0') + ":" +
    //        String(date.getMinutes()).padStart(2, '0') + " " +
    //        String(date.getDate()).padStart(2, '0') + "/" +
    //        String(date.getMonth() + 1).padStart(2, '0') + "/" +
    //        date.getFullYear();
    //    return `
    //<tr class="CartoFactory_${item.id}" data-queue="${item.recordNumber}">
    //    <td>${item.recordNumber}</td>
    //    <td>${item.customerName}</td>
    //    <td>${item.driverName}</td>
    //    <td><a class="btn-detail"
    //                       data-id="${item.id}" style="cursor:pointer">${item.vehicleNumber}</a></td>
    //    <td>${formatted || ""}</td>
    //    <td>
    //        <div class="status-dropdown">
    //            <button class="dropdown-toggle ${isProcessed ? "disabled" : ""}"
    //                    data-type="1"
    //                    data-options='${jsonString}'
    //                    ${isProcessed ? "disabled" : ""}>
    //                ${item.troughTypeName || ""}
    //            </button>
    //        </div>
    //    </td>
    //  <td>
    //    <input type="text"
    //           class="input-form weight"
    //           value="${item.vehicleTroughWeight > 0 ? item.vehicleTroughWeight : ""}"
    //           placeholder="Vui lòng nhập"
    //           ${isProcessed ? "disabled" : ""} />
    //</td>
    //    <td>
    //        <div class="status-dropdown">
    //            <button class="dropdown-toggle"
    //                    data-options='${jsonString2}'>
    //                ${item.vehicleTroughStatusName || ""}
    //            </button>
    //        </div>
    //    </td>
    //</tr>`;
    //}
    function renderRow_Bo_luot(item, isProcessed) {
        var date = new Date(item.vehicleWeighingTimeComeOut);
        let formatted =
            String(date.getHours()).padStart(2, '0') + ":" +
            String(date.getMinutes()).padStart(2, '0') + " " +
            String(date.getDate()).padStart(2, '0') + "/" +
            String(date.getMonth() + 1).padStart(2, '0') + "/" +
            date.getFullYear();
        var rankIcon = "";
        switch (item.rank) {
            case 0:
                rankIcon = "<img src='~/images/icons/icon-bac.png' class='rank-icon silver' />";
                break;
            case 1:
                rankIcon = "<img src='~/images/icons/icon-vang.png' class='rank-icon gold' />";
                break;
            case 2:
                rankIcon = "<img src='~/images/icons/icon-kim-cuong.png' class='rank-icon diamond' />";
                break;
        }
        return `
    <tr class="CartoFactory_${item.id}" data-queue="${item.recordNumber}" style="background: antiquewhite;">
        <td>${item.recordNumber}</td>
        <td class="customer-rank">${rankIcon} <span class="rank-text">${item.customerName}</span></td>
        <td>${item.driverName}</td>
        <td><a class="btn-detail"
                           data-id="${item.id}" style="cursor:pointer">${item.vehicleNumber}</a></td>
        <td>${formatted || ""}</td>
        <td>${item.vehicleLoadTaken > 0 ? item.vehicleLoadTaken.toLocaleString('en-US') : 0}</td>
        <td>
            <div class="status-dropdown">
                <button class="dropdown-toggle ${isProcessed ? "disabled" : ""}"
                        data-type="1"
                        data-options='${jsonString}'
                        ${isProcessed ? "disabled" : ""}>
                    ${item.troughTypeName || ""}
                </button>
            </div>
        </td>
      <td>
        <input type="text"
               class="input-form weight currency"
               value="${item.vehicleTroughWeight > 0 ? item.vehicleTroughWeight : ""}"
               placeholder="Vui lòng nhập"
               ${isProcessed ? "disabled" : ""} />
    </td>
        <td>
            <div class="status-dropdown">
                <button class="dropdown-toggle"
                        data-options='${jsonString2}'>
                    ${item.vehicleTroughStatusName || ""}
                </button>
            </div>
        </td>
    </tr>`;
    }



    // Hàm sắp xếp lại tbody theo QueueNumber tăng dần
    function sortTable_Da_SL() {
        const tbody = document.getElementById("dataBody-1");
        const rows = Array.from(tbody.querySelectorAll("tr"));

        // Chỉ mục của cột "Giờ cân xong đầu vào" (Điều chỉnh nếu cần)
        const TIME_COLUMN_INDEX = 4;

        // Hàm chuyển đổi chuỗi thời gian "HH:mm dd/MM/yyyy" thành đối tượng Date
        // Đây là bước quan trọng để so sánh thời gian chính xác
        const parseDateTime = (timeString) => {
            if (!timeString) return new Date(0); // Trả về ngày rất cũ nếu chuỗi rỗng

            // Ví dụ: "10:17 09/01/2025"
            const [timePart, datePart] = timeString.split(' ');
            if (!datePart || !timePart) return new Date(0);

            const [day, month, year] = datePart.split('/').map(Number);
            const [hours, minutes] = timePart.split(':').map(Number);

            // Tạo đối tượng Date (Lưu ý: Tháng trong JS bắt đầu từ 0)
            return new Date(year, month - 1, day, hours, minutes, 0);
        };

        rows.sort((a, b) => {
            // Lấy giá trị chuỗi thời gian từ ô tại chỉ mục đã cho
            const timeAString = a.cells[TIME_COLUMN_INDEX]?.textContent.trim() || '';
            const timeBString = b.cells[TIME_COLUMN_INDEX]?.textContent.trim() || '';

            // Chuyển đổi sang đối tượng Date để so sánh
            const dateA = parseDateTime(timeAString);
            const dateB = parseDateTime(timeBString);

            // Trả về kết quả so sánh (dateA - dateB cho sắp xếp tăng dần: cũ nhất -> mới nhất)
            return dateA.getTime() - dateB.getTime();
        });

        tbody.innerHTML = "";
        rows.forEach(r => tbody.appendChild(r));
    }
    function sortTable() {
        const tbody = document.getElementById("dataBody-0");
        const rows = Array.from(tbody.querySelectorAll("tr"));

        // Chỉ mục của cột "Giờ cân xong đầu vào" (Điều chỉnh nếu cần)
        const TIME_COLUMN_INDEX = 4;

        // Hàm chuyển đổi chuỗi thời gian "HH:mm dd/MM/yyyy" thành đối tượng Date
        // Đây là bước quan trọng để so sánh thời gian chính xác
        const parseDateTime = (timeString) => {
            if (!timeString) return new Date(0); // Trả về ngày rất cũ nếu chuỗi rỗng

            // Ví dụ: "10:17 09/01/2025"
            const [timePart, datePart] = timeString.split(' ');
            if (!datePart || !timePart) return new Date(0);

            const [day, month, year] = datePart.split('/').map(Number);
            const [hours, minutes] = timePart.split(':').map(Number);

            // Tạo đối tượng Date (Lưu ý: Tháng trong JS bắt đầu từ 0)
            return new Date(year, month - 1, day, hours, minutes, 0);
        };

        rows.sort((a, b) => {
            // Lấy giá trị chuỗi thời gian từ ô tại chỉ mục đã cho
            const timeAString = a.cells[TIME_COLUMN_INDEX]?.textContent.trim() || '';
            const timeBString = b.cells[TIME_COLUMN_INDEX]?.textContent.trim() || '';

            // Chuyển đổi sang đối tượng Date để so sánh
            const dateA = parseDateTime(timeAString);
            const dateB = parseDateTime(timeBString);

            // Trả về kết quả so sánh (dateA - dateB cho sắp xếp tăng dần: cũ nhất -> mới nhất)
            return dateA.getTime() - dateB.getTime();
        });

        tbody.innerHTML = "";
        rows.forEach(r => tbody.appendChild(r));
    }
    // Hàm cập nhật trạng thái máng (client-side)



    // Nhận data mới từ server
    connection.off("ListCarCall_Da_SL");
    connection.on("ListCarCall_Da_SL", function (item) {
        $('.CartoFactory_' + item.id).remove();
        const tbody = document.getElementById("dataBody-1");
        tbody.insertAdjacentHTML("beforeend", renderRow(item, true));
        sortTable_Da_SL(); // sắp xếp lại ngay khi thêm
        _cartcalllist.autoRowspanWithCondition("ListCarCall-1", [0, 1, 2, 3, 4, 5, 7, 8], [0, 1, 2, 3, 4, 5]);
        requestAnimationFrame(() => {
            _cartcalllist.initMangStatus();
        });
    });
    connection.off("ListCarCall_Bo_LUOT");
    connection.on("ListCarCall_Bo_LUOT", function (item) {
        $('.CartoFactory_' + item.id).remove();
        const tbody = document.getElementById("dataBody-1");
        tbody.insertAdjacentHTML("beforeend", renderRow_Bo_luot(item, true));
        sortTable_Da_SL(); // sắp xếp lại ngay khi thêm
        _cartcalllist.autoRowspanWithCondition("ListCarCall-1", [0, 1, 2, 3, 4, 5, 7, 8], [0, 1, 2, 3, 4, 5]);
        requestAnimationFrame(() => {
            _cartcalllist.initMangStatus();
        });
    });
    // Nhận data từ server (SignalR)
    connection.off("UpdateMangStatus");
    connection.on("UpdateMangStatus", function (oldMangId, newMangId, carId) {
        // ✅ Update máng mới thành "Đang xử lý"
        if (newMangId !== null && newMangId !== undefined) {
            $("#input" + (parseInt(newMangId))).val("Đang xử lý")

                .removeClass("empty").addClass("processing");
        }

        // ✅ Kiểm tra máng cũ: nếu không còn xe nào ở máng đó thì reset về "Trống"
        if (oldMangId !== null && oldMangId !== undefined && oldMangId != newMangId) {
            const hasOtherCars = $("#dataBody-0 tr, #dataBody-1 tr").toArray().some(tr => {
                return $(tr).find("button[data-type='1']").text().trim() === "Máng " + (parseInt(oldMangId));
            });


            $("#input" + (parseInt(oldMangId))).val("Trống")

                .removeClass("processing").addClass("empty");

        }

        // ✅ Update luôn dropdown text trong bảng cho xe đó
        const $row = $(".CartoFactory_" + carId);
        if ($row.length) {
            $row.find(".dropdown-toggle[data-type='1']").text("Máng " + (parseInt(newMangId)));
        }
    });



    connection.off("ListCarCall");
    connection.on("ListCarCall", function (item) {
        const tbody = document.getElementById("dataBody-0");
        $('.CartoFactory_' + item.id).remove();
        tbody.insertAdjacentHTML("beforeend", renderRow(item, false));
        sortTable(); // sắp xếp lại ngay khi thêm
        _cartcalllist.autoRowspanWithCondition("ListCarCall-0", [0, 1, 2, 3, 4, 5, 7, 8], [0, 1, 2, 3, 4, 5]);
        requestAnimationFrame(() => {
            _cartcalllist.initMangStatus();
        });
    });

    // Nhận data mới từ gọi xe cân đầu vào
    connection.off("ListWeighedInput_Da_SL");
    connection.on("ListWeighedInput_Da_SL", function (item) {
        const tbody = document.getElementById("dataBody-0");
        tbody.insertAdjacentHTML("beforeend", renderRow(item, false));
        sortTable();
        _cartcalllist.autoRowspanWithCondition("ListCarCall-0", [0, 1, 2, 3, 4, 5, 7, 8], [0, 1, 2, 3, 4, 5]);
    });
    connection.off("ListWeighedInput");
    connection.on("ListWeighedInput", function (item) {
        $('#dataBody-0').find('.CartoFactory_' + item.id).remove();

    });
    connection.off("ListVehicles_Da_SL");
    connection.on("ListVehicles_Da_SL", function (item) {
        $('.CartoFactory_' + item.id).remove();
    });
    connection.off("ListVehicles");
    connection.on("ListVehicles", function (item) {
        const tbody = document.getElementById("dataBody-1");
        tbody.insertAdjacentHTML("beforeend", renderRow(item, true));
        sortTable_Da_SL(); // sắp xếp lại ngay khi thêm
        _cartcalllist.autoRowspanWithCondition("ListCarCall-1", [0, 1, 2, 3, 4, 5, 7, 8], [0, 1, 2, 3, 4, 5]);
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
    $(document).on('click', '.check-troughWeight', function (e) {
        var element = $(this);
        var TroughWeightId = element.closest('tr').find('input.TroughWeightId').val();
        var weight_TroughWeightId = element.closest('td').find('input.weight').val();
        _cartcalllist.UpdateTroughWeight(TroughWeightId, weight_TroughWeightId);
    });
    $(document).on('click', '.cancel-troughWeight', function (e) {
        var element = $(this);
        var TroughWeightId = element.closest('tr').find('input.TroughWeightId').val();
        _cartcalllist.CancelTroughWeight(TroughWeightId, element);
    });
    $(document).on('change', '.CartoFactory_TroughWeight', function () {
        var element = $(this);
        element.addClass('weight');

        // Gọi ajax hoặc function bạn cần
    });
    $(document).on('blur', '#dataBody-0 .Note', function () {
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
            data: { id: id, status: 0, type: 13, weight: 0, Note: note },
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
});
var _cartcalllist = {
    // ✅ Hàm đồng bộ máng khi vừa load trang hoặc reload data
    // ✅ Đồng bộ trạng thái máng khi load trang hoặc reload data
    initMangStatus: function () {
        // Giả sử có 5 máng, bạn thay bằng số máng thực tế
        for (let mangIndex = 1; mangIndex <= 9; mangIndex++) {
            let mangName = "Máng " + mangIndex;

            // 🔎 Kiểm tra xem có xe nào trong máng này chưa hoàn thành không
            let stillHasCar = $("#dataBody-0 tr").toArray().some(tr => {
                let btnText = $(tr).find("button[data-type='1']").text().replace(/\s+/g, ' ').trim();  // 👈 gộp nhiều space thành 1

                let trangThai = $(tr).find("td:last .dropdown-toggle").text().trim();
                return btnText === mangName && trangThai !== "Hoàn thành" && trangThai !== "Bỏ lượt";
            });

            if (stillHasCar) {
                _cartcalllist.updateMangStatus(mangIndex, "Đang xử lý");
            } else {
                _cartcalllist.updateMangStatus(mangIndex, "Trống");
            }
        }
    },

    // ✅ Hàm cập nhật input trạng thái máng
    updateMangStatus: function (mangIndex, statusText) {
        const $input = $("#input" + mangIndex);

        if ($input.length) {
            $input.val(statusText);

            if (statusText === "Trống") {
                $input.removeClass("processing").addClass("empty");
            } else {
                $input.removeClass("empty").addClass("processing");
            }
        }
    },
    init: function () {
        _cartcalllist.ListCartoFactory();
        _cartcalllist.ListCartoFactory_Da_SL();
    },
    ListCartoFactory: function () {
        var model = {
            VehicleNumber: $('#input_chua_xu_ly').val() != undefined && $('#input_chua_xu_ly').val() != "" ? $('#input_chua_xu_ly').val().trim() : "",
            PhoneNumber: $('#input_chua_xu_ly').val() != undefined && $('#input_chua_xu_ly').val() != "" ? $('#input_chua_xu_ly').val().trim() : "",
            VehicleStatus: 0,
            LoadType: null,
            VehicleWeighingType: 0,
            VehicleTroughStatus: null,
            TroughType: null,
            VehicleWeighingStatus: null,
            LoadingStatus: 0,
            VehicleWeighedstatus: 0,
            type: 0,
        }
        $.ajax({
            url: "/ListCar/ListCarCallView",
            type: "post",
            data: { SearchModel: model },
            success: function (result) {
                $('#imgLoading').hide();
                $('#data_chua_xu_ly').html(result);
                _cartcalllist.initMangStatus();
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
            VehicleWeighingType: 0,
            VehicleTroughStatus: "0,4",
            TroughType: null,
            VehicleWeighingStatus: null,
            LoadingStatus: 0,
            VehicleWeighedstatus: 0,
            type: 1,
        }
        $.ajax({
            url: "/ListCar/ListCarCallView",
            type: "post",
            data: { SearchModel: model },
            success: function (result) {
                $('#imgLoading').hide();
                $('#data_da_xu_ly').html(result);
                _cartcalllist.initMangStatus();
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                console.log("Status: " + textStatus);
            }
        });
    },
    UpdateStatus: async function (id, status, type, weight) {
        var status_type = 1
        $.ajax({
            url: "/Car/UpdateStatus",
            type: "post",
            data: { id: id, status: status, type: type, weight: weight },
            success: function (result) {
                status_type = result.status;
                if (result.status == 0) {
                    _msgalert.success(result.msg);

                    // ✅ chỉ remove row nếu cập nhật thành công
                    if (type == 6) {
                        if (parseInt(status) == 0) {
                            $('#dataBody-0').find('.CartoFactory_' + id).remove();
                        } else {
                            $('#dataBody-1').find('.CartoFactory_' + id).remove();
                        }
                    }

                    // 🔥 Sau khi update → reload lại dữ liệu cả 2 bảng
                    _cartcalllist.ListCartoFactory();
                    _cartcalllist.ListCartoFactory_Da_SL();

                } else {
                    _msgalert.error(result.msg);
                }

            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                console.log("Status: " + textStatus);
            }

        });
        return await status_type;
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
    UpdateTroughWeight: function (id, weight_TroughWeightId) {

        $.ajax({
            url: "/Car/UpdateTroughWeight",
            type: "post",
            data: { id: id, VehicleTroughWeight: weight_TroughWeightId },
            success: function (result) {
                if (result.status == 0) {
                    _msgalert.success(result.msg);
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

        $.ajax({
            url: "/Car/CancelTroughWeight",
            type: "post",
            data: { id: id },
            success: function (result) {
                if (result.status == 0) {
                    element.closest('td').find('input.weight').val(result.data);
                    element.closest('td').find('input.weight').removeClass("weight");
                    _msgalert.success(result.msg);
                } else {
                    _msgalert.error(result.msg);
                }
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                console.log("Status: " + textStatus);
            }
        });
    },

}