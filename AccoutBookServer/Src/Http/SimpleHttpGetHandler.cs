using CSSamples.Common.Logger;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace AccoutBookServer.Http
{
    public class SimpleHttpGetHandler : IRequestHandler
    {
        static Logger _logger = Logger.GetLogger(typeof(SimpleHttpGetHandler));
        public delegate Dictionary<string, string> HandlerFunction(SimpleHttpGetHandler sender, HttpListenerContext context);

        Regex _pathPattern;
        string _templateHtmlPath;
        HandlerFunction _handler;

        public string[] UrlParameters { get; private set; }

        public SimpleHttpGetHandler(
            string pathPatternRegex,
            string templateHtmlPath,
            HandlerFunction handler)
        {
            _pathPattern = new Regex(pathPatternRegex);
            _templateHtmlPath = templateHtmlPath;
            _handler = handler;
        }

        public bool CanProcess(HttpListenerContext context)
        {
            var request = context.Request;

            //_logger.Debug("method={0}, url={1}, pattern={2}", request.HttpMethod, request.RawUrl, PathPattern);

            if (request.HttpMethod != WebRequestMethods.Http.Get)
                return false;

            if (!_pathPattern.IsMatch(request.RawUrl))
                return false;

            UrlParameters = _pathPattern.Matches(request.RawUrl).Cast<Match>()
                .SelectMany(match => match.Groups.Cast<Group>()
                    .Select(group => group.Value))
                //.SelectMany(group => group.Captures.Cast<Capture>()
                //    .Select(capture => capture.Value)))
                .ToArray();

            return true;
        }

        public void Process(HttpListenerContext context) => RunWithExceptionHandling(context, _ =>
        {
            context.Response.StatusCode = (int)HttpStatusCode.OK;
            context.Response.ContentType = "text/html";
            context.Response.ContentEncoding = Encoding.UTF8;
            using (var istream = new FileStream(_templateHtmlPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var dict = _handler?.Invoke(this, context);
                if (dict == null)
                {
                    context.Response.ContentLength64 = istream.Length;
                    istream.CopyTo(context.Response.OutputStream);
                    return;
                }

                using (var reader = new StreamReader(istream))
                {
                    var template = reader.ReadToEnd();
                    foreach (var pair in dict)
                        template = template.Replace($"{{{pair.Key}}}", pair.Value);
                    var contens = Encoding.UTF8.GetBytes(template);
                    context.Response.ContentLength64 = contens.Length;
                    context.Response.OutputStream.Write(contens, 0, contens.Length);
                }
            }
        });

        void RunWithExceptionHandling(HttpListenerContext context, Action<HttpListenerContext> action)
        {
            try
            {
                _logger.Info("in: parameters={0}", string.Join(", ", UrlParameters));
                action.Invoke(context);
            }
            catch (Exception e)
            {
                _logger.Error(e);
            }
            finally
            {
                context.Response.Close();
                _logger.Info("out");
            }
        }
    }
}
