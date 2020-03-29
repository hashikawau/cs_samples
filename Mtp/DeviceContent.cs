using PortableDeviceApiLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using IStream = System.Runtime.InteropServices.ComTypes.IStream;

namespace Mtp {
    class DeviceContent {
        private readonly ObjectInfo ROOT = new ObjectInfo("DEVICE", "DEVICE", true, 0);

        private IPortableDeviceContent _content;

        public DeviceContent(IPortableDeviceContent content) {
            this._content = content;
        }

        public string GetDeviceName() {
            _content.Properties(out IPortableDeviceProperties properties);
            // Retrieve the values for the properties
            properties.GetValues("DEVICE", null, out IPortableDeviceValues propertyValues);
            // Identify the property to retrieve
            // Get the name of the object
            propertyValues.GetStringValue(ContentTypes.WPD_OBJECT_NAME, out string name);
            return name;
        }

        public ObjectInfo FindObject(
            string path
        ) {
            var baseObject = ROOT;
            var sectors = PathUtils.SplitPath(path);
            for (int i = 0; i < sectors.Length; ++i) {
                baseObject = FindObject(baseObject, sectors[i]);
                if (baseObject == null)
                    throw new RemoteFileNotExistsException(string.Join("/", sectors.Take(i + 1)));
            }
            return baseObject;
        }

        public ObjectInfo FindObject(
            ObjectInfo baseObject,
            string childName
        )
            => ListChildrenIds(baseObject)
                .FirstOrDefault(info => info.Name == childName);

        public ObjectInfo[] ListChildrenIds(
            ObjectInfo baseObject
        ) {
            if (baseObject == null)
                return Array.Empty<ObjectInfo>();
            // Get the properties of the object
            _content.Properties(out IPortableDeviceProperties properties);
            // Enumerate the items contained by the current object
            _content.EnumObjects(
                0,
                baseObject.Id,
                null,
                out IEnumPortableDeviceObjectIDs objectIds
            );

            var result = new List<ObjectInfo>();
            while (true) {
                uint fetched = 0;
                objectIds.Next(1, out string objectId, ref fetched);

                if (fetched == 0)
                    break;

                result.Add(ObjectInfo.Create(
                    properties,
                    objectId
                ));
            }

            return result.ToArray();
        }

        public void Mkdirs(
            string dirPath
        ) {
            var baseObject = ROOT;
            foreach (var sector in PathUtils.SplitPath(dirPath)) {
                baseObject = Mkdir(baseObject, sector);
            }
        }

        private ObjectInfo Mkdir(
            ObjectInfo baseObject,
            string sector
        ) {
            var dirObject = FindObject(baseObject, sector);
            if (dirObject != null)
                return dirObject;

            string dirId = null;
            _content.CreateObjectWithPropertiesOnly(
                GetRequiredPropertiesForFolder(sector, baseObject.Id),
                ref dirId
            );
            return new ObjectInfo(dirId, sector, true, 0);
        }

        public void Rm(
            string filePath
        ) {
            Rm(FindObject(filePath));
        }

        private void Rm(ObjectInfo fileInfo) {
            if (fileInfo == null)
                return;

            StringToPropVariant(fileInfo.Id, out tag_inner_PROPVARIANT variant);

            var objectIds = new PortableDeviceTypesLib.PortableDevicePropVariantCollection() as IPortableDevicePropVariantCollection;
            objectIds.Add(variant);

            _content.Delete(0, objectIds, null);
        }

        private static void StringToPropVariant(
            string value,
            out tag_inner_PROPVARIANT propvarValue
        ) {
            var pValues = new PortableDeviceTypesLib.PortableDeviceValues() as IPortableDeviceValues;
            pValues.SetStringValue(ContentTypes.WPD_OBJECT_ID, value);
            pValues.GetValue(ContentTypes.WPD_OBJECT_ID, out propvarValue);
        }

        public void CopyFrom(
            string srcFilePath,
            string destDirPath,
            string destFileName
        ) {
            CopyFrom(
                FindObject(srcFilePath),
                Path.Combine(destDirPath, destFileName)
            );
        }

        private void CopyFrom(
            ObjectInfo srcFileObject,
            string destFilePath
        ) {
            _content.Transfer(out IPortableDeviceResources resources);

            uint optimalTransferSizeBytes = 0;
            resources.GetStream(
                srcFileObject.Id,
                ContentTypes.WPD_RESOURCE_DEFAULT,
                ContentTypes.STGM_READ,
                ref optimalTransferSizeBytes,
                out PortableDeviceApiLib.IStream tempStream
            );
            IStream targetStream = (IStream)tempStream;
            try {
                var destDirPath = Path.GetDirectoryName(destFilePath);
                if (!Directory.Exists(destDirPath))
                    Directory.CreateDirectory(destDirPath);
                unsafe {
                    using (var ostream = new BufferedStream(new FileStream(destFilePath, FileMode.OpenOrCreate, FileAccess.Write))) {
                        var buffer = new byte[optimalTransferSizeBytes];
                        int readSize;
                        IntPtr pcbRead = new IntPtr(&readSize);
                        while (true) {

                            //}
                            //for (long remainsSize = srcFileObject.Size; remainsSize > 0; remainsSize -= optimalTransferSizeBytes) {
                            targetStream.Read(
                                buffer,
                                (int)optimalTransferSizeBytes,
                                pcbRead
                            );
                            if (readSize <= 0)
                                break;
                            //Debug.WriteLine("optimal={0}, read={1}", optimalTransferSizeBytes, pcbRead.ToInt32());
                            //Debug.WriteLine("str={0}", (object)Encoding.UTF8.GetString(buffer, 0, (int)optimalTransferSizeBytes));

                            ostream.Write(buffer, 0, (int)Math.Min((long)optimalTransferSizeBytes, readSize));
                        }
                    }
                }
            } finally {
                Marshal.ReleaseComObject(tempStream);
            }
        }

        public void CopyTo(
            string srcFilePath,
            string destDirPath,
            string destFileName
        ) {
            CopyTo(
                srcFilePath,
                FindObject(destDirPath),
                destFileName
            );
        }

        private void CopyTo(
            string srcFilePath,
            ObjectInfo destDirObject,
            string destFileName
        ) {
            FileInfo fileInfo = new FileInfo(srcFilePath);
            IPortableDeviceValues values = GetRequiredPropertiesForContentType(
                destFileName,
                fileInfo.Length,
                destDirObject.Id
            );

            PortableDeviceApiLib.IStream tempStream;
            uint optimalTransferSizeBytes = 0;
            _content.CreateObjectWithPropertiesAndData(
                values,
                out tempStream,
                ref optimalTransferSizeBytes,
                null
            );

            IStream targetStream = (IStream)tempStream;
            try {
                using (var sourceStream = new FileStream(srcFilePath, FileMode.Open, FileAccess.Read)) {
                    var buffer = new byte[optimalTransferSizeBytes];
                    int bytesRead;
                    while (true) {
                        bytesRead = sourceStream.Read(buffer, 0, (int)optimalTransferSizeBytes);

                        if (bytesRead <= 0) {
                            break;
                        }

                        IntPtr pcbWritten = IntPtr.Zero;
                        targetStream.Write(
                            buffer,
                            (int)bytesRead,
                            pcbWritten
                        );
                    }
                }
                targetStream.Commit(0);
            } finally {
                Marshal.ReleaseComObject(tempStream);
            }
        }

        private IPortableDeviceValues GetRequiredPropertiesForContentType(
            string fileName,
            long fileSize,
            string parentObjectId
        ) {
            IPortableDeviceValues values = new PortableDeviceTypesLib.PortableDeviceValues() as IPortableDeviceValues;
            values.SetStringValue(ContentTypes.WPD_OBJECT_PARENT_ID, parentObjectId);
            values.SetUnsignedLargeIntegerValue(ContentTypes.WPD_OBJECT_SIZE, (ulong)fileSize);
            values.SetStringValue(ContentTypes.WPD_OBJECT_ORIGINAL_FILE_NAME, fileName);
            values.SetStringValue(ContentTypes.WPD_OBJECT_NAME, fileName);
            return values;
        }

        private IPortableDeviceValues GetRequiredPropertiesForFolder(
            string fileName,
            string parentObjectId
        ) {
            IPortableDeviceValues values = new PortableDeviceTypesLib.PortableDeviceValues() as IPortableDeviceValues;
            values.SetStringValue(ContentTypes.WPD_OBJECT_PARENT_ID, parentObjectId);
            values.SetStringValue(ContentTypes.WPD_OBJECT_NAME, Path.GetFileName(fileName));
            values.SetGuidValue(ContentTypes.WPD_OBJECT_CONTENT_TYPE, ContentTypes.WPD_CONTENT_TYPE_FOLDER);
            return values;
        }

    }

    static class ContentTypes {
        public static _tagpropertykey WPD_OBJECT_ID
            => Create(new Guid(0xEF6B490D, 0x5CD8, 0x437A, 0xAF, 0xFC, 0xDA, 0x8B, 0x60, 0xEE, 0x4A, 0x3C), 2);

        public static _tagpropertykey WPD_OBJECT_PARENT_ID
            => Create(new Guid(0xEF6B490D, 0x5CD8, 0x437A, 0xAF, 0xFC, 0xDA, 0x8B, 0x60, 0xEE, 0x4A, 0x3C), 3);

        public static _tagpropertykey WPD_OBJECT_NAME
            => Create(new Guid(0xEF6B490D, 0x5CD8, 0x437A, 0xAF, 0xFC, 0xDA, 0x8B, 0x60, 0xEE, 0x4A, 0x3C), 4);

        public static _tagpropertykey WPD_OBJECT_CONTENT_TYPE
            => Create(new Guid(0xEF6B490D, 0x5CD8, 0x437A, 0xAF, 0xFC, 0xDA, 0x8B, 0x60, 0xEE, 0x4A, 0x3C), 7);

        public static _tagpropertykey WPD_OBJECT_SIZE
            => Create(new Guid(0xEF6B490D, 0x5CD8, 0x437A, 0xAF, 0xFC, 0xDA, 0x8B, 0x60, 0xEE, 0x4A, 0x3C), 11);

        public static _tagpropertykey WPD_OBJECT_ORIGINAL_FILE_NAME
            => Create(new Guid(0xEF6B490D, 0x5CD8, 0x437A, 0xAF, 0xFC, 0xDA, 0x8B, 0x60, 0xEE, 0x4A, 0x3C), 12);

        public static _tagpropertykey WPD_RESOURCE_DEFAULT
            => Create(new Guid(0xE81E79BE, 0x34F0, 0x41BF, 0xB5, 0x3F, 0xF1, 0xA0, 0x6A, 0xE8, 0x78, 0x42), 0);

        private static _tagpropertykey Create(Guid fmtid, uint pid) {
            var result = new _tagpropertykey();
            result.fmtid = fmtid;
            result.pid = pid;
            return result;
        }

        public static Guid WPD_CONTENT_TYPE_FUNCTIONAL_OBJECT
            => new Guid(0x99ED0160, 0x17FF, 0x4C44, 0x9D, 0x98, 0x1D, 0x7A, 0x6F, 0x94, 0x19, 0x21);

        public static Guid WPD_CONTENT_TYPE_FOLDER
            => new Guid(0x27E2E392, 0xA111, 0x48E0, 0xAB, 0x0C, 0xE1, 0x77, 0x05, 0xA0, 0x5F, 0x85);

        public const uint STGM_READ = 0x00000000;
    }

}
