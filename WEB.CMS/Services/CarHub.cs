using Entities.ViewModels.Car;
using Microsoft.AspNetCore.SignalR;

namespace WEB.CMS.Services
{
    public class CarHub : Hub
    {
        // Chỉ cần broadcast, client sẽ xử lý UI
        public async Task SendRegistration(CartoFactoryModel record)
        {
            await Clients.All.SendAsync("CarRegistration", record);
        }
    }
}
