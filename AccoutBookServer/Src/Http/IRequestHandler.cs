using System.Net;

namespace AccoutBookServer.Http
{
    public interface IRequestHandler
    {
        void Process(HttpListenerContext context);
        bool CanProcess(HttpListenerContext context);
    }
}
