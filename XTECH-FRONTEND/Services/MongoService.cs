using Microsoft.AspNetCore.Razor.TagHelpers;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using MongoDB.Driver.Core.Events;
using Newtonsoft.Json;
using XTECH_FRONTEND.IRepositories;
using XTECH_FRONTEND.Model;
using XTECH_FRONTEND.Utilities;

namespace XTECH_FRONTEND.Services
{
    public class MongoService : IMongoService
    {

        private readonly IConfiguration _configuration;
        public MongoService(IConfiguration configuration)
        {
            _configuration = configuration;

        }
        public async Task<long> Insert(RegistrationRecord model)
        {
            try
            {
                string url = "mongodb://" + _configuration["MongoServer:user"] + ":" + _configuration["MongoServer:pwd"] + "@" + _configuration["MongoServer:Host"] + ":" + _configuration["MongoServer:Port"] ;
                var client = new MongoClient(url);

                IMongoDatabase db = client.GetDatabase(_configuration["MongoServer:catalog_log"]);
                RegistrationRecord log = new RegistrationRecord()
                {
                    _id = ObjectId.GenerateNewId().ToString(),
                    PhoneNumber = model.PhoneNumber,
                    PlateNumber = model.PlateNumber.ToUpper(),
                    Name = model.Name,
                    Referee = model.Referee.ToUpper(),
                    GPLX = model.GPLX.ToUpper(),
                    QueueNumber = model.QueueNumber,
                    RegistrationTime = model.RegistrationTime,
                    ZaloStatus = model.ZaloStatus,
                    Camp = model.Camp

                };
                IMongoCollection<RegistrationRecord> affCollection = db.GetCollection<RegistrationRecord>(_configuration["MongoServer:Data_Car"]);
                await affCollection.InsertOneAsync(log);
                return model.QueueNumber;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("Insert - MongoService: " + ex.Message);
            }
            return 0;
        }
        public async Task<int> CheckPlateNumber(string PlateNumber)
        {
            var list_data = new List<RegistrationRecord>();
            try
            {
                string url = "mongodb://" + _configuration["MongoServer:user"] + ":" + _configuration["MongoServer:pwd"] + "@" + _configuration["MongoServer:Host"] + ":" + _configuration["MongoServer:Port"] + "/" + _configuration["MongoServer:catalog_log"];
                var client = new MongoClient(url);

                IMongoDatabase db = client.GetDatabase(_configuration["MongoServer:catalog_log"]);

                var todayStart = DateTime.Today;
                var cutoffTime = todayStart.AddHours(18);
                var collection = db.GetCollection<RegistrationRecord>(_configuration["MongoServer:Data_Car"]);
                var filter = Builders<RegistrationRecord>.Filter.Empty;
                filter &= Builders<RegistrationRecord>.Filter.Eq(n => n.PlateNumber, PlateNumber);
                filter &= Builders<RegistrationRecord>.Filter.Gte("RegistrationTime", cutoffTime.AddDays(-1));
                filter &= Builders<RegistrationRecord>.Filter.Lte("RegistrationTime", cutoffTime);
                list_data = collection.Find(filter).ToList();
                if (list_data != null && list_data.Count > 0)
                {
                    return  1;
                }
                return 0;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("CheckPlateNumber - MongoService. " + JsonConvert.SerializeObject(ex));
            }
            return 0;
        }
        public List<RegistrationRecordMongo> GetList()
        {
            var list = new List<RegistrationRecordMongo>();
            try
            {
                var now = DateTime.Now;
                var expireAt = new DateTime(now.Year, now.Month, now.Day, 17, 55, 0);
                string url = "mongodb://" + _configuration["MongoServer:user"] + ":" + _configuration["MongoServer:pwd"] + "@" + _configuration["MongoServer:Host"] + ":" + _configuration["MongoServer:Port"] ;
                var client = new MongoClient(url);
                IMongoDatabase db = client.GetDatabase(_configuration["MongoServer:catalog_log"]);
      
                var collection = db.GetCollection<RegistrationRecordMongo>(_configuration["MongoServer:Data_Car"]);
                var filter = Builders<RegistrationRecordMongo>.Filter.Empty;
                if (now >= expireAt)
                {
                    filter &= Builders<RegistrationRecordMongo>.Filter.Gte("RegistrationTime", expireAt);
                }
                else
                {
                    filter &= Builders<RegistrationRecordMongo>.Filter.Gte("RegistrationTime", expireAt.AddDays(-1));
                }

                var S = Builders<RegistrationRecordMongo>.Sort.Ascending("QueueNumber");
                list = collection.Find(filter).Sort(S).ToList();
                return list;

            }
            catch (Exception ex)
            {

                LogHelper.InsertLogTelegram("GetList - MongoService. " + JsonConvert.SerializeObject(ex));
            }
            return list;
        }
    }
}
