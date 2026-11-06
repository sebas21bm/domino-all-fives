using System;
using System.ServiceModel;

namespace DominoAllFives.Client.WPF.Services.Wcf
{
    /// <summary>
    /// Releases each generated proxy after the call, aborting a faulted connection.
    /// </summary>
    internal static class WcfInvoker
    {
        public static TResult Invoke<TProxy, TResult>(
            Func<TProxy> createProxy, Func<TProxy, TResult> operation)
            where TProxy : ICommunicationObject
        {
            TProxy proxy = createProxy();
            try
            {
                TResult result = operation(proxy);
                proxy.Close();
                return result;
            }
            catch
            {
                proxy.Abort();
                throw;
            }
        }
    }
}
