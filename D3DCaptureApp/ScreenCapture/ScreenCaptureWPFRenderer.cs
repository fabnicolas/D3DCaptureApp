using System.IO;
using System.Windows.Media.Imaging;

namespace D3DCaptureApp {
    class ScreenCaptureWPFRenderer {
        public ScreenCaptureWPFRenderer() { }

        public BitmapImage render(byte[] data_image) {
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
