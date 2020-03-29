using Mtp;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;

namespace MtpFileTransfer {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
            DataContext = new MainWindowViewModel();
        }

        private MainWindowViewModel ViewModel
            => DataContext as MainWindowViewModel;

        private string PcSourceDirectoryPath
            => Properties.Settings.Default.PcSourceDirectoryPath;

        private string DeviceDestinationDirectoryPath
            => Properties.Settings.Default.DeviceDestinationDirectoryPath;

        private ICollection<string> PcSourceFileRelativePaths
            => Properties.Settings.Default.PcSourceFileRelativePaths.Cast<string>().ToList();

        private string DeviceSourceDirectoryPath
            => Properties.Settings.Default.DeviceSourceDirectoryPath;

        private string PcDestinationDirectoryPath
            => Properties.Settings.Default.PcDestinationDirectoryPath;

        private ICollection<string> DeviceSourceFileRelativePaths
            => Properties.Settings.Default.DeviceSourceFileRelativePaths.Cast<string>().ToList();

        private void Button_Click_0(object sender, RoutedEventArgs e) {
            ViewModel.UpdateDevice();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e) {
            ViewModel.CopyFilesToDevice(
                PcSourceDirectoryPath,
                DeviceDestinationDirectoryPath,
                PcSourceFileRelativePaths
            );
        }

        private void Button_Click_2(object sender, RoutedEventArgs e) {
            ViewModel.CopyFilesFromDevice(
                DeviceSourceDirectoryPath,
                PcDestinationDirectoryPath,
                DeviceSourceFileRelativePaths
            );
        }
    }

    class MainWindowViewModel : INotifyPropertyChanged {
        public event PropertyChangedEventHandler PropertyChanged;

        private DeviceManager _deviceManager
            = new DeviceManager();

        public string DeviceName
            => _deviceManager.Devices.FirstOrDefault()?.Name ?? "なし";

        public bool IsDeviceConnected
            => _deviceManager.Devices.FirstOrDefault() != null;

        public void UpdateDevice() {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DeviceName)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsDeviceConnected)));
        }

        public void CopyFilesToDevice(
            string srcRootDir,
            string destRootDir,
            ICollection<string> srcFilePaths
        ) {
            var device = _deviceManager.Devices.FirstOrDefault();
            if (device == null)
                return;

            foreach (var relativePath in srcFilePaths) {
                device.CopyTo(
                    Path.Combine(srcRootDir, relativePath),
                    Path.Combine(destRootDir, Path.GetDirectoryName(relativePath))
                );
            }
        }

        internal void CopyFilesFromDevice(
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
}
