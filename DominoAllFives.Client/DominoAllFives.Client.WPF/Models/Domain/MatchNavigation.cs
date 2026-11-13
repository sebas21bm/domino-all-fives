using System.Collections.Generic;

namespace DominoAllFives.Client.WPF.Models
{
    public sealed class MatchNavigation
    {
        public int MatchId { get; set; }
        public int TargetPoints { get; set; }
        public PlayerSession PlayerSession { get; set; }
        public Dictionary<int, string> PlayerNames { get; set; } = new Dictionary<int, string>();
    }
}
