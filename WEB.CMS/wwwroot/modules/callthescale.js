$(document).ready(function () {
    _Call_The_Scale.init();
   
    $(document).on('click', '.open-audio', function (e) {
        // 'this' chính là phần tử .open-audio được click
        var $row = $(this).closest('tr');          // Tìm <tr> cha gần nhất
        /*const $row = $currentBtn.closest('tr');*/
        var id = 0;
        if ($row.length) {
            const classAttr = $row.attr('class');
            const match = classAttr.match(/CartoFactory_(\d+)/);
            if (match && match[1]) {
                id = match[1];
            }
        }
        // Kiểm tra ID hợp lệ
        if (id === 0) {
            console.error('Lỗi: Không tìm thấy ID hàng.');
            return;
        }

        const audioPlayer = document.getElementById('myAudio_' + id);

        if (audioPlayer) {
            // 3. Vô hiệu hóa và ẩn CHỈ nút hiện tại đang được click

            // Vô hiệu hóa tất cả các nút còn lại (nếu bạn muốn)
            $('.open-audio').prop('disabled', true);
            $('.open-audio').hide();
            // Phát âm thanh
            audioPlayer.play();

            // 4. Bật lại nút khi phát xong (Sử dụng sự kiện 'onended' của Audio HTML5)
            audioPlayer.onended = function () {

                // Bật lại các nút khác
                $('.open-audio').prop('disabled', false);
                $('.open-audio').show();
                console.log(`✅ Đọc xong ID ${id}, nút đã bật lại.`);
            };

            // Xử lý lỗi phát (ví dụ: file không tồn tại)
            audioPlayer.onerror = function () {
                $('.open-audio').prop('disabled', false);
                console.error(`❌ Lỗi phát âm thanh cho ID: ${id}`);
            }
        } else {
            console.error(`Lỗi: Không tìm thấy thẻ audio có ID: myAudio_${id}`);
        }
    });
    var input_Call_The_Scale_Chua_SL = document.getElementById("input_Call_The_Scale_Chua_SL");
    input_Call_The_Scale_Chua_SL.addEventListener("keypress", function (event) {
        // If the user presses the "Enter" key on the keyboard
        if (event.key === "Enter") {
            // Cancel the default action, if needed
            event.preventDefault();
            // Trigger the button element with a click
            _Call_The_Scale.ListCallTheScale();
            _Call_The_Scale.ListCallTheScale_2();
        }
    });
    input_Call_The_Scale_Chua_SL.addEventListener("keyup", function (event) {
        _Call_The_Scale.ListCallTheScale();
        _Call_The_Scale.ListCallTheScale_2();
    });
    var input_Call_The_Scale_Da_SL = document.getElementById("input_Call_The_Scale_Da_SL");
    input_Call_The_Scale_Da_SL.addEventListener("keypress", function (event) {
        // If the user presses the "Enter" key on the keyboard
        if (event.key === "Enter") {
            // Cancel the default action, if needed
            event.preventDefault();
            // Trigger the button element with a click
            _Call_The_Scale.ListCallTheScale_Da_SL();
        }
    });
    input_Call_The_Scale_Da_SL.addEventListener("keyup", function (event) {
        _Call_The_Scale.ListCallTheScale_Da_SL();
    });
    // 🕵️ Lắng nghe sự kiện toàn cục để đóng menu khi cần
    $(window).on('resize.dropdown', function () {
        if ($menu) closeMenu();
    });

    let $menu = null;
    let $currentBtn = null;

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

                const cls = $active.attr('class').split(/\s+/)
                    .filter(c => c !== 'active')[0] || '';

                var Status_type = 0;
                $.ajax({
                    url: "/Car/UpdateStatus",
                    type: "post",
                    data: { id: id_row, status: val_TT, type: 3 },
                    success: function (result) {
                        status_type = result.status;
                        if (result.status == 0) {
                            $currentBtn
                                .text(text)
                                .removeClass(function (_, old) {
                                    return (old.match(/(^|\s)status-\S+/g) || []).join(' ');
                                }) // xoá các class status- cũ
                                .addClass(cls); // gắn class mới (status-arrived, status-blank…)


                            if (val_TT == 1) {
                                $('#dataBody-1').find('.CartoFactory_' + id_row).remove();


                            } else {
                                $('#dataBody-0-0').find('.CartoFactory_' + id_row).remove();
                                $('#dataBody-0-1').find('.CartoFactory_' + id_row).remove();

                            }
                        }
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

    // Nhận data từ server (SignalR)
    connection.off("UpdateMangStatus");
    connection.on("UpdateMangStatus", function (oldMangId, newMangId, carId) {
        // ✅ Update máng mới thành "Đang xử lý"
        if (newMangId !== null && newMangId !== undefined) {
            _Call_The_Scale.updateMangStatus(newMangId, "Đang xử lý");
        }

 

        // ✅ Update luôn dropdown text trong bảng cho xe đó
        const $row = $(".CartoFactory_" + carId);
        if ($row.length) {
            $row.find(".dropdown-toggle[data-type='1']").text("Máng " + (parseInt(newMangId)));
        }
    });

    const AllCode = [
        { Description: "Blank", CodeValue: "1" },
        { Description: "Đã vào cân", CodeValue: "0" },
        // Add more objects as needed
    ];
    // Create a new array of objects in the desired format
    const options = AllCode.map(allcode => ({
        text: allcode.Description,
        value: allcode.CodeValue
    }));
    const jsonString = JSON.stringify(options);
    // Hàm render row
    function renderRow_Da_SL(item) {
        var date = new Date(item.processingIsLoadingDate);
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
        return `
        <tr class="CartoFactory_${item.id}" data-queue="${formatted}" >
            <td>${item.recordNumber}</td>
            <td>${formatted2}</td>
            <td>${item.customerName}</td>
            <td>${item.driverName}</td>
            <td>${item.phoneNumber}</td>
            <td>${item.vehicleNumber}</td>
            <td>${item.loadTypeName}</td>
            <td>
                <div class="status-dropdown">
                    <button class="dropdown-toggle " data-options='${jsonString}'>
                        ${item.vehicleWeighingTypeName}
                    </button>
                </div>

            </td>
            <td> <button class="open-audio">
                                <span class="icon "><img src="/images/graphics/SpeakerHigh.png" height="25" alt=""></span>
                            <audio id="myAudio_${item.id}" controls style="display:none!important;">
                                                                    <source src="${item.audioPath}" type="audio/mpeg" />
                                                                </audio>
                            </button></td>
        </tr>`;
    }
    function renderRow(item) {
        var date = new Date(item.processingIsLoadingDate);
        let formatted =
            String(date.getHours()).padStart(2, '0') + ":" +
            String(date.getMinutes()).padStart(2, '0') + " " +
            String(date.getDate()).padStart(2, '0') + "/" +
            String(date.getMonth() + 1).padStart(2, '0') + "/" +
            date.getFullYear();
        return `
        <tr class="CartoFactory_${item.id}" data-queue="${formatted}" >
            <td>${item.recordNumber}</td>
           <td>${item.vehicleNumber}</td>
            <td>${item.phoneNumber}</td>
            <td>${item.loadTypeName}</td>
            <td>
                <div class="status-dropdown">
                    <button class="dropdown-toggle " data-options='${jsonString}'>
                        ${item.vehicleWeighingTypeName}
                    </button>
                </div>

            </td>
            <td> <button class="open-audio">
                                <span class="icon "><img src="/images/graphics/SpeakerHigh.png" height="25" alt=""></span>
                                 <audio id="myAudio_${item.id}" controls style="display:none!important;">
                                                                    <source src="${item.audioPath}" type="audio/mpeg" />
                                                                </audio>
                            </button></td>
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
    connection.off("ListCallTheScale_Da_SL");
    connection.on("ListCallTheScale_Da_SL", function (item) {
        const tbody = document.getElementById("dataBody-1");
        $('.CartoFactory_' + item.id).remove();
        tbody.insertAdjacentHTML("beforeend", renderRow_Da_SL(item));
        sortTable_Da_SL(); // sắp xếp lại ngay khi thêm
    });
    connection.off("ListCallTheScale_0");
    connection.on("ListCallTheScale_0", function (item) {
        const tbody = document.getElementById("dataBody-0-0");
        $('.CartoFactory_' + item.id).remove();
        tbody.insertAdjacentHTML("beforeend", renderRow(item));
        sortTable(); // sắp xếp lại ngay khi thêm
    });
    connection.off("ListCallTheScale_1");
    connection.on("ListCallTheScale_1", function (item) {
        const tbody = document.getElementById("dataBody-0-1");
        $('.CartoFactory_' + item.id).remove();
        tbody.insertAdjacentHTML("beforeend", renderRow(item));
        sortTable(); // sắp xếp lại ngay khi thêm
    });
    //sử lý đăng tải
    connection.off("ListProcessingIsLoading_Da_SL");
    connection.on("ListProcessingIsLoading_Da_SL", function (item) {
        if (item.loadType == 0) {
            const tbody = document.getElementById("dataBody-0-0");
            tbody.insertAdjacentHTML("beforeend", renderRow(item));
            sortTable(); 
        } else {
            const tbody = document.getElementById("dataBody-0-1");
            tbody.insertAdjacentHTML("beforeend", renderRow(item));
            sortTable(); 
        }     
    });
    connection.off("ListProcessingIsLoading");
    connection.on("ListProcessingIsLoading", function (item) {
        $('#dataBody-0-0').find('.CartoFactory_' + item.id).remove();
        $('#dataBody-0-1').find('.CartoFactory_' + item.id).remove();
    });
    connection.off("ListWeighedInput_Da_SL");
    connection.on("ListWeighedInput_Da_SL", function (item) {
        $('.CartoFactory_' + item.id).remove();
    });
    connection.off("ListWeighedInput");
    connection.on("ListWeighedInput", function (item) {
        const tbody = document.getElementById("dataBody-1");
        tbody.insertAdjacentHTML("beforeend", renderRow_Da_SL(item));
        sortTable_Da_SL(); // sắp xếp lại ngay khi thêm
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
});
var _Call_The_Scale = {


    // ✅ Hàm cập nhật input trạng thái máng
    updateMangStatus: function (mangIndex, statusText) {
        const $input = $("#input" + mangIndex);

        if ($input.length) {
            $input.val(statusText);

            if (statusText === "Trống") {
                $input.removeClass("processing").addClass("empty").css({"background": "", "color": ""});
            } else {
                $input.removeClass("empty").addClass("processing").css({"background": "red", "color": "white"});
            }
        }
    },
    init: function () {
        _Call_The_Scale.ListCallTheScale();
        _Call_The_Scale.ListCallTheScale_2();
        _Call_The_Scale.ListCallTheScale_Da_SL();
    },
    ListCallTheScale: function () {
        var model = {
            VehicleNumber: $('#input_Call_The_Scale_Chua_SL').val() != undefined && $('#input_Call_The_Scale_Chua_SL').val() != "" ? $('#input_Call_The_Scale_Chua_SL').val().trim() : "",
            PhoneNumber: $('#input_Call_The_Scale_Chua_SL').val() != undefined && $('#input_Call_The_Scale_Chua_SL').val() != "" ? $('#input_Call_The_Scale_Chua_SL').val().trim() : "",
            VehicleStatus: 0,
            LoadType: 0,
            VehicleWeighingType: null,
            VehicleTroughStatus: null,
            TroughType: null,
            VehicleWeighingStatus: null,
            LoadingStatus: 0,
            type: 0,
        }
        $.ajax({
            url: "/Car/ListCallTheScale",
            type: "post",
            data: { SearchModel: model },
            success: function (result) {
                $('#imgLoading').hide();
                $('#Call_The_Scale_Chua_SL').html(result);
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                console.log("Status: " + textStatus);
            }
        });
    },
    ListCallTheScale_2: function () {
        var model = {
            VehicleNumber: $('#input_Call_The_Scale_Chua_SL').val() != undefined && $('#input_Call_The_Scale_Chua_SL').val() != "" ? $('#input_Call_The_Scale_Chua_SL').val().trim() : "",
            PhoneNumber: $('#input_Call_The_Scale_Chua_SL').val() != undefined && $('#input_Call_The_Scale_Chua_SL').val() != "" ? $('#input_Call_The_Scale_Chua_SL').val().trim() : "",
            VehicleStatus: 0,
            LoadType: 1,
            VehicleWeighingType: null,
            VehicleTroughStatus: null,
            TroughType: null,
            VehicleWeighingStatus: null,
            LoadingStatus: 0,
            type: 0,
        }
        $.ajax({
            url: "/Car/ListCallTheScale",
            type: "post",
            data: { SearchModel: model },
            success: function (result) {
                $('#imgLoading').hide();
                $('#Call_The_Scale_Chua_SL_2').html(result);
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                console.log("Status: " + textStatus);
            }
        });
    },
    ListCallTheScale_Da_SL: function () {
        var model = {
            VehicleNumber: $('#input_Call_The_Scale_Da_SL').val() != undefined && $('#input_Call_The_Scale_Da_SL').val() != "" ? $('#input_Call_The_Scale_Da_SL').val().trim() : "",
            PhoneNumber: $('#input_Call_The_Scale_Da_SL').val() != undefined && $('#input_Call_The_Scale_Da_SL').val() != "" ? $('#input_Call_The_Scale_Da_SL').val().trim() : "",
            VehicleStatus: 0,
            LoadType: null,
            VehicleWeighingType: 0,
            VehicleTroughStatus: null,
            TroughType: null,
            VehicleWeighingStatus: null,
            LoadingStatus: 0,
            type:1,
        }
        $.ajax({
            url: "/Car/ListCallTheScale",
            type: "post",
            data: { SearchModel: model },
            success: function (result) {
                $('#imgLoading').hide();
                $('#Call_The_Scale_Da_SL').html(result);
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                console.log("Status: " + textStatus);
            }
        });
    },
    UpdateStatus: function (id, status, type) {
        var status_type = 0
       
        return status_type;
    },
}