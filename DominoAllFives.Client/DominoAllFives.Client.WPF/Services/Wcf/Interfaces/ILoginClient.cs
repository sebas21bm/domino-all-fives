using DominoAllFivesService;

namespace DominoAllFives.Client.WPF.Services.Wcf.Interfaces
{
    public interface ILoginClient
    {
        PlayerAccount LoginWithAccount(Credentials credentials);
    }
}
