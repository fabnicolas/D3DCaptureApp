using D3DCaptureApp.Impl;
using System.Threading.Tasks;
using System.Windows;

namespace D3DCaptureApp {
    public partial class WPF_Host:Window {
        ScreenServer _server;

        public WPF_Host() {
            InitializeComponent();
            _server=new ScreenServer();
        }

        public async Task StartServer(string ip, int port) {
            await _server.StartServer(ip,port);
        }
    }
}
