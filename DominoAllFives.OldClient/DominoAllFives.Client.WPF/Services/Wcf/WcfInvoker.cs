using System;
using System.ServiceModel;

namespace DominoAllFives.Client.WPF.Services.Wcf
{
    /// <summary>Releases each generated proxy after the call, aborting a faulted connection.</summary>
    internal static class WcfInvoker
    {
        public static TResult Invoke<TClient, TResult>(
            Func<TClient> createClient, Func<TClient, TResult> operation)
            where TClient : ICommunicationObject
        {
            TClient client = createClient();
            try
            {
                TResult result = operation(client);
                client.Close();
                return result;
            }
            catch
            {
                client.Abort();
                throw;
            }
        }
    }
}
