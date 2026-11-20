using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace DominoAllFives.Client.WPF.Models
{
    public sealed class MatchNavigation
    {
        public int MatchId { get; }
        public int TargetPoints { get; }
        public PlayerSession PlayerSession { get; }
        public IReadOnlyDictionary<int, string> PlayerNames { get; }
        public IReadOnlyDictionary<int, ImageSource> PlayerAvatars { get; }

        public MatchNavigation(
            int matchId,
            int targetPoints,
            PlayerSession playerSession,
            MatchPlayerDirectory players)
        {
            if (players == null)
            {
                throw new ArgumentNullException(nameof(players));
            }

            MatchId = matchId;
            TargetPoints = targetPoints;
            PlayerSession = playerSession ?? throw new ArgumentNullException(nameof(playerSession));
            PlayerNames = players.Names;
            PlayerAvatars = players.Avatars;
        }
    }
}
