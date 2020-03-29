using System;

namespace Mtp {
    public class MtpException : Exception {
        public MtpException(string message) : base(message) { }
    }

    public class LocalFileNotExistsException : MtpException {
        public LocalFileNotExistsException(string path) : base($"local file not exists: path={path}") { }
    }

    public class RemoteFileNotExistsException : MtpException {
        public RemoteFileNotExistsException(string path) : base($"remote file not exists: path={path}") { }
    }

}
