using FileTransfer.Core.Common;
using System;
using System.Collections.Generic;
using System.Net;

namespace FileTransfer.Core.Server
{
    class HttpListenerWrapper
    {
        private static Logger _logger = new Logger(typeof(HttpListenerWrapper).Name);

        private List<string> _prefixes = new List<string>();

        private HttpListener _listener = new HttpListener();

        private Action<HttpListenerContext> _requestHandler;

        internal bool IsListening { get => _listener.IsListening; }

        internal List<string> Prefixes { get => _prefixes; }

        internal void Start(Action<HttpListenerContext> requestHandler)
        {
            _requestHandler = requestHandler ?? throw new ArgumentException();
            foreach (string prefix in _prefixes)
                _listener.Prefixes.Add(prefix);
            _listener.Start();
            _listener.BeginGetContext(ListenRequest, _listener);
        }

        private void ListenRequest(IAsyncResult asyncResult)
        {
            if (_listener.IsListening)
            {
                _listener.BeginGetContext(ListenRequest, _listener);
            }

            HttpListener oldListener = (HttpListener)asyncResult.AsyncState;
            if (oldListener.IsListening)
            {
                HttpListenerContext context = oldListener.EndGetContext(asyncResult);
                _requestHandler.Invoke(context);
            }
        }

        internal void Stop()
        {
            _listener.Stop();
            _listener = new HttpListener();
        }
    }
}