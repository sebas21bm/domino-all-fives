using System;
using DominoAllFives.Contracts.Enums;

namespace DominoAllFives.BusinessLogic.Exceptions
{
    /// <summary>Signals an expected registration rejection inside the server.</summary>
    public sealed class RegistrationRuleException : Exception
    {
        public RegistrationIssue Issue { get; }

        /// <summary>Creates a registration rejection with a defined business issue.</summary>
        /// <param name = "issue">The reason registration cannot proceed.</param>
        /// <exception cref = "ArgumentOutOfRangeException">The issue is undefined.</exception>
        public RegistrationRuleException(RegistrationIssue issue)
            : base("The account cannot be registered.")
        {
            if (!Enum.IsDefined(typeof(RegistrationIssue), issue))
            {
                throw new ArgumentOutOfRangeException(nameof(issue));
            }

            Issue = issue;
        }
    }
}
