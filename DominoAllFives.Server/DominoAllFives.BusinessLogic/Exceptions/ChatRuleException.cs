using System;
using DominoAllFives.Contracts.Enums;

namespace DominoAllFives.BusinessLogic.Exceptions
{
    /// <summary>
    /// Represents a chat domain rule violation identified by a specific <see cref = "ChatIssue"/>.
    /// </summary>
    public sealed class ChatRuleException : Exception
    {
        public ChatIssue Issue { get; }

        public ChatRuleException(ChatIssue issue)
        {
            if (!Enum.IsDefined(typeof(ChatIssue), issue))
            {
                throw new ArgumentOutOfRangeException(nameof(issue));
            }

            Issue = issue;
        }
    }
}
