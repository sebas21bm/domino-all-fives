using System;
using System.ServiceModel;
using Microsoft.Extensions.Logging;

namespace DominoAllFives.Client.WPF.Services.Wcf
{
    /// <summary>
    /// Releases each generated proxy after the call, aborting a faulted connection.
    /// </summary>
    internal static class WcfInvoker
    {
        public static TResult Invoke<TProxy, TResult>(
            Func<TProxy> createProxy,
            Func<TProxy, TResult> operation)
            where TProxy : ICommunicationObject
        {
            TProxy proxy = createProxy();
            bool completed = false;
            try
            {
                TResult result = operation(proxy);
                if (proxy.State == CommunicationState.Faulted)
                {
                    proxy.Abort();
                }
                else
                {
                    try
                    {
                        proxy.Close();
                    }
                    catch (CommunicationException ex)
                    {
                        LoggerFactoryProvider.CreateLogger<TProxy>().LogWarning(
                            ex, "The WCF channel could not be closed cleanly.");
                        proxy.Abort();
                    }
                    catch (TimeoutException ex)
                    {
                        LoggerFactoryProvider.CreateLogger<TProxy>().LogWarning(
                            ex, "The WCF channel close operation timed out.");
                        proxy.Abort();
                    }
                }

                completed = true;
                return result;
            }
            finally
            {
                if (!completed)
                {
                    proxy.Abort();
                }
            }
        }
    }
}
