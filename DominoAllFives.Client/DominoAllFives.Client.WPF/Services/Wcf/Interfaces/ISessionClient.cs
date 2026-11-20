using DominoAllFivesService;

namespace DominoAllFives.Client.WPF.Services.Wcf.Interfaces
{
    public interface ISessionClient
    {
        PlayerAccount LoginWithAccount(Credentials credentials);
    }
}
