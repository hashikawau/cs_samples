using FileTransfer.Core.Common;
using FileTransfer.Core.Server.RequestHandler;
using FileTransfer.Core.Util;
using System;
using System.Collections.Generic;
using System.Net;

namespace FileTransfer.Core.Server
{
    public class HttpServer : IDisposable
    {
        private static Logger _logger = new Logger(typeof(HttpServer).Name);

        public static HttpServer Create()
        {
            var result = new HttpServer();
            return result;
        }

        private HttpServer()
        {
        }

        private HttpListenerWrapper _listener = new HttpListenerWrapper();

        private IRequestHandler _httpGetRequestHandler = new NullRequestHandler();
        private IRequestHandler _httpPostRequestHandler = new NullRequestHandler();
        private IRequestHandler _httpOtherRequestHandler = new HttpOtherRequestHandler();

        private string _protocol = "https";
        private string _hostName = "localhost";
        private int _portNo = 443;
        private string Prefix { get => $"{_protocol}://{_hostName}:{_portNo}/"; }

        public string OutDirectory
        {
            get => _httpGetRequestHandler.GetType() != typeof(HttpGetRequestHandler)
                ? null
                : ((HttpGetRequestHandler)_httpGetRequestHandler).Root;
            set => _httpGetRequestHandler = value == null
                ? (IRequestHandler)new NullRequestHandler()
                : (IRequestHandler)new HttpGetRequestHandler(value);
        }

        public string InDirectory
        {
            get => _httpPostRequestHandler.GetType() != typeof(HttpPostRequestHandler)
               ? null
               : ((HttpPostRequestHandler)_httpPostRequestHandler).Root;
            set => _httpPostRequestHandler = value == null
                ? (IRequestHandler)new NullRequestHandler()
                : (IRequestHandler)new HttpPostRequestHandler(value);
        }

        public string Protocol { get => _protocol; set => _protocol = value; }
        public string HostName { get => _hostName; set => _hostName = value; }
        public int PortNo { get => _portNo; set => _portNo = value; }

        public void Start()
        {
            try
            {
                _logger.Info("Start() in");
                StartImpl();
            }
            finally
            {
                _logger.Info("Start() out");
            }
        }

        private void StartImpl()
        {
            if (_listener.IsListening)
                throw new ServerIsRunnningException();

            string sslCertificateHash = StartUp.GetSslCertificateHashBoundToPort(PortNo);
            //if (sslCertificateHash == null)
            //    sslCertificateHash = StartUp.BindSslCertificateToPort(PortNo, Const.DEFAULT_SSL_CERTIFICATE);
            //if (sslCertificateHash == null)
            //    throw new PortNotBoundSslCertException(PortNo);

            //if (StartUp.FindSslCertificateByThumbprint(sslCertificateHash) == null)
            //    throw new SslCertNotRegisteredException(sslCertificateHash);

            _listener.Prefixes.Add(Prefix);
            _listener.Start(HandleRequest);
        }

        private void HandleRequest(HttpListenerContext context)
        {
            try
            {
                _logger.Info("HandleRequest() in");
                var handler
                    = _httpGetRequestHandler.CanProcess(context) ? _httpGetRequestHandler
                    : _httpPostRequestHandler.CanProcess(context) ? _httpPostRequestHandler
                    : _httpOtherRequestHandler;
                handler.Process(context);
            }
            finally
            {
                _logger.Info("HandleRequest() out");
            }
        }

        public void Stop()
        {
            try
            {
                _logger.Info("Stop() in");
                StopImpl();
            }
            finally
            {
                _logger.Info("Stop() out");
            }
        }

        private void StopImpl()
        {
            if (_listener.IsListening)
                _listener.Stop();
        }

        public void Dispose()
        {
            try
            {
                _logger.Info("Dispose() in");
                StopImpl();
            }
            finally
            {
                _logger.Info("Dispose() out");
            }
        }
    }
}