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
        /// <summary>
        /// Executes an operation without a result and releases its proxy.
        /// </summary>
        public static void Invoke<TProxy>(
            Func<TProxy> createProxy,
            Action<TProxy> operation)
            where TProxy : ICommunicationObject
        {
            TProxy proxy = createProxy();
            bool completed = false;

            try
            {
                operation(proxy);

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
                            ex,"The WCF channel could not be closed cleanly.");
                        proxy.Abort();
                    }
                    catch (TimeoutException ex)
                    {
                        LoggerFactoryProvider.CreateLogger<TProxy>().LogWarning(
                            ex,"The WCF channel close operation timed out.");
                        proxy.Abort();
                    }
                }
                completed = true;
            }
            finally
            {
                if (!completed)
                {
                    proxy.Abort();
                }
            }
        }

        /// <summary>
        /// Executes an operation with a result using the same proxy lifecycle.
        /// </summary>
        public static TResult Invoke<TProxy, TResult>(
            Func<TProxy> createProxy,
            Func<TProxy, TResult> operation)
            where TProxy : ICommunicationObject
        {
            TResult result = default;

            void ExecuteOperation(TProxy proxy)
            {
                result = operation(proxy);
            }

            Invoke<TProxy>(createProxy, ExecuteOperation);
            return result;
        }
    }
}