using Serilog.Core;
using SerilogLoggerSystem;
using System.Drawing;
using System.Threading.Tasks;

namespace D3DCaptureApp.Impl {
    class ScreenServer {
        private static readonly Logger logger = SerilogFactory.GetLogger();
        NetServer _server;
        byte[] previous_frame_bytes;

        public ScreenServer() {
            previous_frame_bytes=null;
        }

        public async Task StartServer(string ip,int port) {
            _server=new NetServer(ip,port);
            _server.StartServer();
            ScreenCaptureThread screen_capture_thread = new ScreenCaptureThread();
            screen_capture_thread.OnFrameReady+=(sender,frame_bytes) => {
                logger.Information("Captured "+frame_bytes.Length+" bytes.");

                if(previous_frame_bytes!=null) {
                    // If it's NOT the first frame, then send the differences between frames.
                    Bitmap image_diff = ScreenInterpolator.GetDeltaImage(
                        ScreenCaptureWPFRenderer.Render(previous_frame_bytes).ToBitmap(),
                        ScreenCaptureWPFRenderer.Render(frame_bytes).ToBitmap(),
                        Color.Purple
                    );
                    previous_frame_bytes=frame_bytes;
                    frame_bytes=image_diff.ToByteArray();   // Instead of sending frame - send differences.
                } else {
                    // If it's the first frame, just send it.
                    previous_frame_bytes=frame_bytes;
                }

                // Send frame/differences to clients.
                _server.SendBytesBroadcast(frame_bytes);
            };
            screen_capture_thread.StartCapture();

            await Task.FromResult<object>(null);
        }
    }
}
