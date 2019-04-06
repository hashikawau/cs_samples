using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace FileTransfer.Core.Common
{
    public class ServerIsRunnningException : Exception
    {
        private static string MESSAGE_FORMAT = "server is already running";

        internal ServerIsRunnningException()
            : base(string.Format(MESSAGE_FORMAT))
        {
        }
    }

    public class PortNotBoundSslCertException : Exception
    {
        private static string MESSAGE_FORMAT = "port not bound ssl certificate: port={0}. bind by 'netssh http' command";

        internal PortNotBoundSslCertException(int portNo)
            : base(string.Format(MESSAGE_FORMAT, portNo))
        {
        }
    }

    public class SslCertNotRegisteredException : Exception
    {
        private static string MESSAGE_FORMAT = "ssl not registered of key value: keyValue={0}. import by 'mmc' command";

        internal SslCertNotRegisteredException(string keyValue)
            : base(string.Format(MESSAGE_FORMAT, keyValue))
        {
        }
    }
}
