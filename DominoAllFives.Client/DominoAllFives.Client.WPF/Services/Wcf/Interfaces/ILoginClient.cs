using DominoAllFivesService;

namespace DominoAllFives.Client.WPF.Services.Wcf.Interfaces
{
    public interface ILoginClient
    {
        PlayerAccount LoginWithAccount(LoginCredentials credentials);
    }
}
