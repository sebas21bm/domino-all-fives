using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using MatchService;

namespace DominoAllFives.Client.WPF.Models
{
    public static class PlayScoreFormatter
    {
        private const string SumSeparator = " + ";
        private const string EndSeparator = ", ";
        public static string Format(PlayScore score, string playerName)
        {
            if (score == null)
            {
                return GetText("Match_lblPlayPending");
            }

            List<string> lines = new List<string>
            {
                string.Format(CultureInfo.CurrentCulture, GetText("Match_lblPlayHeader"),
                    playerName, score.Tile.FirstValue, score.Tile.SecondValue, score.RoundNumber),
                string.Format(CultureInfo.CurrentCulture, GetText("Match_lblPlaySum"),
                    string.Join(SumSeparator, score.EndValues), score.Total, score.BasePoints)
            };
            AddEndDetails(lines, score);
            AddPowerDetails(lines, score);
            lines.Add(string.Format(CultureInfo.CurrentCulture, GetText("Match_lblPlayPoints"),
                score.Points));
            if (score.RoundPoints > 0)
            {
                lines.Add(string.Format(CultureInfo.CurrentCulture, GetText(
                    "Match_lblPlayRoundBonus"), score.RoundPoints));
            }

            return string.Join(System.Environment.NewLine, lines);
        }

        private static void AddEndDetails(List<string> lines, PlayScore score)
        {
            if (score.IncludesOpeningDouble)
            {
                lines.Add(GetText("Match_lblPlayOpeningDouble"));
            }

            if (score.BlockedEnds.Count > 0)
            {
                string blocked = string.Join(EndSeparator, score.BlockedEnds.Select(end
                    => GetText("Match_end" + end)));
                lines.Add(string.Format(CultureInfo.CurrentCulture, GetText(
                    "Match_lblPlayBlocked"), blocked));
            }

            if (score.BasePoints == 0)
            {
                lines.Add(GetText("Match_lblPlayNoPoints"));
            }
        }

        private static void AddPowerDetails(List<string> lines, PlayScore score)
        {
            if (score.Tile.SpecialPower == SpecialPower.DoublePoints)
            {
                lines.Add(string.Format(CultureInfo.CurrentCulture, GetText("Match_lblPlayRed"),
                    score.BasePoints, score.Multiplier, score.Points));
            }

            if (score.Tile.SpecialPower == SpecialPower.Block)
            {
                lines.Add(string.Format(CultureInfo.CurrentCulture, GetText(
                    "Match_lblPlayPurple"), GetText("Match_end" + score.PlayedEnd)));
            }
        }

        private static string GetText(string key)
        {
            return Localization.Resources.ResourceManager.GetString(key)
                ?? throw new InvalidOperationException("Missing localization resource: " + key);
        }
    }
}
