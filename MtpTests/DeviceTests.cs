using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mtp;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace MtpFileTransfer.Mtp.Tests {
    [TestClass()]
    public class DeviceTests {
        Device Device0 {
            get {
                var deviceManager = new DeviceManager();
                return deviceManager.Devices[0];
            }
        }

        [TestMethod()]
        public void NameTest_1() {
            Assert.AreEqual("P024", Device0.Name);
        }

        [TestMethod()]
        //[ExpectedException()]
        public void LsTest_1_1_fail_none_directory() {
            var result = Device0.Ls("/Internal storage/none/none");
            Debug.WriteLine("count={0}", result.Count());
            foreach (var e in result)
                Debug.WriteLine("  {0}", (object)e);
        }

        [TestMethod()]
        public void LsTest_2() {
            var result = Device0.Ls("/");
            Debug.WriteLine("count={0}", result.Count());
            foreach (var e in result)
                Debug.WriteLine("  {0}", (object)e);
        }

        [TestMethod()]
        public void LsTest_3() {
            var result = Device0.Ls("/Internal storage");
            Debug.WriteLine("count={0}", result.Count());
            foreach (var e in result)
                Debug.WriteLine("  {0}", (object)e);
        }

        [TestMethod()]
        public void LsTest_4() {
            var result = Device0.Ls("/Internal storage/Download");
            Debug.WriteLine("count={0}", result.Count());
            foreach (var e in result)
                Debug.WriteLine("  {0}", (object)e);
        }

        [TestMethod()]
        public void LsTest_5() {
            var result = Device0.Ls("/Internal storage/Download/J00-10.pdf");
            Debug.WriteLine("count={0}", result.Count());
            foreach (var e in result)
                Debug.WriteLine("  {0}", (object)e);
        }

        [TestMethod()]
        public void CopyFromTest_1() {
            Device0.CopyFrom(
                "/Internal storage/Download/test.sh",
                "C:/Users/hashikawa/Downloads",
                "test_2.sh"
            );
            var result = Directory.GetFiles("C:/Users/hashikawa/Downloads");
            Debug.WriteLine("count={0}", result.Count());
            foreach (var e in result)
                Debug.WriteLine("  {0}", (object)e);
            Assert.IsNotNull(result.FirstOrDefault(s => Path.GetFileName(s) == "test_2.sh"));
        }

        [TestMethod()]
        public void CopyFromTest_2() {
            Device0.CopyFrom(
                "/Internal storage/Download/IT Engineer's 30 Common Verbs.zip",
                "C:/Users/hashikawa/Downloads"
            );
            var result = Directory.GetFiles("C:/Users/hashikawa/Downloads");
            Debug.WriteLine("count={0}", result.Count());
            foreach (var e in result)
                Debug.WriteLine("  {0}", (object)e);
            Assert.IsNotNull(result.FirstOrDefault(s => Path.GetFileName(s) == "test_2.sh"));
        }

        [TestMethod()]
        public void CopyFromTest_3() {
            Device0.CopyFrom(
                "/Internal storage/Download/test.sh",
                "C:/Users/hashikawa/Downloads/tmp"
            );
            var result = Directory.GetFiles("C:/Users/hashikawa/Downloads/tmp");
            Debug.WriteLine("count={0}", result.Count());
            foreach (var e in result)
                Debug.WriteLine("  {0}", (object)e);
            Assert.IsNotNull(result.FirstOrDefault(s => Path.GetFileName(s) == "test.sh"));
        }

        [TestMethod()]
        public void CopyFromTest_4_1_fail_none_source() {
            Device0.CopyFrom(
                "/Internal storage/Download/none",
                "C:/Users/hashikawa/Downloads"
            );
            var result = Directory.GetFiles("C:/Users/hashikawa/Downloads");
            Debug.WriteLine("count={0}", result.Count());
            foreach (var e in result)
                Debug.WriteLine("  {0}", (object)e);
            Assert.IsNotNull(result.FirstOrDefault(s => Path.GetFileName(s) == "test_2.sh"));
        }

        [TestMethod()]
        public void CopyFromTest_4_2_fail_none_dest() {
            Device0.CopyFrom(
                "/Internal storage/Download/none",
                "C:/???"
            );
            var result = Directory.GetFiles("C:/Users/hashikawa/Downloads");
            Debug.WriteLine("count={0}", result.Count());
            foreach (var e in result)
                Debug.WriteLine("  {0}", (object)e);
            Assert.IsNotNull(result.FirstOrDefault(s => Path.GetFileName(s) == "test_2.sh"));
        }

        [TestMethod()]
        public void CopyToTest_1() {
            Device0.CopyTo(
                "C:/Users/hashikawa/Downloads/test.sh",
                "/Internal storage/Download",
                "test_2.sh"
            );
            var result = Device0.Ls("/Internal storage/Download");
            Debug.WriteLine("count={0}", result.Count());
            foreach (var e in result)
                Debug.WriteLine("  {0}", (object)e);
        }

        [TestMethod()]
        public void CopyToTest_2() {
            Device0.CopyTo(
                "C:/Users/hashikawa/Downloads/WpdSampleProject.zip",
                "/Internal storage/Download"
            );
            var result = Device0.Ls("/Internal storage/Download");
            Debug.WriteLine("count={0}", result.Count());
            foreach (var e in result)
                Debug.WriteLine("  {0}", (object)e);
        }

        [TestMethod()]
        public void CopyToTest_3() {
            Device0.CopyTo(
                "C:/Users/hashikawa/Downloads/test.sh",
                "/Internal storage/Download/tmp"
            );
            var result = Device0.Ls("/Internal storage/Download");
            Debug.WriteLine("count={0}", result.Count());
            foreach (var e in result)
                Debug.WriteLine("  {0}", (object)e);
        }

        [TestMethod()]
        public void RmTest_1() {
            Device0.Rm(
                "/Internal storage/none/none"
            );
        }

        [TestMethod()]
        public void RmTest_2() {
            Device0.Rm(
                "/Internal storage/Download/test.sh"
            );
            var result = Device0.Ls("/Internal storage/Download");
            Debug.WriteLine("count={0}", result.Count());
            foreach (var e in result)
                Debug.WriteLine("  {0}", (object)e);
        }

        [TestMethod()]
        public void RmTest_3() {
            Device0.Rm(
                "/Internal storage/Download/none"
            );
            var result = Device0.Ls("/Internal storage/Download");
            Debug.WriteLine("count={0}", result.Count());
            foreach (var e in result)
                Debug.WriteLine("  {0}", (object)e);
        }

        [TestMethod()]
        public void MkdirsTest_1() {
            Device0.Mkdirs(
                "/Internal storage/Download/none"
            );
            var result = Device0.Ls("/Internal storage/Download");
            Debug.WriteLine("count={0}", result.Count());
            foreach (var e in result)
                Debug.WriteLine("  {0}", (object)e);
            Assert.AreEqual("none", result.FirstOrDefault(s => s == "none"));
        }

    }
}