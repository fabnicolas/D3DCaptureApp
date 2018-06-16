using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;

namespace D3DCaptureApp {
    class ScreenCaptureWPFRenderer {
        public static BitmapImage Render(byte[] data_image) {
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

    static class BitmapExtender {
        public static BitmapImage ToBitmapImage(this Bitmap bitmap) {
            using(MemoryStream stream = new MemoryStream()) {
                bitmap.Save(stream,ImageFormat.Png);
                stream.Position=0;

                BitmapImage bitmap_image = new BitmapImage();

                bitmap_image.BeginInit();
                bitmap_image.StreamSource=stream;
                bitmap_image.CacheOption=BitmapCacheOption.OnLoad;
                bitmap_image.EndInit();
                bitmap_image.Freeze();

                return bitmap_image;
            }
        }

        public static Bitmap ToBitmap(this BitmapImage bitmapImage) {
            using(MemoryStream stream = new MemoryStream()) {
                BitmapEncoder bitmap_encoder = new BmpBitmapEncoder();
                bitmap_encoder.Frames.Add(BitmapFrame.Create(bitmapImage));
                bitmap_encoder.Save(stream);
                Bitmap bitmap = new Bitmap(stream);
                return new Bitmap(bitmap);
            }
        }

        public static byte[] ToByteArray(this Image image) {
            using(MemoryStream stream = new MemoryStream()) {
                image.Save(stream,ImageFormat.Bmp);
                return stream.ToArray();
            }
        }
    }
}
