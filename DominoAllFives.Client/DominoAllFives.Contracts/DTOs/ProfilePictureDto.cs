using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DominoAllFives.Contracts.DTOs
{
    /// <summary>
    /// Represents the profile picture sent by a player
    /// </summary>
    public class ProfilePictureDto
    {
        public int PlayerId { get; set; }

        public string FileName { get; set; }

        public byte[] ImageData { get; set; }
    }
}
