using IP.Project.Models.Identity;

namespace IP.Project.Contracts.Interfaces
{
    public interface IAuthService
    {
        Task<(int, string)> Registeration(RegistrationModel model, string role);
        Task<(int, string)> Login(LoginModel model);

        Task<(int, string)> Logout();
    }
}