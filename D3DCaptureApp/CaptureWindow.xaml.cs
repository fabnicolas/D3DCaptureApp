using System;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;

namespace D3DCaptureApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class CaptureWindow : Window
    {
        public CaptureWindow()
        {
            InitializeComponent();
            start_capture();
        }

        public void start_capture() {
            var screen_capture_thread = new ScreenCaptureThread();
            screen_capture_thread.onFrameReady+=(sender,frame_bytes) => {
                renderer(frame_bytes);
            };
            screen_capture_thread.start();
        }

        private void renderer(byte[] data_image) {
            try {
                BitmapImage image_loaded=load_image(data_image);
                Dispatcher.BeginInvoke(new ThreadStart(() => ImgCanvas.Source=image_loaded));
                Console.WriteLine("Frame loaded");
            } catch(Exception e) {
                Console.WriteLine("Exception: "+e.Message);
            }
        }

        private static BitmapImage load_image(byte[] data_image) {
            if(data_image==null||data_image.Length==0)
                return null;
            var image = new BitmapImage();
            using(var mem = new MemoryStream(data_image)) {
                mem.Position=0;
                image.BeginInit();
                image.CreateOptions=BitmapCreateOptions.PreservePixelFormat;
                image.CacheOption=BitmapCacheOption.OnLoad;
                image.UriSource=null;
                image.StreamSource=mem;
                image.EndInit();
            }
            image.Freeze();
            return image;
        }
    }
}
