using CSSamples.BluetoothTest.Server;
using CSSamples.Common.Logger;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Windows.Devices.Bluetooth.Rfcomm;

namespace CSSamples.Bluetooth.Server
{
    [TestClass]
    public class BluetoothAdapterTest
    {
        private static FileAppender _appender = new FileAppender("C:/tmp/logs/bluetooth_adapter_test.log");
        private static WrapperStubFactories _factories = new WrapperStubFactories();

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            Logger.Appenders += _appender.Append;
            BluetoothAdapter._factories = _factories;
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            Logger.Appenders -= _appender.Append;
        }

        [TestMethod]
        public void Test_StartListening_Success()
        {
            // arrange
            var target = new BluetoothAdapter();
            // act
            target.StartListening(RfcommServiceId.SerialPort);
            _factories.ListenerStub.DispatchConnectionReceivedEvent();
            //target._provider.Listen();
            // assert
            //Assert.AreEqual(false, target.Running);
        }

    }
}