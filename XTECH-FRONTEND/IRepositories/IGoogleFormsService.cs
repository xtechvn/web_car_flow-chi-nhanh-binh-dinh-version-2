using XTECH_FRONTEND.Model;

namespace XTECH_FRONTEND.IRepositories
{

    public interface IGoogleFormsService
    {
        Task<bool> SubmitToGoogleFormAsync(RegistrationRecord record);
    }
}
