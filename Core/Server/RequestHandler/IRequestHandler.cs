using System.Net;

namespace FileTransfer.Core.Server.RequestHandler
{
    interface IRequestHandler
    {
        void Process(HttpListenerContext context);
        bool CanProcess(HttpListenerContext context);
    }
}
