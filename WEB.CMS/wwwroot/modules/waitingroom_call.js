$(document).ready(function () {
    _waiting_room.GetList();
    _waiting_room.GetList_Da_SL();
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
            <td>
                <div>${item.driverName}</div>
                <div>${item.phoneNumber}</div>
            </td>
           

            <td> ${item.csNotes == null ? '' : item.csNotes} </td>
           <td>${html_tt}</td>
  
            <td>
                 ${item.loadTypeName == null ? '' : item.loadTypeName}
            </td>
        </tr>`;
    }

    function sortTable() {
        const tbody = document.getElementById("dataBody-0");
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

    function sortTable_Da_SL() {
        const tbody = document.getElementById("dataBody-1");
        if (!tbody) return;
        const rows = Array.from(tbody.querySelectorAll("tr"));

        rows.sort((a, b) => {
            const timeA = parseDateTime(a.dataset.queue);
            const timeB = parseDateTime(b.dataset.queue);
            return timeB - timeA; // giảm dần (mới nhất lên đầu)
        });

        tbody.innerHTML = "";
        rows.forEach(r => tbody.appendChild(r));
        updateCallCards();
    }

    function updateCallCards() {
        const tbody = document.getElementById("dataBody-1");
        if (!tbody) return;
        const rows = Array.from(tbody.querySelectorAll("tr")).slice(0, 3);
        let html = '';

        rows.forEach((row, index) => {
            const cells = row.querySelectorAll('td');
            if (cells.length < 8) return;

            const plate = cells[2].innerText.trim();
            const customer = cells[3].innerText.trim();
            const driver = cells[4].querySelector('div:first-child').innerText.trim();
            const note = cells[5].innerText.trim();
            const loadType = cells[7].innerText.trim();
            
            // Assume "Dock" might be in the note or default to "CHỜ..."
            let dock = "";
            if (note.toUpperCase().includes("DOCK")) {
                const match = note.match(/DOCK\s*(\d+)/i);
                if (match) dock = match[0].toUpperCase();
            }

            html += `
            <div class="car-card">
                <div class="car-card-header">
                    <span class="turn-badge">Lượt ${index + 1}</span>
                    <span class="dock-info">${dock}</span>
                </div>
                <div class="plate-number">${plate}</div>
                <div class="driver-info">
                    <svg viewBox="0 0 24 24"><path d="M12 12c2.21 0 4-1.79 4-4s-1.79-4-4-4-4 1.79-4 4 1.79 4 4 4zm0 2c-2.67 0-8 1.34-8 4v2h16v-2c0-2.66-5.33-4-8-4z"/></svg>
                    <span>${driver}</span>
                </div>
                ${note ? `
                <div class="cs-note">
                   CS Ghi chú: ${note}
                </div>` : ''}
            </div>`;
        });

        $('#CarCallCards').html(html);
    }

    // SignalR handlers
    connection.on("ListProcessingIsLoading_Da_SL", function (item) {
        const tbody = document.getElementById("dataBody-1");
        $('.waiting_room_' + item.id).remove();
        tbody.insertAdjacentHTML("afterbegin", renderRow(item));
        sortTable_Da_SL();
    });

    connection.on("ListProcessingIsLoading", function (item) {
        const tbody = document.getElementById("dataBody-0");
        $('.waiting_room_' + item.id).remove();
        tbody.insertAdjacentHTML("beforeend", renderRow(item));
        sortTable();
    });

    function parseDateTime(str) {
        if (!str) return 0;
        const [time, date] = str.split(" ");
        const [hour, minute] = time.split(":").map(Number);
        const [day, month, year] = date.split("/").map(Number);
        return new Date(year, month - 1, day, hour, minute).getTime();
    };

    // Export to global scope if needed
    window.updateCallCards = updateCallCards;
});

var _waiting_room = {
    GetList: function () {
        var model = {
            VehicleNumber: "",
            PhoneNumber: "",
            VehicleStatus: 0,
            LoadType: null,
            type: 0,
        }
        $.ajax({
            url: "/WaitingRoom/GetList",
            type: "post",
            data: { SearchModel: model },
            success: function (result) {
                $('#Processing_Is_Loading').html(result);
            }
        });
    },
    GetList_Da_SL: function () {
        var model = {
            VehicleNumber: "",
            PhoneNumber: "",
            VehicleStatus: 0,
            LoadingStatus: 0,
            type: 1,
        }
        $.ajax({
            url: "/WaitingRoom/GetList",
            type: "post",
            data: { SearchModel: model },
            success: function (result) {
                $('#Processing_Is_Loading_Da_SL').html(result);
                setTimeout(updateCallCards, 500); // Small delay to ensure DOM is ready
            }
        });
    },
}
