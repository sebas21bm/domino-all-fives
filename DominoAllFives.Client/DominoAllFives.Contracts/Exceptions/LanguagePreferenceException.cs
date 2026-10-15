using System;
using DominoAllFives.Contracts.Enums;

namespace DominoAllFives.Contracts.Exceptions
{
    public class LanguagePreferenceException : Exception
    {
        public LanguagePreferenceFailureReason Reason { get; }

        public LanguagePreferenceException(LanguagePreferenceFailureReason reason)
            : base("Unable to change account language.")
        {
            Reason = reason;
        }
    }
}
