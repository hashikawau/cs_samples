using Mtp;
using System.Windows;

namespace MtpFileTransfer {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e) {
            var deviceManager = new DeviceManager();
            var device = deviceManager.Devices[0];
            Log("device={0}", device);
            Log("ls_1={0}", string.Join("\n", device.Ls()));
        }

        private void Button_Click_2(object sender, RoutedEventArgs e) {
        }

        private void Log(string format, params object[] args) {
            if (!string.IsNullOrEmpty(LogText.Text))
                LogText.AppendText("\n");
            LogText.AppendText(string.Format(format, args));
            LogText.ScrollToEnd();
        }
    }

}
