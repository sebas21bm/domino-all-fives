using System;

namespace DominoAllFives.BusinessLogic.Gameplay
{
    public sealed class MatchRuleException : Exception
    {
        public MatchIssue Issue { get; private set; }

        public MatchRuleException(MatchIssue issue) : base(issue.ToString())
        {
            Issue = issue;
        }
    }
}
