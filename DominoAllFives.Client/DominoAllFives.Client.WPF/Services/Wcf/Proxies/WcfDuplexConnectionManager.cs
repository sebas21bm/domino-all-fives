using System;
using System.ServiceModel;

namespace DominoAllFives.Client.WPF.Services.Wcf.Proxies
{
    /// <summary>Serializes access to one persistent proxy; callbacks do not acquire this gate.</summary>
    internal sealed class WcfDuplexConnectionManager<TProxy> : IDisposable where TProxy : class, ICommunicationObject
    {
        private readonly object _sync = new object();
        private readonly Func<TProxy> _createProxy;
        private TProxy _proxy;
        private bool _disposed;

        public WcfDuplexConnectionManager(Func<TProxy> createProxy)
        {
            _createProxy = createProxy ?? throw new ArgumentNullException(nameof(createProxy));
        }

        public CommunicationState State { get { lock (_sync) return _proxy == null ? CommunicationState.Closed : _proxy.State; } }

        public void Open()
        {
            lock (_sync)
            {
                ThrowIfDisposed();
                if (_proxy != null)
                {
                    if (_proxy.State == CommunicationState.Opened) return;
                    throw new InvalidOperationException("The duplex proxy is not available.");
                }
                _proxy = _createProxy();
                if (_proxy == null) throw new InvalidOperationException("The proxy factory returned null.");
                try { _proxy.Open(); }
                catch { Abort(); throw; }
            }
        }

        public TResult Invoke<TResult>(Func<TProxy, TResult> operation)
        {
            if (operation == null) throw new ArgumentNullException(nameof(operation));
            lock (_sync)
            {
                ThrowIfDisposed();
                if (_proxy == null || _proxy.State != CommunicationState.Opened)
                    throw new InvalidOperationException("The duplex proxy is not open.");
                try { return operation(_proxy); }
                catch (FaultException) { throw; }
                catch (CommunicationException) { Abort(); throw; }
                catch (TimeoutException) { Abort(); throw; }
            }
        }

        public void Invoke(Action<TProxy> operation)
        {
            if (operation == null) throw new ArgumentNullException(nameof(operation));
            Invoke(proxy => { operation(proxy); return true; });
        }

        public void Close()
        {
            lock (_sync)
            {
                TProxy proxy = _proxy;
                _proxy = null;
                if (proxy == null) return;
                try
                {
                    if (proxy.State == CommunicationState.Faulted) proxy.Abort();
                    else proxy.Close();
                }
                catch (CommunicationException) { proxy.Abort(); }
                catch (TimeoutException) { proxy.Abort(); }
                catch { proxy.Abort(); throw; }
            }
        }

        private void Abort()
        {
            TProxy proxy = _proxy;
            _proxy = null;
            proxy?.Abort();
        }

        public void Dispose()
        {
            lock (_sync)
            {
                if (_disposed) return;
                _disposed = true;
                Close();
            }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(WcfDuplexConnectionManager<TProxy>));
        }
    }
}
