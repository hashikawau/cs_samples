using System;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Foundation;
using Windows.Networking.Sockets;

namespace CSSamples.Bluetooth.Server
{
    class WrapperImplFactories : IWrapperFactories
    {
        public WrapperImplFactories() { }

        public Func<RfcommServiceId, IRfcommServiceProviderWrapper> ProviderFactory
            => (serviceId) => new RfcommServiceProviderWrapperImpl(RfcommServiceProvider.CreateAsync(serviceId)
                .AsTask().Result);

        public Func<IStreamSocketListenerWrapper> ListenerFactory
            => () => new StreamSocketListenerWrapperImpl();
    }

    class RfcommServiceProviderWrapperImpl : IRfcommServiceProviderWrapper
    {
        private RfcommServiceProvider _provider;

        public RfcommServiceProviderWrapperImpl(RfcommServiceProvider provider)
        {
            _provider = provider;
        }

        public void StartAdvertising(IStreamSocketListenerWrapper listener)
            => _provider.StartAdvertising(listener.Value);

        public void StopAdvertising()
            => _provider.StopAdvertising();

    }

    class StreamSocketListenerWrapperImpl : IStreamSocketListenerWrapper
    {
        public StreamSocketListenerWrapperImpl() { }

        public StreamSocketListener Value { get; }
            = new StreamSocketListener();

        public event TypedEventHandler<StreamSocketListener, StreamSocketListenerConnectionReceivedEventArgs> ConnectionReceived
        {
            add => Value.ConnectionReceived += value;
            remove => Value.ConnectionReceived -= value;
        }

        public IAsyncAction BindServiceNameAsync(string localServiceName, SocketProtectionLevel protectionLevel)
            => Value.BindServiceNameAsync(localServiceName, protectionLevel);
    }
}
