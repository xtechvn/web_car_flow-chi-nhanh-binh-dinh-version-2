using XTECH_FRONTEND.Model;

namespace XTECH_FRONTEND.IRepositories
{
    public interface IValidationService
    {
        ValidationResult ValidateCarRegistration(CarRegistrationRequest request);
        TimeRestrictionResult CheckTimeRestriction(string phoneNumber);
    }
}
