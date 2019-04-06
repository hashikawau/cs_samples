using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;

namespace FileTransfer.Core.Util
{
    [TestClass]
    public class StartUpTest
    {
        [TestMethod]
        public void Test_GetAddresses()
        {
            //Debug.WriteLine(string.Join(", ", StartUp.GetIpAdresses()));
            Assert.IsTrue(StartUp.GetIpAdresses().Length > 0);
        }

        [TestMethod]
        public void Test_GetSslCertificateHashBoundToPort_Success()
        {
            Assert.AreEqual("F386FA31CAA05B57C7606FD1FBC33E271D053C2B", StartUp.GetSslCertificateHashBoundToPort(4430));
        }

        [TestMethod]
        public void Test_GetSslCertificateHashBoundToPort_Fail()
        {
            //Assert.IsTrue(StartUp.IsSslCertBound(443));
            Assert.AreEqual(null, StartUp.GetSslCertificateHashBoundToPort(8080));
        }

        [TestMethod]
        public void Test_FindSslCertificateByThumbprint_Success()
        {
            Assert.AreEqual("CN=localhostCA", StartUp.FindSslCertificateByThumbprint("F386FA31CAA05B57C7606FD1FBC33E271D053C2B").Issuer);
        }

        [TestMethod]
        public void Test_FindSslCertificateByThumbprint_Fail()
        {
            Assert.AreEqual(null, StartUp.FindSslCertificateByThumbprint("0"));
        }

        //[TestMethod]
        public void Test_BindSslCertificateToPort_Success()
        {
            Assert.AreEqual("F386FA31CAA05B57C7606FD1FBC33E271D053C2B", StartUp.BindSslCertificateToPort(8080, "localhost"));
        }
    }
}
