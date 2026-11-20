using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Media;

namespace DominoAllFives.Client.WPF.Models
{
    public sealed class MatchPlayerDirectory
    {
        public IReadOnlyDictionary<int, string> Names { get; }
        public IReadOnlyDictionary<int, ImageSource> Avatars { get; }

        public MatchPlayerDirectory(
            IDictionary<int, string> names,
            IDictionary<int, ImageSource> avatars)
        {
            Dictionary<int, string> playerNames = new Dictionary<int, string>(names);
            Dictionary<int, ImageSource> playerAvatars = new Dictionary<int, ImageSource>(avatars);
            Names = new ReadOnlyDictionary<int, string>(playerNames);
            Avatars = new ReadOnlyDictionary<int, ImageSource>(playerAvatars);
        }
    }
}
