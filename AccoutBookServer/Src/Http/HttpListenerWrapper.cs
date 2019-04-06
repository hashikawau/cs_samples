using CSSamples.Common.Logger;
using System;
using System.Collections.Generic;
using System.Net;

namespace AccoutBookServer.Http
{
    public class HttpListenerWrapper
    {
        static Logger _logger = Logger.GetLogger(typeof(HttpListenerWrapper));

        HttpListener _listener;

        List<IRequestHandler> _requestHandlers;

        public HttpListenerWrapper()
        {
            _listener = new HttpListener();
            _requestHandlers = new List<IRequestHandler>();
        }

        public bool IsListening { get => _listener.IsListening; }

        public void AddRequestHandler(IRequestHandler handler)
        {
            if (handler != null)
                _requestHandlers.Add(handler);
        }

        public void RemoveRequestHandler(IRequestHandler handler)
        {
            if (handler != null)
                _requestHandlers.Remove(handler);
        }

        public void Start(string prefix)
        {
            _listener.Prefixes.Add(prefix);
            _listener.Start();
            _listener.BeginGetContext(ListenRequest, _listener);
        }

        public void Stop()
        {
            _listener.Stop();
            _listener = new HttpListener();
        }

        void ListenRequest(IAsyncResult asyncResult)
        {
            if (_listener.IsListening)
            {
                _listener.BeginGetContext(ListenRequest, _listener);
            }

            HttpListener oldListener = (HttpListener)asyncResult.AsyncState;
            if (oldListener.IsListening)
            {
                HttpListenerContext context = oldListener.EndGetContext(asyncResult);
                foreach (var handler in _requestHandlers)
                {
                    if (handler.CanProcess(context))
                    {
                        handler.Process(context);
                        return;
                    }
                }
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                context.Response.Close();
            }
        }

    }
}