using Mtp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace MtpFileTransfer {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        private readonly MainWindowViewModel ViewModel
            = new MainWindowViewModel();

        private readonly DeviceManager _deviceManager
            = new DeviceManager();

        public MainWindow() {
            InitializeComponent();
            DataContext = ViewModel;
            UpdateDeviceAsync().Wait();
        }

        private Device _device;

        private void Button_Click_0(object sender, RoutedEventArgs e) {
            UpdateDeviceAsync();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e) {
            CopyFilesToDeviceAsync();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e) {
            //ViewModel.CopyFilesFromDevice(
            //    DeviceSourceDirectoryPath,
            //    PcDestinationDirectoryPath,
            //    DeviceSourceFileRelativePaths
            //);
        }

        private Task UpdateDeviceAsync() {
            return Service
                .GetPrimaryDeviceAsync(_deviceManager)
                .ContinueWith(task => {
                    _device = task.Result;
                    ViewModel.UpdateDevice(_device);
                });
        }

        private Task CopyFilesToDeviceAsync() {
            return Service
                .CopyFilesToDeviceAsync(_device)
                //.ContinueWith();
                ;
        }

        public void CopyFilesFromDevice(
            string srcRootDir,
            string destRootDir,
            ICollection<string> srcFilePaths
        ) {
            var device = _deviceManager.Devices.FirstOrDefault();
            if (device == null)
                return;

            foreach (var relativePath in srcFilePaths) {
                device.CopyFrom(
                    Path.Combine(srcRootDir, relativePath),
                    Path.Combine(destRootDir, Path.GetDirectoryName(relativePath))
                );
            }
        }

    }

    class MainWindowViewModel : INotifyPropertyChanged {
        private Device _device;

        public event PropertyChangedEventHandler PropertyChanged;

        public string DeviceName
            => _device?.Name ?? "なし";

        public bool IsDeviceConnected
            => _device != null;

        public void UpdateDevice(Device device) {
            _device = device;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DeviceName)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsDeviceConnected)));
        }

    }

    public static class Service {
        public static Task<Device> GetPrimaryDeviceAsync(DeviceManager deviceManager) {
            return Task.Run(() =>
                deviceManager.Devices.FirstOrDefault()
            );
        }

        public static Task<List<Tuple<string, string, Exception>>> CopyFilesToDeviceAsync(Device device) {
            return Task.Run(() => {
                if (device == null)
                    throw new NoDeviceConnectedException();
                device.Connect();
                return CopyToDeviceSettings
                    .SourceAndDestinationPaths
                    .Select(srcAndDst => CopyFileToDevice(device, srcAndDst))
                    .ToList();
            });
        }

        private static Tuple<string, string, Exception> CopyFileToDevice(
            Device device,
            Tuple<string, string> srcAndDst
        ) {
            var src = srcAndDst.Item1;
            var dst = srcAndDst.Item2;
            try {
                device.CopyTo(src, dst);
                return Tuple.Create(src, dst, null as Exception);
            } catch (Exception e) {
                return Tuple.Create(src, dst, e);
            }
        }
    }

    static class CopyToDeviceSettings {
        public static List<Tuple<string, string>> SourceAndDestinationPaths
            => SrcRelatives
                .Select(relative => Tuple.Create(
                    Path.Combine(SrcRoot, relative),
                    Path.Combine(DstRoot, Path.GetDirectoryName(relative))
                ))
                .ToList();
        private static string SrcRoot
            => Properties.Settings.Default.CopyToDevice_SourceRootDirectoryPath;
        private static string DstRoot
            => Properties.Settings.Default.CopyToDevice_DestinationRootDirectoryPath;
        private static List<string> SrcRelatives
            => Properties.Settings.Default.CopyToDevice_SourceFileRelativePaths.Cast<string>().ToList();
    }

    static class CopyFromDeviceSettings {
        public static List<Tuple<string, string>> SourceAndDestinationPaths
            => SrcRelatives
                .Select(relative => Tuple.Create(
                    Path.Combine(SrcRoot, relative),
                    Path.Combine(DstRoot, Path.GetDirectoryName(relative))
                ))
                .ToList();
        private static string SrcRoot
            => Properties.Settings.Default.CopyFromDevice_SourceRootDirectoryPath;
        private static string DstRoot
            => Properties.Settings.Default.CopyFromDevice_DestinationRootDirectoryPath;
        private static List<string> SrcRelatives
            => Properties.Settings.Default.CopyFromDevice_SourceFileRelativePaths.Cast<string>().ToList();
    }

    public class AppException : Exception {
        public AppException(string message) : base(message) { }
    }

    public class NoDeviceConnectedException : AppException {
        public NoDeviceConnectedException() : base("No device connected") { }
    }

}
