using CSSamples.Bluetooth.Server;
using CSSamples.Common.Logger;
using System;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Foundation;
using Windows.Networking.Sockets;

namespace CSSamples.BluetoothTest.Server
{
    class WrapperStubFactories : IWrapperFactories
    {
        public RfcommServiceProviderWrapperStub ProviderStub = new RfcommServiceProviderWrapperStub();
        public StreamSocketListenerWrapperStub ListenerStub = new StreamSocketListenerWrapperStub();

        public Func<RfcommServiceId, IRfcommServiceProviderWrapper> ProviderFactory
            => (serviceId) => ProviderStub;

        public Func<IStreamSocketListenerWrapper> ListenerFactory
            => () => ListenerStub;
    }

    class RfcommServiceProviderWrapperStub : IRfcommServiceProviderWrapper
    {
        public void StartAdvertising(IStreamSocketListenerWrapper listener) { }

        public void StopAdvertising() { }
    }

    class StreamSocketListenerWrapperStub : IStreamSocketListenerWrapper
    {
        public StreamSocketListenerWrapperStub() { }

        public void DispatchConnectionReceivedEvent()
        {
            ConnectionReceived?.Invoke(null, null);
        }

        public StreamSocketListener Value { get; } = null;

        public event TypedEventHandler<StreamSocketListener, StreamSocketListenerConnectionReceivedEventArgs> ConnectionReceived;

        public IAsyncAction BindServiceNameAsync(string localServiceName, SocketProtectionLevel protectionLevel)
            => new AsyncAction();

        class AsyncAction : IAsyncAction
        {
            private static Logger _logger = Logger.GetLogger(typeof(AsyncAction));

            public void GetResults()
            {
                throw new NotImplementedException();
            }

            private AsyncActionCompletedHandler _handler;
            public AsyncActionCompletedHandler Completed
            {
                get => throw new NotImplementedException();
                set => _handler = value;
            }

            public void Cancel()
            {
                throw new NotImplementedException();
            }

            public void Close()
            {
                throw new NotImplementedException();
            }

            public Exception ErrorCode => throw new NotImplementedException();

            public uint Id => throw new NotImplementedException();

            public AsyncStatus Status => AsyncStatus.Completed;
        }
    }
}