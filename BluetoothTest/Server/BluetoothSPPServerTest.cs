using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CSSamples.Bluetooth.Server
{
    [TestClass]
    public class BluetoothSPPServerTest
    {
        [TestMethod]
        public void Test_Start_Success_NotRunning()
        {
            // arrange
            var target = new BluetoothSPPServer();
            // act
            target.Start();
            // assert
            Assert.AreEqual(true, target.Running);
        }

        [TestMethod]
        public void Test_Start_Success_AlreadyRunning()
        {
            // arrange
            var target = new BluetoothSPPServer();
            target.Start();
            // act
            target.Start();
            // assert
            Assert.AreEqual(true, target.Running);
        }

        [TestMethod]
        public void Test_Stop_Success_NotRunning()
        {
            // arrange
            var target = new BluetoothSPPServer();
            // act
            target.Stop();
            // assert
            Assert.AreEqual(false, target.Running);
        }

        [TestMethod]
        public void Test_Stop_Success_AlreadyRunning()
        {
            // arrange
            var target = new BluetoothSPPServer();
            // act
            target.Start();
            target.Stop();
            // assert
            Assert.AreEqual(false, target.Running);
        }

    }
}
