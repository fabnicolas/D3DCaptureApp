using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace D3DCaptureApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            start_capture();
        }

        public void start_capture() {
            var screen_capture_thread = new ScreenCaptureThread();
            screen_capture_thread.onFrameReady+=(sender,frame_bytes) => {
                Dispatcher.BeginInvoke(new ThreadStart(() => renderer(frame_bytes)));
            };
            screen_capture_thread.Start();
        }

        private void renderer(byte[] frame_ms) {
            try {
                ImgCanvas.Source=LoadImage(frame_ms);
                Console.WriteLine("Frame loaded");
            } catch(Exception e) {
                Console.WriteLine("Exception: "+e.Message);
            }
        }

        private static BitmapImage LoadImage(byte[] imageData) {
            if(imageData==null||imageData.Length==0)
                return null;
            var image = new BitmapImage();
            using(var mem = new MemoryStream(imageData)) {
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
