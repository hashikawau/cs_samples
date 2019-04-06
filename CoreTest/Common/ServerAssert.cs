using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;

namespace FileTransfer.Core.Common
{
    class ServerAssert
    {
        internal static void GetResultEqual(HttpStatusCode expectedStatusCode, string expectedContents, HttpResponseMessage actualResponse)
        {
            Assert.AreEqual(expectedStatusCode, actualResponse.StatusCode);
            Assert.AreEqual(Encoding.UTF8.GetByteCount(expectedContents), actualResponse.Content.Headers.ContentLength);
            Assert.AreEqual(expectedContents, actualResponse.Content.ReadAsStringAsync().Result);
        }

        internal static void PostResultEqual(HttpStatusCode expectedStatusCode, string expectedContents, string filePath, HttpResponseMessage actualResponse)
        {
            Assert.AreEqual(expectedStatusCode, actualResponse.StatusCode);
            Assert.AreEqual(expectedContents, File.ReadAllText(filePath));
        }
    }
}
