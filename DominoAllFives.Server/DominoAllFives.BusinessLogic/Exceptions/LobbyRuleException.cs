using System;
using DominoAllFives.Contracts.Enums;

namespace DominoAllFives.BusinessLogic.Exceptions
{
    public sealed class LobbyRuleException : Exception
    {
        public LobbyIssue Issue { get; }

        public LobbyRuleException(LobbyIssue issue)
        {
            if (!Enum.IsDefined(typeof(LobbyIssue), issue))
            {
                throw new ArgumentOutOfRangeException(nameof(issue));
            }

            Issue = issue;
        }
    }
}
