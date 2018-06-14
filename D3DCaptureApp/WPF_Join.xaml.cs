using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace D3DCaptureApp {
    public partial class WPF_Join:Window {
        NetClient _client;

        public WPF_Join() {
            InitializeComponent();
        }

        public async Task StartClient(string ip, int port) {
            _client=new NetClient(ip, port);
            _client.start_client();
            await ReadServerResponse();
        }

        public async Task ReadServerResponse() {
            Console.WriteLine("Capture started.");
            ScreenCaptureWPFRenderer renderer = new ScreenCaptureWPFRenderer();
            await _client.ASYNC_on_server_response((frame_bytes) => {
                Console.Write("[Join] Data="+frame_bytes.Length+"... ");
                Dispatcher.BeginInvoke(new ThreadStart(() => {
                    this.ImageCanvas.Source=renderer.render(frame_bytes);
                    Console.WriteLine("Rendered "+frame_bytes.Length+" bytes.");
                }));
            });
        }
    }
}
