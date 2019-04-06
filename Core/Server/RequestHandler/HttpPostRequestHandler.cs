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
    class HttpPostRequestHandler : IRequestHandler
    {
        private static Logger _logger = new Logger(typeof(HttpPostRequestHandler).Name);

        public HttpPostRequestHandler(string root)
        {
            _root = root;
        }

        private string _root;

        public string Root { get => _root; }

        public bool CanProcess(HttpListenerContext context)
        {
            return context.Request.HttpMethod == WebRequestMethods.Http.Post;
        }

        public void Process(HttpListenerContext context)
        {
            try
            {
                _logger.Info("Process() in");
                ProcessImpl(context.Request, context.Response);
            }
            catch (Exception exception)
            {
                _logger.Error(exception.StackTrace);
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                WriteFileFromTo(new MemoryStream(Encoding.UTF8.GetBytes(exception.StackTrace)), context.Response.OutputStream, Const.DEFAULT_BUFFER_SIZE, context.Response, 0, 0);
            }
            finally
            {
                _logger.Info("Process() out");
            }
        }

        private void ProcessImpl(HttpListenerRequest request, HttpListenerResponse response)
        {
            string path = Root + request.Url.LocalPath;

            if (!Directory.Exists(Path.GetDirectoryName(path)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
            }

            using (FileStream ostream = new FileStream(path, FileMode.Create))
            {
                byte[] buffer = new byte[Const.DEFAULT_BUFFER_SIZE];
                int readSize;
                while ((readSize = request.InputStream.Read(buffer, 0, buffer.Length)) > 0)
                    ostream.Write(buffer, 0, readSize);
            }

            response.StatusCode = (int)HttpStatusCode.OK;
            response.OutputStream.Close();
        }

        private static long WriteFileFromTo(string srcFilePath, Stream destStream, int bufferSize, HttpListenerResponse response, long start, long end)
        {
            using (FileStream fileStream = new FileStream(srcFilePath, FileMode.Open, FileAccess.Read))
            {
                return WriteFileFromTo(fileStream, destStream, bufferSize, response, start, end);
            }
        }

        private static long WriteFileFromTo(Stream srcStream, Stream destStream, int bufferSize, HttpListenerResponse response, long start, long end)
        {
            byte[] buffer = new byte[bufferSize];
            long limit = end >= 0 ? end : srcStream.Length - 1;
            response.AppendHeader("Accept-Ranges", "bytes");
            response.ContentLength64 = limit - start + 1;
            response.AppendHeader("Content-Range", $"bytes {start}-{limit}/{srcStream.Length}");

            long current = start;
            srcStream.Seek(start, SeekOrigin.Begin);
            while (true)
            {
                int readSize = srcStream.Read(buffer, 0, Math.Min(bufferSize, (int)(limit - current + 1)));
                if (readSize == 0)
                    break;

                destStream.Write(buffer, 0, readSize);
                current += readSize;
            }
            return 0;
        }
    }
}
