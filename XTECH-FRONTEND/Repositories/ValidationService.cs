using Microsoft.Extensions.Caching.Memory;
using System.Text.RegularExpressions;
using XTECH_FRONTEND.IRepositories;
using XTECH_FRONTEND.Model;

namespace XTECH_FRONTEND.Repositories
{
    public class ValidationService : IValidationService
    {
        private readonly IMemoryCache _cache;
        private const int RESTRICTION_MINUTES = 15;

        public ValidationService(IMemoryCache cache)
        {
            _cache = cache;
        }

        public ValidationResult ValidateCarRegistration(CarRegistrationRequest request)
        {
            var result = new ValidationResult { IsValid = true };

            // Validate plate number format (Vietnamese format: 30A-12345)
            if (string.IsNullOrWhiteSpace(request.PlateNumber))
            {
                result.IsValid = false;
                result.Errors.Add("Biển số xe không được để trống");
            }
            else if (!IsValidPlateNumber(request.PlateNumber))
            {
                result.IsValid = false;
                result.Errors.Add("Biển số xe không đúng định dạng (VD: 30A-12345)");
            }

            // Validate phone number (Vietnamese format: 10-11 digits)
            if (string.IsNullOrWhiteSpace(request.PhoneNumber))
            {
                result.IsValid = false;
                result.Errors.Add("Số điện thoại không được để trống");
            }
            else if (!IsValidPhoneNumber(request.PhoneNumber))
            {
                result.IsValid = false;
                result.Errors.Add("Số điện thoại không đúng định dạng (10-11 số)");
            }

            return result;
        }

        public TimeRestrictionResult CheckTimeRestriction(string PlateNumber)
        {
            var cacheKey = $"last_submission_{PlateNumber.Replace("-","")}";

            if (_cache.TryGetValue(cacheKey, out DateTime lastSubmission))
            {
                var timeDiff = DateTime.Now - lastSubmission;
                var remainingTime = TimeSpan.FromMinutes(RESTRICTION_MINUTES) - timeDiff;

                if (remainingTime.TotalMinutes > 0)
                {
                    return new TimeRestrictionResult
                    {
                        CanSubmit = false,
                        RemainingMinutes = (int)Math.Ceiling(remainingTime.TotalMinutes),
                        LastSubmission = lastSubmission
                    };
                }
            }

            return new TimeRestrictionResult
            {
                CanSubmit = true,
                RemainingMinutes = 0
            };
        }

        private bool IsValidPlateNumber(string plateNumber)
        {
            // Vietnamese plate format: 30A-12345 or 30A-1234
            var pattern = @"^[0-9]{2}[A-Z]{1,3}-[0-9]{4,5}$";
            return Regex.IsMatch(plateNumber.ToUpper(), pattern);
        }

        private bool IsValidPhoneNumber(string phoneNumber)
        {
            // Vietnamese phone: 10-11 digits, starting with 0
            var pattern = @"^0[0-9]{9,10}$";
            return Regex.IsMatch(phoneNumber, pattern);
        }
    }
}
