using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mtp {
    class MtpException : Exception {
        public MtpException(string message) : base(message) { }
    }
}
