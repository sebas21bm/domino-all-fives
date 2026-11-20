namespace DominoAllFives.Client.WPF.Services.Wcf.Interfaces
{
    /// <summary>Submits account changes for the supplied server session.</summary>
    public interface IAccountClient
    {
        void ChangePassword(string sessionKey, string currentPassword, string newPassword);

        void DeleteAccount(string sessionKey, string password);
    }
}
