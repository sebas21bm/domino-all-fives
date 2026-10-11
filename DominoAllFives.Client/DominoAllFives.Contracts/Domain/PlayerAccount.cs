using DominoAllFives.Contracts.Enums;
using DominoAllFives.Contracts.Exceptions;
using System;

namespace DominoAllFives.Contracts.Domain
{
    public class PlayerAccount
    {
        public int PlayerId { get; private set; }

        public string Username { get; private set; }

        public string EmailOrUsername { get; private set; }

        public string Password { get; private set; }

        public bool IsAuthenticated { get; private set; }

        public bool IsDeleted { get; private set; }

        public bool IsBanned { get; private set; }

        public DateTime? SuspensionUntil { get; private set; }

        private PlayerAccount()
        {
        }

        public static PlayerAccount ForLogin(
            string emailOrUsername,
            string password)
        {
            if (string.IsNullOrWhiteSpace(emailOrUsername) ||
                string.IsNullOrWhiteSpace(password))
            {
                throw new AuthenticationException(
                    LoginFailureReason.InvalidCredentials);
            }

            return new PlayerAccount
            {
                EmailOrUsername = emailOrUsername.Trim(),
                Password = password
            };
        }

        public static PlayerAccount FromStoredAccount(
            int playerId,
            string username,
            bool isDeleted,
            bool isBanned,
            DateTime? suspensionUntil)
        {
            return new PlayerAccount
            {
                PlayerId = playerId,
                Username = username,
                IsDeleted = isDeleted,
                IsBanned = isBanned,
                SuspensionUntil = suspensionUntil
            };
        }

        public void EnsureCanLogin(DateTime currentTime)
        {
            if (IsDeleted)
            {
                throw new AuthenticationException(
                    LoginFailureReason.InvalidCredentials);
            }

            if (IsBanned)
            {
                throw new AuthenticationException(
                    LoginFailureReason.Banned);
            }

            if (SuspensionUntil.HasValue &&
                SuspensionUntil.Value > currentTime)
            {
                throw new AuthenticationException(
                    LoginFailureReason.Suspended);
            }
        }

        public void MarkAsAuthenticated()
        {
            EnsureCanLogin(DateTime.Now);
            IsAuthenticated = true;
        }
    }
}
