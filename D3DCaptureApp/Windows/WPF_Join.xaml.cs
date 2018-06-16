using Serilog.Core;
using SerilogLoggerSystem;
using System.Threading.Tasks;
using System.Windows;

namespace D3DCaptureApp {
    public partial class WPF_Join:Window {
        private readonly Logger logger = SerilogFactory.GetLogger();

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
            logger.Information("Capture started.");
            _client.OnServerMessage((frame_bytes) => {
                logger.Information("Sending rendering request with "+frame_bytes.Length+" bytes as frame...");
                Dispatcher.Invoke(() => ImgCanvas.Source=ScreenCaptureWPFRenderer.Render(frame_bytes));
            });
        }
    }
}
