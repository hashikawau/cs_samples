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
    public class HttpServer_GetTest
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
        public void Test_OutDirectory_Success()
        {
            using (HttpServer server = HttpServer.Create())
            {
                server.OutDirectory = outDirectory;
                Assert.AreEqual(outDirectory, server.OutDirectory);

                server.PortNo = 4430;
                server.Start();
                Assert.AreEqual(outDirectory, server.OutDirectory);

                server.Stop();
                Assert.AreEqual(outDirectory, server.OutDirectory);
            }
        }

        [TestMethod]
        public void Test_OutDirectory_Fail()
        {
            using (HttpServer server = HttpServer.Create())
            {
                Assert.AreEqual(null, server.OutDirectory);

                server.PortNo = 4430;
                server.Start();
                Assert.AreEqual(null, server.OutDirectory);

                server.Stop();
                Assert.AreEqual(null, server.OutDirectory);
            }
        }

        [TestMethod]
        public void Test_Get_Success_1Directory()
        {
            using (HttpServer server = HttpServer.Create())
            using (HttpClient client = new HttpClient())
            {
                server.PortNo = 4430;
                server.OutDirectory = outDirectory;
                server.Start();
                var result = client.GetAsync(uri_root).Result;
                // assert
                ServerAssert.GetResultEqual(HttpStatusCode.OK, "hello.txt\nハロー.txt", result);
            }
        }

        [TestMethod]
        public void Test_Get_Success_1File()
        {
            using (HttpServer server = HttpServer.Create())
            using (HttpClient client = new HttpClient())
            {
                server.PortNo = 4430;
                server.OutDirectory = outDirectory;
                server.Start();
                var result = client.GetAsync(uri_hello).Result;
                // assert
                ServerAssert.GetResultEqual(HttpStatusCode.OK, "hello, world.", result);
            }
        }

        [TestMethod]
        public void Test_Get_Success_1File_JP()
        {
            using (HttpServer server = HttpServer.Create())
            using (HttpClient client = new HttpClient())
            {
                server.PortNo = 4430;
                server.OutDirectory = outDirectory;
                server.Start();
                var result = client.GetAsync(uri_hello_jp).Result;
                // assert
                ServerAssert.GetResultEqual(HttpStatusCode.OK, "ハロー, ワールド", result);
            }
        }

        [TestMethod]
        public void Test_Get_Success_1File_Range_BeginEnd()
        {
            using (HttpServer server = HttpServer.Create())
            using (HttpClient client = new HttpClient())
            {
                server.PortNo = 4430;
                server.OutDirectory = outDirectory;
                server.Start();
                client.DefaultRequestHeaders.Add("Range", "bytes=3-10");
                var result = client.GetAsync(uri_hello).Result;
                // assert
                ServerAssert.GetResultEqual(HttpStatusCode.PartialContent, "lo, worl", result);
            }
        }

        [TestMethod]
        public void Test_Get_Success_1File_Range_Begin()
        {
            using (HttpServer server = HttpServer.Create())
            using (HttpClient client = new HttpClient())
            {
                server.PortNo = 4430;
                server.OutDirectory = outDirectory;
                server.Start();
                client.DefaultRequestHeaders.Add("Range", "bytes=3-");
                var result = client.GetAsync(uri_hello).Result;
                // assert
                ServerAssert.GetResultEqual(HttpStatusCode.PartialContent, "lo, world.", result);
            }
        }

        [TestMethod]
        public void Test_Get_Success_20File_Parallel()
        {
            using (HttpServer server = HttpServer.Create())
            using (HttpClient client = new HttpClient())
            {
                server.PortNo = 4430;
                server.OutDirectory = outDirectory;
                server.Start();

                int numLoop = 20;
                var orderList = new List<int>();

                var tasks = Enumerable
                    .Range(0, numLoop)
                    .Select(i => client.GetAsync(uri_hello)
                        .ContinueWith(response =>
                        {
                            orderList.Add(i);
                            return response.Result;
                        }));

                foreach (var task in tasks.Reverse())
                {
                    var result = task.Result;
                    // assert
                    ServerAssert.GetResultEqual(HttpStatusCode.OK, "hello, world.", result);
                }
                //System.Diagnostics.Debug.WriteLine(string.Join("\n", orderList));

                // assert
                CollectionAssert.AreNotEqual(Enumerable.Range(0, numLoop).Reverse().ToList(), orderList);
                Assert.AreEqual(numLoop, orderList.Distinct().Count());
            }
        }

        [TestMethod]
        public void Test_Get_Success_NotFound()
        {
            using (HttpServer server = HttpServer.Create())
            using (HttpClient client = new HttpClient())
            {
                server.PortNo = 4430;
                server.OutDirectory = outDirectory;
                server.Start();
                var result = client.GetAsync(uri_not_found).Result;
                // assert
                ServerAssert.GetResultEqual(HttpStatusCode.NotFound, "Not Found", result);
            }
        }

        [TestMethod]
        public void Test_Get_Success_AfterStop()
        {
            using (HttpServer server = HttpServer.Create())
            using (HttpClient client = new HttpClient())
            {
                server.PortNo = 4430;
                server.OutDirectory = outDirectory;
                server.Start();

                server.Stop();
                server.Start();

                var result = client.GetAsync(uri_hello).Result;
                // assert
                ServerAssert.GetResultEqual(HttpStatusCode.OK, "hello, world.", result);
            }
        }
    }
}
