using FileTransfer.Core.Common;
using FileTransfer.Core.Properties;
using FileTransfer.Core.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;

namespace FileTransfer.Core.Server
{
    [TestClass]
    public class HttpServer_StartTest
    {
        private static string outDirectory = "./root/out/";
        private static string inDirectory = "./root/in/";
        private static Uri uri_root = new Uri("https://localhost:4430/");
        private static Uri uri_hello = new Uri("https://localhost:4430/hello.txt");
        private static Uri uri_hello_jp = new Uri("https://localhost:4430/ハロー.txt");
        private static Uri uri_not_found = new Uri("https://localhost:4430/not_found.txt");

        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
            if (StartUp.GetSslCertificateHashBoundToPort(4430) == null)
                StartUp.BindSslCertificateToPort(4430, "localhost");
            ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback((sender, certification, chain, errors) => true);
        }

        [TestInitialize]
        public void TestInitialize()
        {
            Directory.CreateDirectory(outDirectory);
            Directory.CreateDirectory(inDirectory);
            File.WriteAllText(Path.Combine(outDirectory, "hello.txt"), TestResource.hello);
            File.WriteAllText(Path.Combine(outDirectory, "ハロー.txt"), TestResource.hello_jp);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            if (Directory.Exists(outDirectory))
                Directory.Delete(outDirectory, true);
            if (Directory.Exists(inDirectory))
                Directory.Delete(inDirectory, true);
        }

        [TestMethod]
        [ExpectedException(typeof(PortNotBoundSslCertException))]
        public void Test_Start_Fail()
        {
            using (HttpServer server = HttpServer.Create())
            {
                server.PortNo = 8080;
                server.OutDirectory = outDirectory;
                server.Start();
            }
        }

        [TestMethod]
        public void Test_Start_Fail_Forbidden()
        {
            using (HttpServer server = HttpServer.Create())
            using (HttpClient client = new HttpClient())
            {
                server.PortNo = 4430;
                server.OutDirectory = outDirectory;
                server.Start();
                var result = client.DeleteAsync(uri_hello).Result;
                // assert
                ServerAssert.GetResultEqual(HttpStatusCode.Forbidden, "Forbidden", result);
            }
        }

        [TestMethod]
        [HierarchicalExpectedException(typeof(AggregateException), typeof(HttpRequestException))]
        public void Test_Stop_Success()
        {
            using (HttpServer server = HttpServer.Create())
            {
                server.PortNo = 4430;
                server.OutDirectory = outDirectory;
                server.Start();
            }

            using (HttpClient client = new HttpClient())
            {
                var result = client.GetAsync(uri_hello).Result;
            }
        }
    }
}
