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
    public class HttpServer_PostTest
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
        public void Test_InDirectory_Success()
        {
            using (HttpServer server = HttpServer.Create())
            {
                server.InDirectory = inDirectory;
                Assert.AreEqual(inDirectory, server.InDirectory);

                server.PortNo = 4430;
                server.Start();
                Assert.AreEqual(inDirectory, server.InDirectory);

                server.Stop();
                Assert.AreEqual(inDirectory, server.InDirectory);
            }
        }

        [TestMethod]
        public void Test_InDirectory_Fail()
        {
            using (HttpServer server = HttpServer.Create())
            {
                Assert.AreEqual(null, server.InDirectory);

                server.PortNo = 4430;
                server.Start();
                Assert.AreEqual(null, server.InDirectory);

                server.Stop();
                Assert.AreEqual(null, server.InDirectory);
            }
        }

        [TestMethod]
        public void Test_Post_Success_1File()
        {
            using (HttpServer server = HttpServer.Create())
            using (HttpClient client = new HttpClient())
            {
                server.PortNo = 4430;
                server.InDirectory = inDirectory;
                server.Start();
                var result = client.PostAsync(uri_hello, new StringContent("hello, world.")).Result;
                // assert
                ServerAssert.PostResultEqual(HttpStatusCode.OK, "hello, world.", Path.Combine(inDirectory, "hello.txt"), result);
            }
        }

        [TestMethod]
        public void Test_Post_Success_1File_JP()
        {
            using (HttpServer server = HttpServer.Create())
            using (HttpClient client = new HttpClient())
            {
                server.PortNo = 4430;
                server.InDirectory = inDirectory;
                server.Start();
                var result = client.PostAsync(uri_hello_jp, new StringContent("ハロー, ワールド")).Result;
                // assert
                ServerAssert.PostResultEqual(HttpStatusCode.OK, "ハロー, ワールド", Path.Combine(inDirectory, "ハロー.txt"), result);
            }
        }

        [TestMethod]
        public void Test_Post_Success_20File_Parallel()
        {
            using (HttpServer server = HttpServer.Create())
            using (HttpClient client = new HttpClient())
            {
                server.PortNo = 4430;
                server.InDirectory = inDirectory;
                server.Start();

                int numLoop = 20;
                var orderList = new List<int>();

                var tasks = Enumerable
                    .Range(0, numLoop)
                    .Select(i => client.PostAsync(uri_hello + i.ToString(), new StringContent("hello, world."))
                        .ContinueWith(response =>
                        {
                            orderList.Add(i);
                            return response.Result;
                        }));

                int j = numLoop;
                foreach (var task in tasks.Reverse())
                {
                    var result = task.Result;
                    // assert
                    ServerAssert.PostResultEqual(HttpStatusCode.OK, "hello, world.", Path.Combine(inDirectory, "hello.txt" + (--j).ToString()), result);
                }
                System.Diagnostics.Debug.WriteLine(string.Join("\n", orderList));

                // assert
                CollectionAssert.AreNotEqual(Enumerable.Range(0, numLoop).Reverse().ToList(), orderList);
                Assert.AreEqual(numLoop, orderList.Distinct().Count());
            }
        }

        [TestMethod]
        public void Test_Post_Success_AfterStop()
        {
            using (HttpServer server = HttpServer.Create())
            using (HttpClient client = new HttpClient())
            {
                server.PortNo = 4430;
                server.InDirectory = inDirectory;
                server.Start();

                server.Stop();
                server.Start();

                var result = client.PostAsync(uri_hello, new StringContent("hello, world.")).Result;
                // assert
                ServerAssert.PostResultEqual(HttpStatusCode.OK, "hello, world.", Path.Combine(inDirectory, "hello.txt"), result);
            }
        }
    }
}
