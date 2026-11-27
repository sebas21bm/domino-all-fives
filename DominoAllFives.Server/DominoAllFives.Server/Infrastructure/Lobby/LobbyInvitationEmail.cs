using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;

namespace DominoAllFives.Server.Infrastructure.Lobby
{
    internal static class LobbyInvitationEmail
    {
        private const string GmailHost = "smtp.gmail.com";
        private const int StartTlsPort = 587;
        private const int SendTimeoutMilliseconds = 15000;
        private const string LocalCredentialFile = "gmail-sender.dpapi";

        public static void Send(MailAddress recipient, string lobbyCode)
        {
            NetworkCredential credentials = LoadCredentials();

            using (MailMessage message = new MailMessage())
            using (SmtpClient smtp = new SmtpClient(GmailHost, StartTlsPort))
            {
                message.From = new MailAddress(credentials.UserName, "Domino All Fives");
                message.To.Add(recipient);
                message.Subject = "Domino All Fives";
                message.SubjectEncoding = Encoding.UTF8;
                message.BodyEncoding = Encoding.UTF8;
                message.Body = lobbyCode;

                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtp.EnableSsl = true;
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = credentials;
                smtp.Timeout = SendTimeoutMilliseconds;
                smtp.Send(message);
            }
        }

        private static NetworkCredential LoadCredentials()
        {
            string sender = Environment.GetEnvironmentVariable("DOMINO_GMAIL_ADDRESS");
            string password = Environment.GetEnvironmentVariable("DOMINO_GMAIL_APP_PASSWORD");
            if (string.IsNullOrWhiteSpace(sender) && string.IsNullOrWhiteSpace(password))
            {
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, LocalCredentialFile);
                if (File.Exists(path))
                {
                    string[] localCredentials = ReadLocalCredentials(path);
                    if (localCredentials.Length == 2)
                    {
                        sender = localCredentials[0];
                        password = localCredentials[1];
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(sender) || string.IsNullOrWhiteSpace(password))
            {
                throw new InvalidOperationException("The Gmail sender has not been configured.");
            }

            return new NetworkCredential(sender.Trim(), password.Replace(" ", string.Empty));
        }

        private static string[] ReadLocalCredentials(string path)
        {
            try
            {
                byte[] protectedCredentials = File.ReadAllBytes(path);
                byte[] credentials = ProtectedData.Unprotect(
                    protectedCredentials, null, DataProtectionScope.CurrentUser);
                try
                {
                    return Encoding.UTF8.GetString(credentials).Split('\n');
                }
                finally
                {
                    Array.Clear(credentials, 0, credentials.Length);
                }
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException
                || ex is CryptographicException)
            {
                throw new InvalidOperationException("The local Gmail credentials could not be read.", ex);
            }
        }
    }
}
