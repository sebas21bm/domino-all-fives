using System;
using DominoAllFives.Contracts.Enums;

namespace DominoAllFives.BusinessLogic.Exceptions
{
    /// <summary>
    /// Signals an expected account rule rejection inside the server.
    /// </summary>
    public sealed class AccountRuleException : Exception
    {
        public AccountIssue Issue { get; }

        public AccountRuleException(AccountIssue issue) : base("The account cannot sign in.")
        {
            if (!Enum.IsDefined(typeof(AccountIssue), issue))
            {
                throw new ArgumentOutOfRangeException(nameof(issue));
            }
            Issue = issue;
        }
    }
}
