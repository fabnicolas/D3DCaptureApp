using D3DCaptureApp.Impl;
using System.Threading.Tasks;
using System.Windows;

namespace D3DCaptureApp {
    public partial class WPF_Join:Window {
        ScreenClient _client;

        public WPF_Join() {
            InitializeComponent();
            _client=new ScreenClient();
        }

        public async Task StartClient(string ip, int port) {
            _client.OnServerMessage+=(frame) => Dispatcher.Invoke(() => this.ImgCanvas.Source=frame);
            await _client.StartClient(ip,port);
        }
    }
}
