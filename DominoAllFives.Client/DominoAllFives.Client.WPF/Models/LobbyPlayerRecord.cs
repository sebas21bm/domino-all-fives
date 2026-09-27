using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DominoAllFives.Client.WPF.Models
{
    public class LobbyPlayerRecord
    {
        public string Username { get; set; } = string.Empty;
        public string AvatarPath { get; set; } = string.Empty;
        public bool IsHost { get; set; }
        public bool IsCurrentPlayer { get; set; }
    }
}
