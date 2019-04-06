using CSSamples.Common.Logger;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Devices.Enumeration;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace CSSamples.Bluetooth.Server
{
    class Adapter
    {
        private static Logger _logger = Logger.GetLogger(typeof(Adapter));

        private RfcommServiceProvider _provider;
        //private IRfcommServiceProviderWrapper _provider;

        internal Adapter()
        {
            _provider = RfcommServiceProvider.CreateAsync(RfcommServiceId.SerialPort).AsTask().Result;
            //_provider = RfcommServiceProviderWrapperFactory.GetProvider(RfcommServiceId.SerialPort);
        }

        internal async void Initialize()
        {
            _logger.Info("Initialize() in");
            // Initialize the provider for the hosted RFCOMM service
            //_provider = await RfcommServiceProvider.CreateAsync(RfcommServiceId.ObexObjectPush);

            _logger.Info("service uuid={0}", _provider.ServiceId.AsString());

            // Create a listener for this service and start listening
            StreamSocketListener listener = new StreamSocketListener();
            listener.ConnectionReceived += OnConnectionReceived;
            await listener.BindServiceNameAsync(
                _provider.ServiceId.AsString(),
                SocketProtectionLevel.BluetoothEncryptionAllowNullAuthentication);

            // Set the SDP attributes and start advertising
            //InitializeServiceSdpAttributes(_provider);
            _provider.StartAdvertising(listener);

            _logger.Info("Initialize() out");
        }

        internal void Stop()
        {
            // Stop advertising/listening so that we're only serving one client
            _provider.StopAdvertising();
        }

        internal async void Send()
        {
            _logger.Info("Send() in");
            // 保存されたBluetoothデバイス名と一致するデバイス情報を取得しデータを送信する
            var serviceInfos = await DeviceInformation.FindAllAsync(RfcommDeviceService.GetDeviceSelector(RfcommServiceId.SerialPort));
            foreach (var info in serviceInfos)
            {
                _logger.Info("info={0}", info.Name);
                await Connect(info);
                break;
            }
            _logger.Info("Send() out");
        }

        private async Task Connect(DeviceInformation serviceInfo)
        {
            try
            {
                // 指定されたデバイス情報で接続を行う
                //if (DeviceService == null)
                //{
                var DeviceService = await RfcommDeviceService.FromIdAsync(serviceInfo.Id);
                var BtSocket = new StreamSocket();
                await BtSocket.ConnectAsync(
                  DeviceService.ConnectionHostName,
                  DeviceService.ConnectionServiceName,
                  SocketProtectionLevel.BluetoothEncryptionAllowNullAuthentication);
                var Writer = new DataWriter(BtSocket.OutputStream);
                var Message = "Connected " + DeviceService.ConnectionHostName.DisplayName;
                //}
                // 接続されたBluetoothデバイスにデータを送信する
                //SetPower(this.Power);

                byte[] byteArray = Encoding.UTF8.GetBytes("hello, home-pc");
                while (true)
                {
                    Writer.WriteBytes(byteArray);
                    var sendResult = await Writer.StoreAsync();
                    Thread.Sleep(1000);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }
        }

        const uint SERVICE_VERSION_ATTRIBUTE_ID = 0x0300;
        const byte SERVICE_VERSION_ATTRIBUTE_TYPE = 0x0A;   // UINT32
        const uint SERVICE_VERSION = 200;
        void InitializeServiceSdpAttributes(RfcommServiceProvider provider)
        {
            var writer = new Windows.Storage.Streams.DataWriter();

            // First write the attribute type
            writer.WriteByte(SERVICE_VERSION_ATTRIBUTE_TYPE);
            // Then write the data
            writer.WriteUInt32(SERVICE_VERSION);

            var data = writer.DetachBuffer();
            provider.SdpRawAttributes.Add(SERVICE_VERSION_ATTRIBUTE_ID, data);
        }

        async void OnConnectionReceived(
            StreamSocketListener listener,
            StreamSocketListenerConnectionReceivedEventArgs args)
        {
            //await listener.Close();
            //_socket = args.Socket;

            // The client socket is connected. At this point the App can wait for
            // the user to take some action, e.g. click a button to receive a file
            // from the device, which could invoke the Picker and then save the
            // received file to the picked location. The transfer itself would use
            // the Sockets API and not the Rfcomm API, and so is omitted here for
            // brevity.

            try
            {

                Stream istream = args.Socket.InputStream.AsStreamForRead();
                byte[] buffer = new byte[1024];
                int readCount;
                _logger.Info("receive start");
                while ((readCount = await istream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    var eotIndex = -1;
                    for (int i = 0; i < readCount; ++i)
                    {
                        if (buffer[i] == 0x04)
                        {
                            eotIndex = i;
                            break;
                        }
                    }

                    int size = eotIndex >= 0 ? eotIndex : readCount;
                    //_logger.Debug("readCount={0}, eotIndex={1}, size={2}", readCount, eotIndex, size);

                    _logger.Info("read: {0}", Encoding.UTF8.GetString(buffer, 0, readCount));
                    //for (int i = 0; i < readCount; ++i)
                    //_logger.Info("read: {0:X2}, {1}", buffer[i], Encoding.UTF8.GetString(buffer, i, 1));

                    if (eotIndex >= 0)
                        break;
                    //_logger.Debug("read={0}, {1}", readCount, Encoding.UTF8.GetString(buffer, 0, readCount));
                }
                _logger.Info("receive end");
                //istream.Close();
                Stream ostream = args.Socket.OutputStream.AsStreamForWrite();
                byte[] message = Encoding.UTF8.GetBytes("hello, hello\r\n world\r\n\r\n");
                _logger.Info("send start");
                ostream.Write(message, 0, message.Length);
                ostream.Write(new byte[] { 0x04 }, 0, 1);
                ostream.Flush();
                _logger.Info("send end");
                ostream.Close();
                _logger.Info("closed");
            }
            catch (Exception e)
            {
                _logger.Error(e);
            }
        }
    }
}
