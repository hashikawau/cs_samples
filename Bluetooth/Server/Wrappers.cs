using System;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Foundation;
using Windows.Networking.Sockets;

namespace CSSamples.Bluetooth.Server
{
    interface IWrapperFactories
    {
        Func<RfcommServiceId, IRfcommServiceProviderWrapper> ProviderFactory { get; }
        Func<IStreamSocketListenerWrapper> ListenerFactory { get; }
    }

    interface IRfcommServiceProviderWrapper
    {
        void StartAdvertising(IStreamSocketListenerWrapper listener);
        //void StartAdvertising(StreamSocketListener listener);
        void StopAdvertising();
        //static IAsyncOperation<RfcommServiceProvider> CreateAsync(RfcommServiceId serviceId);
        //IDictionary<uint, IBuffer> SdpRawAttributes { get; }
        //RfcommServiceId ServiceId { get; }
    }

    interface IStreamSocketListenerWrapper
    {
        StreamSocketListener Value { get; }

        IAsyncAction BindServiceNameAsync(string localServiceName, SocketProtectionLevel protectionLevel);
        event TypedEventHandler<StreamSocketListener, StreamSocketListenerConnectionReceivedEventArgs> ConnectionReceived;
    }

}
