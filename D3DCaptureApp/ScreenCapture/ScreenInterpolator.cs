using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace D3DCaptureApp {
    class ScreenInterpolator {
        public static unsafe Bitmap GetDeltaImage(Bitmap image_source,Bitmap image_tocompare,Color color_filter) {
            if(image_source==null||image_tocompare==null||image_source.Height!=image_tocompare.Height||image_source.Width!=image_tocompare.Width)
                return null;

            Bitmap imageD = image_tocompare.Clone() as Bitmap;

            // Since images are equals at this point, any image can do (Use bounds of first image)
            int height = image_source.Height;
            int width = image_source.Width;
            Rectangle bounds = new Rectangle(0,0,width,height);

            // Locking bits
            BitmapData data1 = image_source.LockBits(bounds,ImageLockMode.ReadOnly,PixelFormat.Format24bppRgb);
            BitmapData data2 = image_tocompare.LockBits(bounds,ImageLockMode.ReadOnly,PixelFormat.Format24bppRgb);
            BitmapData dataD = imageD.LockBits(bounds,ImageLockMode.WriteOnly,PixelFormat.Format24bppRgb);

            // Manipulation of images is done by using pointers, which is very fast
            byte* p_data1 = (byte*)data1.Scan0;
            byte* p_data2 = (byte*)data2.Scan0;
            byte* p_dataD = (byte*)dataD.Scan0;

            // Converting Color object to byte[3]
            byte[] swap_color = new byte[3];
            swap_color[0]=color_filter.B;
            swap_color[1]=color_filter.G;
            swap_color[2]=color_filter.R;

            // Calculate padding to skip at every end of a width scan
            int row_padding = data1.Stride-(image_source.Width*3);

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
            image_source.UnlockBits(data1);
            image_tocompare.UnlockBits(data2);
            imageD.UnlockBits(dataD);

            return imageD;
        }
        public static unsafe Bitmap AddDeltaImage(Bitmap image_source,Bitmap image_delta,Color color_filter) {
            if(image_source==null||image_delta==null||image_source.Height!=image_delta.Height||image_source.Width!=image_delta.Width)
                return null;

            Bitmap imageD = image_delta.Clone() as Bitmap;

            // Since images are equals at this point, any image can do (Use bounds of first image)
            int height = image_source.Height;
            int width = image_source.Width;
            Rectangle bounds = new Rectangle(0,0,width,height);

            // Locking bits
            BitmapData data1 = image_source.LockBits(bounds,ImageLockMode.ReadOnly,PixelFormat.Format24bppRgb);
            BitmapData data2 = image_delta.LockBits(bounds,ImageLockMode.ReadOnly,PixelFormat.Format24bppRgb);
            BitmapData dataD = imageD.LockBits(bounds,ImageLockMode.WriteOnly,PixelFormat.Format24bppRgb);

            // Manipulation of images is done by using pointers, which is very fast
            byte* p_data1 = (byte*)data1.Scan0;
            byte* p_data2 = (byte*)data2.Scan0;
            byte* p_dataD = (byte*)dataD.Scan0;

            // Converting Color object to byte[3]
            byte[] swap_color = new byte[3];
            swap_color[0]=color_filter.B;
            swap_color[1]=color_filter.G;
            swap_color[2]=color_filter.R;

            // Calculate padding to skip at every end of a width scan
            int row_padding = data1.Stride-(image_source.Width*3);

            for(int i = 0;i<height;i++) {
                for(int j = 0;j<width;j++) {
                    int equal_RGB_components = 0;   // If this value reachs 3, pixels on a specific position are equal.

                    byte[] tmp1 = new byte[3];
                    byte[] tmp2 = new byte[3];

                    // Compare pixel by each component (R,G,B) and store in tmp
                    for(int x = 0;x<3;x++) {
                        tmp1[x]=p_data1[0]; // Current component of current pixel in image_source
                        tmp2[x]=p_data2[0]; // ...same... in image_delta
                        if(swap_color[x]==p_data2[0])
                            equal_RGB_components++;
                        p_data1++; // Advance first image pointer.
                        p_data2++; // advance second image pointer.
                    }

                    // Change color of the pixel if pixels at same positions of two images are equal; otherwise add new values
                    for(int x = 0;x<3;x++) {
                        p_dataD[0]=(equal_RGB_components==3) ? tmp1[x] : tmp2[x];
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
            image_source.UnlockBits(data1);
            image_delta.UnlockBits(data2);
            imageD.UnlockBits(dataD);

            return imageD;
        }
        
    }
}
