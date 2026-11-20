using System;
using DominoAllFives.Contracts.Enums;

namespace DominoAllFives.BusinessLogic.Exceptions
{
    /// <summary>Signals an expected player profile rule rejection inside the server.</summary>
    public sealed class ProfileRuleException : Exception
    {
        public ProfileIssue Issue { get; }

        /// <summary>Creates a rejection with a defined profile issue.</summary>
        /// <param name = "issue">The profile rule that rejected the operation.</param>
        /// <exception cref = "ArgumentOutOfRangeException">The issue is undefined.</exception>
        public ProfileRuleException(ProfileIssue issue)
            : base("The player profile operation was rejected.")
        {
            if (!Enum.IsDefined(typeof(ProfileIssue), issue))
            {
                throw new ArgumentOutOfRangeException(nameof(issue));
            }

            Issue = issue;
        }
    }
}
