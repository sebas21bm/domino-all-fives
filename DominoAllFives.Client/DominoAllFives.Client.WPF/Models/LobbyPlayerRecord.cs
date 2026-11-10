using System;
using System.Windows.Media.Imaging;

namespace DominoAllFives.Client.WPF.Models
{
    public sealed class LobbyPlayerRecord
    {
        public int PlayerId { get; }
        public string Username { get; }
        public BitmapImage AvatarImage { get; }
        public bool IsHost { get; }
        public bool IsCurrentPlayer { get; }

        public LobbyPlayerRecord(int playerId, string username, BitmapImage avatarImage,
            bool isHost, bool isCurrentPlayer)
        {
            if (playerId <= 0) throw new ArgumentOutOfRangeException(nameof(playerId));
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException(null, nameof(username));
            PlayerId = playerId;
            Username = username;
            AvatarImage = avatarImage ?? throw new ArgumentNullException(nameof(avatarImage));
            IsHost = isHost;
            IsCurrentPlayer = isCurrentPlayer;
        }
    }
}
