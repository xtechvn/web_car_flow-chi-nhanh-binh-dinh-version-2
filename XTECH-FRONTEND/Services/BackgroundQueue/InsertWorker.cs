using Google.Apis.Sheets.v4.Data;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Configuration;
using System.Text;
using XTECH_FRONTEND.Model;
using XTECH_FRONTEND.Services.RedisWorker;
using XTECH_FRONTEND.Utilities;

namespace XTECH_FRONTEND.Services.BackgroundQueue
{
    public class InsertWorker : BackgroundService
    {
        private readonly IInsertQueue _queue;
        private readonly IServiceProvider _serviceProvider;
        private readonly IHttpClientFactory _factory;
        private readonly RedisConn redisService;
        private readonly IConfiguration _configuration;

        public InsertWorker(
            IInsertQueue queue,
            IServiceProvider serviceProvider,
            IHttpClientFactory factory, IConfiguration configuration)
        {
            _queue = queue;
            _serviceProvider = serviceProvider;
            _factory = factory;
            _configuration = configuration;
            redisService = new RedisConn(configuration);
            redisService.Connect();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var job = await _queue.DequeueAsync(stoppingToken);

                // chạy async tách riêng -> không block worker
                _ = Task.Run(() => ProcessJob(job, stoppingToken), stoppingToken);
            }
        }

        private async Task ProcessJob(InsertJob job, CancellationToken token)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<InsertWorker>>();

                var client = _factory.CreateClient("InsertClient");

                var json = Newtonsoft.Json.JsonConvert.SerializeObject(job.Data);

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(
                    "http://api.longancargill.com/api/vehicleInspection/insert",
                    content,
                    token
                );

                if (!response.IsSuccessStatusCode)
                {
                    logger.LogError("InsertWorker: Failed to call API");
                }
            }
            catch (Exception ex)
            {
                string cache_name = "CARGLL_BinhDinh";
                var data_list = new List<CarRegistrationResponse>();
                var data = await redisService.GetAsync(cache_name, Convert.ToInt32(_configuration["Redis:Database:db_common"]));
               
                if (data != null && data.Trim() != "")
                {
                    data_list = JsonConvert.DeserializeObject<List<CarRegistrationResponse>>(data);
                    data_list.Add(job.Data);
                }
                redisService.Set(cache_name, JsonConvert.SerializeObject(data_list), Convert.ToInt32(_configuration["Redis:Database:db_common"]));

                LogHelper.InsertLogTelegram("InsertWorker error: " + ex.Message);
            }
        }
    }

}
