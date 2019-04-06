using CSSamples.Common.Logger;
using System;
using System.IO;
using System.Text;
using Windows.Devices.Bluetooth.Rfcomm;

namespace CSSamples.Bluetooth.Server
{
    public class BluetoothSPPServer
    {
        private static Logger _logger = Logger.GetLogger(typeof(BluetoothSPPServer));

        private BluetoothAdapter _adapter = new BluetoothAdapter();

        public bool Running { get; private set; } = false;

        public BluetoothSPPServer()
        {
            _adapter.BluetoothClientConnected += (sender, args) =>
            {
                Stream istream = args.InputStream;
                byte[] buffer = new byte[1024];
                int readCount;
                _logger.Info("receive start");
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

                    _logger.Info("read: {0}", Encoding.UTF8.GetString(buffer, 0, readCount));
                    //for (int i = 0; i < readCount; ++i)
                    //_logger.Info("read: {0:X2}, {1}", buffer[i], Encoding.UTF8.GetString(buffer, i, 1));

                    if (eotIndex >= 0)
                        break;
                    //_logger.Debug("read={0}, {1}", readCount, Encoding.UTF8.GetString(buffer, 0, readCount));
                }
                _logger.Info("receive end");
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
            };
        }

        public void Start()
        {
            try
            {
                _logger.Info("Start() in");
                if (!Running)
                    _adapter.StartListening(RfcommServiceId.SerialPort);
                Running = true;
            }
            catch (Exception e)
            {
                _adapter.StopListening();
                Running = false;
            }
            finally
            {
                _logger.Info("Start() out");
            }
        }

        public void Stop()
        {
            if (Running)
                _adapter.StopListening();
            Running = false;
        }
    }
}
