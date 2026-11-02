using System.Windows.Media.Imaging;

namespace DominoAllFives.Client.WPF.Models
{
    /// <summary>
    /// Represents a player displayed in the rankings interface.
    /// </summary>
    public class PlayerRankingRecord
    {
        public int Rank { get; set; }

        public string Username { get; set; }

        public int StatisticValue { get; set; }

        public BitmapImage Avatar { get; set; }
    }
}