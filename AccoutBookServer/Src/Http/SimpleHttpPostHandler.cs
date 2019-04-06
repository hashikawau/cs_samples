using CSSamples.Common.Logger;
using System;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace AccoutBookServer.Http
{
    public class SimpleHttpPostHandler : IRequestHandler
    {
        static Logger _logger = Logger.GetLogger(typeof(SimpleHttpPostHandler));
        public delegate string HandlerFunction(SimpleHttpPostHandler sender, HttpListenerContext context);

        Regex _pathPattern;
        string _templateHtmlPath;
        HandlerFunction _handler;

        public string[] UrlParameters { get; private set; }

        public SimpleHttpPostHandler(
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

            if (request.HttpMethod != WebRequestMethods.Http.Post)
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
            context.Response.Redirect(_handler.Invoke(this, context));
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
