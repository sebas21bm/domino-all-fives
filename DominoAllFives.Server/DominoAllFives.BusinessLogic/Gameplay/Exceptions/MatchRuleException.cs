using System;

namespace DominoAllFives.BusinessLogic.Gameplay
{
    public sealed class MatchRuleException : Exception
    {
        public MatchIssue Issue { get; }

        public MatchRuleException(MatchIssue issue) : base(issue.ToString())
        {
            if (!Enum.IsDefined(typeof(MatchIssue), issue))
            {
                throw new ArgumentOutOfRangeException(nameof(issue));
            }

            Issue = issue;
        }
    }
}
