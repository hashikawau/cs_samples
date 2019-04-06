using CSSamples.Common.Logger;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Networking.Sockets;

namespace CSSamples.Bluetooth.Server
{
    class BluetoothAdapter
    {
        private static Logger _logger = Logger.GetLogger(typeof(BluetoothAdapter));

        //private static Func<RfcommServiceId, IRfcommServiceProviderWrapper> _factory
        //    = (serviceId) => new IRfcommServiceProviderWrapperImpl(RfcommServiceProvider.CreateAsync(serviceId).AsTask().Result);
        internal static IWrapperFactories _factories = new WrapperImplFactories();

        //private RfcommServiceProvider _provider;
        private IRfcommServiceProviderWrapper _provider;

        public event EventHandler<BluetoothClientConnectedEventArgs> BluetoothClientConnected;

        public BluetoothAdapter()
        {
            _provider = null;
            //var _serialPort = serialPort;
        }

        public void StartListening(RfcommServiceId serviceId)
        {
            _logger.Info("StartListen() in: serviceId={0}", serviceId.AsString());
            // Initialize the provider for the hosted RFCOMM service
            //_provider = RfcommServiceProvider.CreateAsync(serviceId).AsTask().Result;
            _provider = _factories.ProviderFactory.Invoke(RfcommServiceId.SerialPort);

            // Create a listener for this service and start listening
            IStreamSocketListenerWrapper listener = _factories.ListenerFactory.Invoke();
            //StreamSocketListener listener = new StreamSocketListener();
            listener.ConnectionReceived += OnConnectionReceived;
            listener.BindServiceNameAsync(serviceId.AsString(), SocketProtectionLevel.BluetoothEncryptionAllowNullAuthentication).AsTask().Wait();

            // Set the SDP attributes and start advertising
            //InitializeServiceSdpAttributes(_provider);
            _provider.StartAdvertising(listener);

            _logger.Info("StartListen() out");
        }

        //const uint SERVICE_VERSION_ATTRIBUTE_ID = 0x0300;
        //const byte SERVICE_VERSION_ATTRIBUTE_TYPE = 0x0A;   // UINT32
        //const uint SERVICE_VERSION = 200;
        //void InitializeServiceSdpAttributes(RfcommServiceProvider provider)
        //{
        //    var writer = new Windows.Storage.Streams.DataWriter();

        //    // First write the attribute type
        //    writer.WriteByte(SERVICE_VERSION_ATTRIBUTE_TYPE);
        //    // Then write the data
        //    writer.WriteUInt32(SERVICE_VERSION);

        //    var data = writer.DetachBuffer();
        //    provider.SdpRawAttributes.Add(SERVICE_VERSION_ATTRIBUTE_ID, data);
        //}

        internal void StopListening()
        {
            // Stop advertising/listening so that we're only serving one client
            if (_provider != null)
                _provider.StopAdvertising();
        }

        async void OnConnectionReceived(StreamSocketListener listener, StreamSocketListenerConnectionReceivedEventArgs args)
        {
            _logger.Info("OnConnectionReceived in");
            await Task.Run(() =>
            {
                try
                {
                    BluetoothClientConnected?.Invoke(this, new BluetoothClientConnectedEventArgs(
                        args.Socket.InputStream.AsStreamForRead(),
                        args.Socket.OutputStream.AsStreamForWrite()));
                }
                catch (Exception e)
                {
                    _logger.Error(e);
                }
            });
            _logger.Info("OnConnectionReceived out");
        }

        //internal async void Send()
        //{
        //    _logger.Info("Send() in");
        //    // 保存されたBluetoothデバイス名と一致するデバイス情報を取得しデータを送信する
        //    var serviceInfos = await DeviceInformation.FindAllAsync(RfcommDeviceService.GetDeviceSelector(RfcommServiceId.SerialPort));
        //    foreach (var info in serviceInfos)
        //    {
        //        _logger.Info("info={0}", info.Name);
        //        await Connect(info);
        //        break;
        //    }
        //    _logger.Info("Send() out");
        //}

        //private async Task Connect(DeviceInformation serviceInfo)
        //{
        //    try
        //    {
        //        // 指定されたデバイス情報で接続を行う
        //        //if (DeviceService == null)
        //        //{
        //        var DeviceService = await RfcommDeviceService.FromIdAsync(serviceInfo.Id);
        //        var BtSocket = new StreamSocket();
        //        await BtSocket.ConnectAsync(
        //          DeviceService.ConnectionHostName,
        //          DeviceService.ConnectionServiceName,
        //          SocketProtectionLevel.BluetoothEncryptionAllowNullAuthentication);
        //        var Writer = new DataWriter(BtSocket.OutputStream);
        //        var Message = "Connected " + DeviceService.ConnectionHostName.DisplayName;
        //        //}
        //        // 接続されたBluetoothデバイスにデータを送信する
        //        //SetPower(this.Power);

        //        byte[] byteArray = Encoding.UTF8.GetBytes("hello, home-pc");
        //        while (true)
        //        {
        //            Writer.WriteBytes(byteArray);
        //            var sendResult = await Writer.StoreAsync();
        //            Thread.Sleep(1000);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.Error(ex);
        //    }
        //}
    }

    class BluetoothClientConnectedEventArgs
    {
        public Stream InputStream { get; private set; }
        public Stream OutputStream { get; private set; }

        internal BluetoothClientConnectedEventArgs(Stream inputStream, Stream outputStream)
        {
            InputStream = inputStream;
            OutputStream = outputStream;
        }
    }
}
