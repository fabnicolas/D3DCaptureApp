using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace D3DCaptureApp {
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
            screen_capture_thread.onFrameReady+=async (sender,frame_bytes) => {
                await _server.ASYNC_sendToAll_bytes(frame_bytes);
            };
            screen_capture_thread.start();
            await Task.FromResult<object>(null);
        }
    }
}
