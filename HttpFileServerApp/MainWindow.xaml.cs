using FileTransfer.Core.Server;
using System.Windows;

namespace FileTransfer.ServerApp
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            _server = HttpServer.Create();

            StopServer_Button.Click += (sender, ev) =>
            {
                _server.Stop();
            };
            StartServer_Button.Click += (sender, ev) =>
            {
                _server.Protocol = "https";
                _server.HostName = "192.168.2.201";
                _server.PortNo = 4430;
                _server.OutDirectory = "C:/Users/hashikawa/Downloads";
                _server.InDirectory = "C:/Users/hashikawa/Downloads";
                _server.Start();
            };
            ContentRendered += (sender, ev) =>
            {
#if DEBUG
                _server.Protocol = "https";
                _server.HostName = "192.168.2.201";
                _server.PortNo = 4430;
#else
                _server.Protocol = "http";
                _server.HostName = "192.168.2.201";
                _server.PortNo = 80;
#endif
                _server.OutDirectory = "C:/Users/hashikawa/Downloads";
                _server.InDirectory = "C:/Users/hashikawa/Downloads";
                _server.Start();
            };
        }

        HttpServer _server;
    }
}
