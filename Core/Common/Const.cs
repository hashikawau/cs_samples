namespace FileTransfer.Core.Common
{
    class Const
    {
#if DEBUG
        internal static int DEFAULT_BUFFER_SIZE = 1;
#else
        internal static int DEFAULT_BUFFER_SIZE = 1024 * 1024 * 8;
#endif
        internal static string DEFAULT_SSL_CERTIFICATE = "localhost";
    }
}
