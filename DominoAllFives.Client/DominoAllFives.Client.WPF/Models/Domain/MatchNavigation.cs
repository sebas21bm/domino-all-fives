using System.Collections.Generic;
using System.Windows.Media;

namespace DominoAllFives.Client.WPF.Models
{
    public sealed class MatchNavigation
    {
        public int MatchId { get; set; }
        public int TargetPoints { get; set; }
        public PlayerSession PlayerSession { get; set; }
        public Dictionary<int, string> PlayerNames { get; set; } = new Dictionary<int, string>();
        public Dictionary<int, ImageSource> PlayerAvatars { get; set; } =
            new Dictionary<int, ImageSource>();
    }
}
