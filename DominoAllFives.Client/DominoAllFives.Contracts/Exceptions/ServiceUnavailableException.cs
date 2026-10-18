using System;

namespace DominoAllFives.Contracts.Exceptions
{
    public class ServiceUnavailableException : Exception
    {
        public ServiceUnavailableException()
            : base("The service is temporarily unavailable.")
        {
        }
    }
}
