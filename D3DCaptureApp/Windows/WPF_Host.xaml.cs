using Serilog.Core;
using SerilogLoggerSystem;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace D3DCaptureApp {
    public partial class WPF_Host:Window {
        private static readonly Logger logger = SerilogFactory.GetLogger();

        NetServer _server;
        private Bitmap previous_image;

        public WPF_Host() {
            InitializeComponent();
            previous_image=null;
        }

        public async Task StartServer(string ip,int port) {
            _server=new NetServer(ip,port);
            _server.StartServer();
            ScreenCaptureThread screen_capture_thread = new ScreenCaptureThread();
            screen_capture_thread.OnFrameReady+=(sender,frame_bytes) => {
                logger.Information("Captured "+frame_bytes.Length+" bytes.");
                Bitmap image_rendered = ScreenCaptureWPFRenderer.Render(frame_bytes).ToBitmap();
                if(previous_image!=null) {
                    Bitmap image_diff = ScreenInterpolator.GetImageDifference(previous_image,image_rendered,Color.Purple);
                    byte[] image_diff_array = image_diff.ToByteArray();
                    logger.Information("Image diff weights (before compression): "+image_diff_array.Length+" bytes.");
                    byte[] compressed = LZ4Compressor.Compress(image_diff_array);
                    logger.Information("Image diff weights (after compression): "+compressed.Length+" bytes.");
                    Dispatcher.Invoke(() => ImgCanvas.Source=image_diff.ToBitmapImage());
                }
                previous_image=image_rendered;

                _server.sendToAll_bytes(frame_bytes);
            };
            screen_capture_thread.StartCapture();

            await Task.FromResult<object>(null);
        }
    }
}
