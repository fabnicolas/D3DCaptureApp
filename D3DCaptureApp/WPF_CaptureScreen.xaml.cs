using System.Threading;
using System.Windows;

namespace D3DCaptureApp {
    public partial class WPF_CaptureScreen:Window {
        public WPF_CaptureScreen() {
            InitializeComponent();
            start_capture();
        }

        public void start_capture() {
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
