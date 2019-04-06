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
    class NullRequestHandler : IRequestHandler
    {
        private static Logger _logger = new Logger(typeof(NullRequestHandler).Name);

        public NullRequestHandler()
        {
        }

        public bool CanProcess(HttpListenerContext context)
        {
            return false;
        }

        public void Process(HttpListenerContext context)
        {
            try
            {
                _logger.Info("Process() in");
            }
            finally
            {
                _logger.Info("Process() out");
            }
        }
    }
}
