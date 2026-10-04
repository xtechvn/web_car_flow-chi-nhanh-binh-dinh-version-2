using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using XTECH_FRONTEND.Model;

namespace XTECH_FRONTEND.Repositories
{
    public class RegistrationHub : Hub
    {
        // Chỉ cần broadcast, client sẽ xử lý UI
        public async Task SendRegistration(RegistrationRecordMongo record)
        {
            await Clients.All.SendAsync("ReceiveRegistration", record);
        }
    }
}
