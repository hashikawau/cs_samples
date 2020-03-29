using PortableDeviceApiLib;
using PortableDeviceTypesLib;
using System;
using System.IO;
using System.Linq;
using IPortableDeviceKeyCollection = PortableDeviceApiLib.IPortableDeviceKeyCollection;
using IPortableDeviceValues = PortableDeviceApiLib.IPortableDeviceValues;

namespace Mtp {
    public class Device : IDisposable {
        private readonly PortableDevice _device
            = new PortableDevice();

        public Device(string deviceId) {
            DeviceId = deviceId;
        }

        public void Dispose()
            => Disconnect();

        public override string ToString()
            => $"{{{GetType().Name}: {nameof(DeviceId)}={DeviceId}, {nameof(Name)}={Name}}}";

        public string DeviceId {
            get;
        }

        public string Name
            => CreateDeviceContent().GetDeviceName();

        public bool IsConnected {
            get;
            private set;
        }

        public void Connect() {
            if (IsConnected)
                return;
            var clientInfo = (IPortableDeviceValues)new PortableDeviceValues();
            _device.Open(DeviceId, clientInfo);
            IsConnected = true;
        }

        public void Disconnect() {
            if (!IsConnected)
                return;
            _device.Close();
            IsConnected = false;
        }

        private DeviceContent CreateDeviceContent() {
            Connect();
            _device.Content(out IPortableDeviceContent content);
            return new DeviceContent(content);
        }

        public string[] Ls(string path = "/") {
            var content = CreateDeviceContent();
            var baseObject = content.FindObject(path);
            if (baseObject == null)
                return Array.Empty<string>();
            if (!baseObject.IsDirectory)
                return new string[] { baseObject.Name };
            return content.ListChildrenIds(baseObject)
                .Select(info => info.Name)
                .ToArray();
        }

        public void CopyFrom(
            string srcFilePath,
            string destDirPath,
            string destFileName = null
        ) {
            var content = CreateDeviceContent();
            var fileName = destFileName ?? Path.GetFileName(srcFilePath);
            content.CopyFrom(srcFilePath, destDirPath, fileName);
        }

        public void CopyTo(
            string srcFilePath,
            string destDirPath,
            string destFileName = null
        ) {
            if (!File.Exists(srcFilePath))
                throw new LocalFileNotExistsException(srcFilePath);
            var content = CreateDeviceContent();
            var fileName = destFileName ?? Path.GetFileName(srcFilePath);
            content.Rm(Path.Combine(destDirPath, fileName));
            content.Mkdirs(destDirPath);
            content.CopyTo(srcFilePath, destDirPath, fileName);
        }

        public void Rm(
            string filePath
        ) {
            var content = CreateDeviceContent();
            content.Rm(filePath);
        }

        public void Mkdirs(
            string dirPath
        ) {
            var content = CreateDeviceContent();
            content.Mkdirs(dirPath);
        }

    }

    class ObjectInfo {
        public ObjectInfo(
            string id,
            string name,
            bool isDirectory,
            long size
        ) {
            this.Id = id;
            this.Name = name;
            this.IsDirectory = isDirectory;
            this.Size = size;
        }
        public string Id { get; private set; }
        public string Name { get; private set; }
        public bool IsDirectory { get; private set; }
        public long Size { get; private set; }

        public override string ToString() {
            return $"{{{nameof(ObjectInfo)}: {nameof(Id)}={Id}, {nameof(Name)}={Name}, {nameof(IsDirectory)}={IsDirectory}, {nameof(Size)}={Size}}}";
        }

        public static ObjectInfo Create(
            IPortableDeviceProperties properties,
            string objectId
        ) {
            properties.GetSupportedProperties(objectId, out IPortableDeviceKeyCollection keys);
            properties.GetValues(objectId, keys, out IPortableDeviceValues values);
            values.GetStringValue(ContentTypes.WPD_OBJECT_NAME, out string name);
            values.GetGuidValue(ContentTypes.WPD_OBJECT_CONTENT_TYPE, out Guid contentType);

            if (contentType == ContentTypes.WPD_CONTENT_TYPE_FOLDER || contentType == ContentTypes.WPD_CONTENT_TYPE_FUNCTIONAL_OBJECT)
                return new ObjectInfo(objectId, name, true, 0);

            // Get the original name of the object
            try {
                values.GetStringValue(ContentTypes.WPD_OBJECT_ORIGINAL_FILE_NAME, out name);
            } catch (Exception e) {
                name = "";
            }
            values.GetUnsignedIntegerValue(ContentTypes.WPD_OBJECT_SIZE, out uint size);
            return new ObjectInfo(objectId, name, false, (long)size);
        }
    }

}
