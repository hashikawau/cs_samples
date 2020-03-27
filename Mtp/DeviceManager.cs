using PortableDeviceApiLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mtp {
    public class DeviceManager {
        private readonly PortableDeviceManager _deviceManager
            = new PortableDeviceManager();

        public Device[] Devices {
            get {
                _deviceManager.RefreshDeviceList();
                return QueryDeviceIds(QueryDeviceCount())
                    .Select(x => new Device(x))
                    .ToArray();
            }
        }

        private uint QueryDeviceCount() {
            uint count = 1;
            _deviceManager.GetDevices(null, ref count);
            return count;
        }

        private string[] QueryDeviceIds(uint count) {
            if (count == 0)
                return Array.Empty<string>();
            // Retrieve the device id for each connected device
            string result = "";
            _deviceManager.GetDevices(ref result, ref count);
            return new string[] { result };
            //string[] result = new string[count];
            //_deviceManager.GetDevices(ref result, ref count);
            //return result;
        }
    }
}
