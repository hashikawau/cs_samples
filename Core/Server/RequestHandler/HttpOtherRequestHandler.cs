using FileTransfer.Core.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace FileTransfer.Core.Server.RequestHandler
{
    class HttpOtherRequestHandler : IRequestHandler
    {
        private static Logger _logger = new Logger(typeof(HttpOtherRequestHandler).Name);

        public HttpOtherRequestHandler()
        {
        }

        public bool CanProcess(HttpListenerContext context)
        {
            return true;
        }

        public void Process(HttpListenerContext context)
        {
            try
            {
                _logger.Info("Process() in");
                ProcessImpl(context);
            }
            finally
            {
                _logger.Info("Process() out");
            }
        }

        private void ProcessImpl(HttpListenerContext context)
        {
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;

            response.StatusCode = (int)HttpStatusCode.Forbidden;
            byte[] message = Encoding.UTF8.GetBytes("Forbidden");
            response.ContentLength64 = message.Length;
            response.OutputStream.Write(message, 0, message.Length);
        }
    }
}
