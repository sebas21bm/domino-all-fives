using DominoAllFivesService;

namespace DominoAllFives.Client.WPF.Services
{
    public interface ILoginClient
    {
        LoginResult Login(LoginCredentials credentials);
    }
}
