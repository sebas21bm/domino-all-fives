using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using DominoAllFives.BusinessLogic.Storage;
using DominoAllFives.Contracts.Domain;
using Player = DominoAllFives.Contracts.Domain.Player;
using DominoAllFives.Contracts.Enums;
using DominoAllFives.DataAccess.Models;
using StoredPlayer = DominoAllFives.DataAccess.Models.Player;
using DominoAllFives.DataAccess.Repositories;
using Microsoft.Extensions.Logging.Abstractions;

namespace DominoAllFives.BusinessLogic.Managers
{
    public static class SessionManager
    {
        private const int SessionKeySizeInBytes = 32;
        private static readonly object sessionLock = new object();
        private static readonly Dictionary<string, int> sessions =
            new Dictionary<string, int>(
            StringComparer.Ordinal);
        private static readonly ProfilePictureStorage pictureStorage =
            new ProfilePictureStorage(
            NullLogger<ProfilePictureStorage>.Instance);
        public static string CreateSession(int playerId)
        {
            if (playerId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(playerId));
            }

            lock (sessionLock)
            {
                RemovePlayerSessionInternal(playerId);
                byte[] bytes = new byte[SessionKeySizeInBytes];
                using (RandomNumberGenerator generator = RandomNumberGenerator.Create())
                {
                    generator.GetBytes(bytes);
                }

                string key = Convert.ToBase64String(bytes);
                sessions.Add(key, playerId);
                return key;
            }
        }

        public static int GetPlayerId(string sessionKey)
        {
            if (string.IsNullOrWhiteSpace(sessionKey))
            {
                throw new ArgumentException(null, nameof(sessionKey));
            }

            lock (sessionLock)
            {
                int playerId;
                if (!sessions.TryGetValue(sessionKey, out playerId))
                {
                    throw new KeyNotFoundException();
                }

                return playerId;
            }
        }

        public static Player GetPlayer(string sessionKey)
        {
            int playerId = GetPlayerId(sessionKey);
            using (DominoAllFivesEntities context = new DominoAllFivesEntities())
            {
                StoredPlayer stored = new PlayerRepository(context).GetById(playerId);
                if (stored == null || stored.IsDeleted)
                {
                    RemoveSession(sessionKey);
                    throw new KeyNotFoundException();
                }

                return new Player
                {
                    PlayerId = stored.IdPlayer,
                    Username = stored.Username,
                    Picture = ProfilePicture.FromStoredContent(
                        pictureStorage.GetProfilePictureData(
                        stored.ProfilePicture)),
                    Presence = PlayerPresence.Online,
                    IsGuest = stored.IsGuest
                };
            }
        }

        public static bool RemoveSession(string sessionKey)
        {
            if (string.IsNullOrWhiteSpace(sessionKey))
            {
                throw new ArgumentException(null, nameof(sessionKey));
            }

            lock (sessionLock)
            {
                return sessions.Remove(sessionKey);
            }
        }

        public static bool RemovePlayerSession(int playerId)
        {
            if (playerId <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(playerId));
            }

            lock (sessionLock)
            {
                return RemovePlayerSessionInternal(playerId);
            }
        }

        private static bool RemovePlayerSessionInternal(int playerId)
        {
            string key = sessions.FirstOrDefault(entry => entry.Value == playerId).Key;
            return !string.IsNullOrEmpty(key) && sessions.Remove(key);
        }
    }
}
