using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mtp;
using System.Diagnostics;
using System.Linq;

namespace MtpFileTransfer.Mtp.Tests {
    [TestClass()]
    public class DeviceManagerTests {
        [TestMethod()]
        public void DevicesTest_1() {
            var deviceManager = new DeviceManager();
            var result = deviceManager.Devices;
            Assert.AreEqual(1, result.Count());
            Debug.WriteLine("info={0}", result[0]);
        }
    }
}