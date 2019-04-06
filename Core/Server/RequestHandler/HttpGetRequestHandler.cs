using FileTransfer.Core.Common;
using FileTransfer.Core.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace FileTransfer.Core.Server.RequestHandler
{
    class HttpGetRequestHandler : IRequestHandler
    {
        private static Logger _logger = new Logger(typeof(HttpGetRequestHandler));

        public HttpGetRequestHandler(string root)
        {
            _root = root;
        }

        private string _root;

        public string Root { get => _root; }

        public bool CanProcess(HttpListenerContext context)
        {
            return context.Request.HttpMethod == WebRequestMethods.Http.Get;
        }

        public void Process(HttpListenerContext context)
        {
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;
            try
            {
                _logger.Info("Process() in: request.path={0}, headers={1}", request.RawUrl, request.Headers);
                ProcessImpl(GetAbsoluteResourcePath(request), request, response);
            }
            catch (Exception exception)
            {
                _logger.Error(exception);
            }
            finally
            {
                response.Close();
                _logger.Info("Process() out");
            }
        }

        private string GetAbsoluteResourcePath(HttpListenerRequest request)
        {
            return Root + request.Url.LocalPath;
        }

        private static void ProcessImpl(string resourcePath, HttpListenerRequest request, HttpListenerResponse response)
        {
            if (File.Exists(resourcePath))
                ReturnResource(resourcePath, request, response);
            else if (Directory.Exists(resourcePath))
                ReturnResourceList(resourcePath, request, response);
            else
                ReturnNoResource(request, response);
        }

        private static void ReturnResource(string resourcePath, HttpListenerRequest request, HttpListenerResponse response)
        {
            var rangeStatus = GetRangeStatus(request);
            WriteFileFromTo(
                rangeStatus.Item1,
                rangeStatus.Item2,
                rangeStatus.Item3,
                GetContentType(resourcePath),
                resourcePath,
                response.OutputStream,
                Const.DEFAULT_BUFFER_SIZE,
                response
                );
        }

        private static Tuple<HttpStatusCode, long, long> GetRangeStatus(HttpListenerRequest request)
        {
            HttpStatusCode status = HttpStatusCode.OK;
            long start = 0;
            long end = -1;

            string[] range = request.Headers.GetValues("Range");
            if (range != null)
            {
                var matches = Regex.Matches(range[0], @"bytes=([0-9]*)-([0-9]*)");
                foreach (Match match in matches)
                {
                    if (!string.IsNullOrEmpty(match.Groups[1].Value))
                    {
                        start = long.Parse(match.Groups[1].Value);
                        status = HttpStatusCode.PartialContent;
                    }
                    if (!string.IsNullOrEmpty(match.Groups[2].Value))
                    {
                        end = long.Parse(match.Groups[2].Value);
                        status = HttpStatusCode.PartialContent;
                    }
                    break;
                }
            }
            return Tuple.Create(status, start, end);
        }

        private static void ReturnResourceList(string resourcePath, HttpListenerRequest request, HttpListenerResponse response)
        {
            IEnumerable<string> fileLinks = Directory
                .GetFileSystemEntries(resourcePath)
                .Select(p => Path.GetFileName(p))
                .Select(fileName => string.Format("<a href=\"{0}\">{1}</a><br>", Path.Combine(request.Url.LocalPath, fileName), fileName));
            string html = string.Format(Resource.template, string.Join("\n", fileLinks));
            WriteFileFromTo(
                HttpStatusCode.OK,
                0,
                -1,
                "text/html",
                new MemoryStream(Encoding.UTF8.GetBytes(html)),
                response.OutputStream,
                Const.DEFAULT_BUFFER_SIZE,
                response);
        }

        private static void ReturnNoResource(HttpListenerRequest request, HttpListenerResponse response)
        {
            WriteFileFromTo(
                HttpStatusCode.NotFound,
                0,
                -1,
                "text/plain",
                new MemoryStream(Encoding.UTF8.GetBytes("Not Found")),
                response.OutputStream,
                Const.DEFAULT_BUFFER_SIZE,
                response);
        }

        private static string GetContentType(string fileName)
        {
            switch (Path.GetExtension(fileName).ToLower())
            {
                case ".txt": return "text/plain";
                case ".csv": return "text/csv";
                case ".html": return "text/html";
                case ".css": return "text/css";
                case ".js": return "text/javascript";
                case ".pdf": return "application/pdf";
                case ".xlsx": return "application/vnd.ms-excel";
                case ".pptx": return "application/vnd.ms-powerpoint";
                case ".docx": return "application/msword";
                case ".jpg": return "image/jpg";
                case ".jpeg": return "image/jpeg";
                case ".png": return "image/png";
                case ".gif": return "image/gif";
                case ".zip": return "application/zip";
                case ".mp3": return "audio/mp3";
                case ".mp4": return "video/mp4";
                case ".m4v": return "video/mp4";
                case ".mpeg": return "video/mpeg";
                default: return "application/octet-stream";
            }
        }

        private static void WriteFileFromTo(HttpStatusCode status, long start, long end, string contentType, string srcFilePath, Stream destStream, int bufferSize, HttpListenerResponse response)
        {
            using (FileStream fileStream = new FileStream(srcFilePath, FileMode.Open, FileAccess.Read))
            {
                WriteFileFromTo(status, start, end, contentType, fileStream, destStream, bufferSize, response);
            }
        }

        private static void WriteFileFromTo(HttpStatusCode status, long start, long end, string contentType, Stream srcStream, Stream destStream, int bufferSize, HttpListenerResponse response)
        {
            byte[] buffer = new byte[bufferSize];
            long limit = end >= 0 ? end : srcStream.Length - 1;

            response.StatusCode = (int)status;
            response.ContentType = contentType;
            response.AppendHeader("Accept-Ranges", "bytes");
            response.ContentLength64 = limit - start + 1;
            response.AppendHeader("Content-Range", $"bytes {start}-{limit}/{srcStream.Length}");

            srcStream.Seek(start, SeekOrigin.Begin);
            long remaining = limit - start + 1;
            int readSize;
            try
            {
                while ((readSize = srcStream.Read(buffer, 0, Math.Min(bufferSize, (int)remaining))) > 0)
                {
                    destStream.Write(buffer, 0, readSize);
                    remaining -= readSize;
                }
            }
            catch (SystemException e)
            {
                _logger.Info("Cancelled");
            }
        }
    }
}
