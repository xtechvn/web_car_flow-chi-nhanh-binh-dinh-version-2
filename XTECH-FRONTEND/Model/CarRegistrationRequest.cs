using MongoDB.Bson.Serialization.Attributes;
using XTECH_FRONTEND.Utilities;

namespace XTECH_FRONTEND.Model
{
    public class CarRegistrationRequest
    {
        public string PlateNumber { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Referee { get; set; } = string.Empty;
        public string GPLX { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Camp { get; set; } = string.Empty;
        public string Timedow { get; set; } = string.Empty;
    }

    public class CarRegistrationResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int? QueueNumber { get; set; }
        public DateTime? RegistrationTime { get; set; }
        public string? PlateNumber { get; set; }
        public string? PhoneNumber { get; set; }
        public int? RemainingTimeMinutes { get; set; }
        public string? ZaloStatus { get; set; }
        public string? Camp { get; set; }
        public string GPLX { get; set; } 
        public string Referee { get; set; } 
        public string Name { get; set; } 
        public int LocationType { get; set; } 

   
    }

    public class RegistrationRecord
    {
        public string _id { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public string PlateNumber { get; set; } = string.Empty;
        public string Referee { get; set; } = string.Empty;
        public string GPLX { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int QueueNumber { get; set; }
        public DateTime RegistrationTime { get; set; }
        public string ZaloStatus { get; set; } = string.Empty;
        public string Camp { get; set; } = string.Empty;
    }

    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new();
    }

    public class TimeRestrictionResult
    {
        public bool CanSubmit { get; set; }
        public int RemainingMinutes { get; set; }
        public DateTime? LastSubmission { get; set; }
    }
    public class RegistrationRecordMongo
    {
        public string _id { get; set; }
        public string PhoneNumber { get; set; }
        public string PlateNumber { get; set; }
        public string Referee { get; set; }
        public string GPLX { get; set; }
        public string Name { get; set; }
        public int QueueNumber { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)] public DateTime? RegistrationTime { get; set; }
        [BsonIgnore]
        public string CreatedTime
        {
            get
            {
                return DateUtil.DateTimeToString(RegistrationTime);
            }
            set
            {
                RegistrationTime = DateUtil.StringToDateTime(value);
            }
        }
        public string ZaloStatus { get; set; }
        public string Camp { get; set; }
    }
}
