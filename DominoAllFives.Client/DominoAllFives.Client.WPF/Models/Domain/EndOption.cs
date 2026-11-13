using DominoAllFives.Contracts.Gameplay;

namespace DominoAllFives.Client.WPF.Models
{
    public sealed class EndOption
    {
        public BoardEnd End { get; set; }

        public override string ToString()
        {
            return Localization.Resources.ResourceManager.GetString("Match_end" + End);
        }
    }
}
