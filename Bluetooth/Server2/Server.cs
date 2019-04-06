using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Networking.Sockets;

namespace ConsoleApp1
{
    class Server
    {
        // https://docs.microsoft.com/en-us/windows/uwp/devices-sensors/send-or-receive-files-with-rfcomm#receive-file-as-a-server
        public void Start()
        {
            var serviceId = RfcommServiceId.SerialPort;
            RfcommServiceProvider provider = RfcommServiceProvider.CreateAsync(serviceId).AsTask().Result;
            StreamSocketListener listener = new StreamSocketListener();
            listener.ConnectionReceived += OnConnectionReceived;

            Console.WriteLine("StartListen() in: serviceId={0}", serviceId.AsString());
            listener.BindServiceNameAsync(serviceId.AsString(), SocketProtectionLevel.BluetoothEncryptionAllowNullAuthentication).AsTask().Wait();

            //InitializeServiceSdpAttributes(provider);
            provider.StartAdvertising(listener);

            Console.WriteLine("StartListen() out");

        }

        const uint SERVICE_VERSION_ATTRIBUTE_ID = 0x0300;
        const byte SERVICE_VERSION_ATTRIBUTE_TYPE = 0x0A;   // UINT32
        const uint SERVICE_VERSION = 200;
        static void InitializeServiceSdpAttributes(RfcommServiceProvider provider)
        {
            var writer = new Windows.Storage.Streams.DataWriter();

            // First write the attribute type
            writer.WriteByte(SERVICE_VERSION_ATTRIBUTE_TYPE);
            // Then write the data
            writer.WriteUInt32(SERVICE_VERSION);

            var data = writer.DetachBuffer();
            provider.SdpRawAttributes.Add(SERVICE_VERSION_ATTRIBUTE_ID, data);
        }

        static void OnConnectionReceived(StreamSocketListener listener, StreamSocketListenerConnectionReceivedEventArgs args)
        {
            Console.WriteLine("OnConnectionReceived in");
            Task.Run(() =>
            {
                StreamSocket socket = args.Socket;
                Stream istream = socket.InputStream.AsStreamForRead();
                byte[] buffer = new byte[1024];
                int readCount;
                Console.WriteLine("receive start");
                while ((readCount = istream.Read(buffer, 0, buffer.Length)) > 0)
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

                    Console.WriteLine("read: {0}", Encoding.UTF8.GetString(buffer, 0, readCount));
                    //for (int i = 0; i < readCount; ++i)
                    //_logger.Info("read: {0:X2}, {1}", buffer[i], Encoding.UTF8.GetString(buffer, i, 1));

                    if (eotIndex >= 0)
                        break;
                    //_logger.Debug("read={0}, {1}", readCount, Encoding.UTF8.GetString(buffer, 0, readCount));
                }
                Console.WriteLine("receive end");
                //istream.Close();

                //Stream ostream = args.Socket.OutputStream.AsStreamForWrite();
                //byte[] message = Encoding.UTF8.GetBytes("hello, hello\r\n world\r\n\r\n");
                //_logger.Info("send start");
                //ostream.Write(message, 0, message.Length);
                //ostream.Write(new byte[] { 0x04 }, 0, 1);
                //ostream.Flush();
                //_logger.Info("send end");
                //ostream.Close();
                //_logger.Info("closed");
            });
            Console.WriteLine("OnConnectionReceived out");
        }
    }
}
