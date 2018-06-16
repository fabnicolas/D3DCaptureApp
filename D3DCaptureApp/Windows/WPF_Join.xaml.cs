using System;
using System.Threading.Tasks;
using System.Windows;

namespace D3DCaptureApp {
    public partial class WPF_Join:Window {
        NetClient _client;

        public WPF_Join() {
            InitializeComponent();
        }

        public async Task StartClient(string ip, int port) {
            _client=new NetClient(ip,port);
            _client.start_client();
            await Task.Run(() => ReadServerResponse());
        }

        public void ReadServerResponse() {
            Console.WriteLine("Capture started.");
            ScreenCaptureWPFRenderer renderer = new ScreenCaptureWPFRenderer();
            _client.on_server_response((frame_bytes) => {
                Console.WriteLine("[Join] Data="+frame_bytes.Length+".");
                Dispatcher.Invoke(() => ImgCanvas.Source=renderer.render(frame_bytes));
            });
        }
    }
}
