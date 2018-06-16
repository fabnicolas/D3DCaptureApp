using Serilog;
using Serilog.Core;
using SerilogLoggerSystem;
using System.Threading;
using System.Windows;

namespace D3DCaptureApp {
    public partial class WPF_CaptureScreen:Window {
        private readonly Logger logger = SerilogFactory.GetLogger();

        public WPF_CaptureScreen() {
            InitializeComponent();
            start_capture();
        }

        public void start_capture() {
            logger.Information("Capture started.");
            ScreenCaptureThread screen_capture_thread = new ScreenCaptureThread();
            ScreenCaptureWPFRenderer renderer = new ScreenCaptureWPFRenderer();
            screen_capture_thread.onFrameReady+=(sender,frame_bytes) => {
                Dispatcher.BeginInvoke(new ThreadStart(() =>
                    ImgCanvas.Source=renderer.render(frame_bytes)
                ));
            };
            screen_capture_thread.start();
        }

        private void fullscreen() {
            WindowStyle=WindowStyle.None;
            WindowState=WindowState.Maximized;
        }

        private void listener_button_fullscreen(object sender,RoutedEventArgs e) {
            fullscreen();
        }
    }
}
