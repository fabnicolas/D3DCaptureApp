using System.Drawing;
using System.Drawing.Imaging;

namespace D3DCaptureApp {
    class ScreenInterpolator {
        public static unsafe Bitmap GetImageDifference(Bitmap image1,Bitmap image2,Color match_color) {
            if(image1==null||image2==null||image1.Height!=image2.Height||image1.Width!=image2.Width)
                return null;

            Bitmap imageD = image2.Clone() as Bitmap;

            // Since images are equals at this point, any image can do (Use bounds of first image)
            int height = image1.Height;
            int width = image1.Width;
            Rectangle bounds = new Rectangle(0,0,width,height);

            // Locking bits
            BitmapData data1 = image1.LockBits(bounds,ImageLockMode.ReadOnly,PixelFormat.Format24bppRgb);
            BitmapData data2 = image2.LockBits(bounds,ImageLockMode.ReadOnly,PixelFormat.Format24bppRgb);
            BitmapData dataD = imageD.LockBits(bounds,ImageLockMode.WriteOnly,PixelFormat.Format24bppRgb);

            // Manipulation of images is done by using pointers, which is very fast
            byte* p_data1 = (byte*)data1.Scan0;
            byte* p_data2 = (byte*)data2.Scan0;
            byte* p_dataD = (byte*)dataD.Scan0;

            // Converting Color object to byte[3]
            byte[] swap_color = new byte[3];
            swap_color[0]=match_color.B;
            swap_color[1]=match_color.G;
            swap_color[2]=match_color.R;

            // Calculate padding to skip at every end of a width scan
            int row_padding = data1.Stride-(image1.Width*3);

            for(int i = 0;i<height;i++) {
                for(int j = 0;j<width;j++) {
                    int equal_RGB_components = 0;   // If this value reachs 3, pixels on a specific position are equal.

                    byte[] tmp = new byte[3];

                    // Compare pixel by each component (R,G,B) and store in tmp
                    for(int x = 0;x<3;x++) {
                        tmp[x]=p_data2[0];
                        if(p_data1[0]==p_data2[0])
                            equal_RGB_components++;
                        p_data1++; // Advance first image pointer.
                        p_data2++; // advance second image pointer.
                    }

                    // Change color of the pixel if pixels at same positions of two images are equal; otherwise add new values
                    for(int x = 0;x<3;x++) {
                        p_dataD[0]=(equal_RGB_components==3) ? swap_color[x] : tmp[x];
                        p_dataD++; // Advance write pointer.
                    }
                }

                // For each column, at its end, skip padding for all 3 pointers.
                if(row_padding>0) {
                    p_data1+=row_padding;
                    p_data2+=row_padding;
                    p_dataD+=row_padding;
                }
            }

            // Unlocking bits
            image1.UnlockBits(data1);
            image2.UnlockBits(data2);
            imageD.UnlockBits(dataD);

            return imageD;
        }

        
    }
}
