using System;
using System.Threading.Tasks;
using System.Windows;

namespace D3DCaptureApp {
    /// <summary>
    /// 
    /// </summary>
    public partial class WPF_Host:Window {
        NetServer _server;

        public WPF_Host() {
            InitializeComponent();
        }

        public async Task StartServer(string ip,int port) {
            _server = new NetServer(ip, port);
            _server.start_server();
            ScreenCaptureThread screen_capture_thread = new ScreenCaptureThread();
            ScreenCaptureWPFRenderer renderer = new ScreenCaptureWPFRenderer();
            screen_capture_thread.onFrameReady+=(sender,frame_bytes) => {
                _server.sendToAll_bytes(frame_bytes);
                System.Threading.Thread.Sleep(300);
            };
            screen_capture_thread.start();

            INJECT_NETCLIENT(ip,port);
            await Task.FromResult<object>(null);
        }

        NetClient _client;
        private void INJECT_NETCLIENT(string ip,int port) {
            _client = new NetClient(ip,port);
            _client.start_client();
            Task.Run(()=>ReadServerResponse());
        }

        public void ReadServerResponse() {
            Console.WriteLine("Capture started.");
            ScreenCaptureWPFRenderer renderer = new ScreenCaptureWPFRenderer();
            _client.on_server_response((frame_bytes) => {
                Console.WriteLine("[Join] Data="+frame_bytes.Length+".");
            });
        }
    }
}
