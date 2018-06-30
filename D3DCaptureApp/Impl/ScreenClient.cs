using Serilog.Core;
using SerilogLoggerSystem;
using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace D3DCaptureApp.Impl {
    class ScreenClient {
        private readonly Logger logger = SerilogFactory.GetLogger();
        public Action<BitmapImage> OnServerMessage;

        NetClient _client;
        private BitmapImage previous_image;

        public ScreenClient() {
            previous_image=null;
        }

        public async Task StartClient(string ip,int port) {
            _client=new NetClient(ip,port);
            _client.StartClient();
            await Task.Run(() => ReadServerResponse());
        }

        public void ReadServerResponse() {
            logger.Information("Capture started.");
            _client.OnServerMessage((frame_bytes) => {
                logger.Information("Received from server "+frame_bytes.Length+" bytes. Rendering...");

                BitmapImage server_image_data = ScreenCaptureWPFRenderer.Render(frame_bytes);
                if(previous_image!=null) {
                    Bitmap image_reconstructed = ScreenInterpolator.AddDeltaImage(
                        previous_image.ToBitmap(),
                        server_image_data.ToBitmap(),
                        Color.Purple
                    );
                    server_image_data=image_reconstructed.ToBitmapImage();
                }
                previous_image=server_image_data;
                OnServerMessage?.Invoke(server_image_data);
            });
        }
    }
}
