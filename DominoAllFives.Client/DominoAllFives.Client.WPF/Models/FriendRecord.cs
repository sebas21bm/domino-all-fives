using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DominoAllFives.Client.WPF.Models
{
    public class FriendRecord
    {
        public string Username { get; set; }
        public string AvatarPath { get; set; }
        public bool IsInvited { get; set; }
        public bool CanInvite => !IsInvited;
    }
}
