using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using DominoAllFives.BusinessLogic.Storage;
using DominoAllFives.Contracts.Domain;
using DominoAllFives.Contracts.Enums;
using DominoAllFives.DataAccess.Models;
using DominoAllFives.DataAccess.Repositories;
using Microsoft.Extensions.Logging.Abstractions;
using StoredPlayer = DominoAllFives.DataAccess.Models.Player;
using Player = DominoAllFives.Contracts.Domain.Player;

namespace DominoAllFives.BusinessLogic.Managers
{
    public static class SessionManager
    {
        private static readonly object SessionLock = new object();
        private static readonly Dictionary<string, int> Sessions =
            new Dictionary<string, int>(StringComparer.Ordinal);
        private static readonly ProfilePictureStorage PictureStorage =
            new ProfilePictureStorage(NullLogger<ProfilePictureStorage>.Instance);

        public static string CreateSession(int playerId)
        {
            if (playerId <= 0) throw new ArgumentOutOfRangeException(nameof(playerId));
            lock (SessionLock)
            {
                RemovePlayerSessionInternal(playerId);
                byte[] bytes = new byte[32];
                using (RandomNumberGenerator generator = RandomNumberGenerator.Create())
                    generator.GetBytes(bytes);
                string key = Convert.ToBase64String(bytes);
                Sessions.Add(key, playerId);
                return key;
            }
        }

        public static int GetPlayerId(string sessionKey)
        {
            if (string.IsNullOrWhiteSpace(sessionKey))
                throw new ArgumentException(null, nameof(sessionKey));
            lock (SessionLock)
            {
                int playerId;
                if (!Sessions.TryGetValue(sessionKey, out playerId))
                    throw new KeyNotFoundException();
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
                        PictureStorage.GetProfilePictureData(stored.ProfilePicture)),
                    Presence = PlayerPresence.Online,
                    IsGuest = stored.IsGuest
                };
            }
        }

        public static bool RemoveSession(string sessionKey)
        {
            if (string.IsNullOrWhiteSpace(sessionKey))
                throw new ArgumentException(null, nameof(sessionKey));
            lock (SessionLock) return Sessions.Remove(sessionKey);
        }

        public static bool RemovePlayerSession(int playerId)
        {
            if (playerId <= 0) throw new ArgumentOutOfRangeException(nameof(playerId));
            lock (SessionLock) return RemovePlayerSessionInternal(playerId);
        }

        private static bool RemovePlayerSessionInternal(int playerId)
        {
            string key = Sessions.FirstOrDefault(entry => entry.Value == playerId).Key;
            return !string.IsNullOrEmpty(key) && Sessions.Remove(key);
        }
    }
}
