using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;

namespace D3DCaptureApp {
    public class ScreenCaptureThread {
        private bool _run, _init;

        public int Size { get; private set; }
        public ScreenCaptureThread() { }

        public void Start() {
            _run=true;
            var factory = new Factory1();   // SharpDX DXGI Factory
            var adapter = factory.GetAdapter1(0); // Get first video card (Adapter) 
            var device = new SharpDX.Direct3D11.Device(adapter); // Get D3D11 device from adapter

            // Get frontal output buffer of the adapter
            var output = adapter.GetOutput(0);
            var output1 = output.QueryInterface<Output1>();

            // Get video card width/height of the desktop to capture (The resolution of your screen)
            int screen_width = output.Description.DesktopBounds.Right;
            int screen_height = output.Description.DesktopBounds.Bottom;

            // Build a 2D texture to use as staging texture that is accessible by the CPU
            var texture_description = new Texture2DDescription {
                CpuAccessFlags=CpuAccessFlags.Read,
                BindFlags=BindFlags.None,
                Format=Format.B8G8R8A8_UNorm,
                Width=screen_width,
                Height=screen_height,
                OptionFlags=ResourceOptionFlags.None,
                MipLevels=1,
                ArraySize=1,
                SampleDescription={ Count=1,Quality=0 },
                Usage=ResourceUsage.Staging
            };
            var texture_screen = new Texture2D(device,texture_description);

            Task.Factory.StartNew(() => {
                using(var output_clone = output1.DuplicateOutput(device)) {
                    while(_run) {
                        try {
                            SharpDX.DXGI.Resource output_frame_resource;
                            OutputDuplicateFrameInformation output_frame_info;

                            // Try to get duplicated frame within given time is ms
                            output_clone.AcquireNextFrame(5,out output_frame_info,out output_frame_resource);

                            // Copy resource into memory that can be accessed by the CPU
                            using(var texture_screen_acquired = output_frame_resource.QueryInterface<Texture2D>())
                                device.ImmediateContext.CopyResource(texture_screen_acquired,texture_screen);

                            // Get the desktop capture texture
                            var map_src = device.ImmediateContext.MapSubresource(texture_screen,0,MapMode.Read,SharpDX.Direct3D11.MapFlags.None);

                            // Create Drawing.Bitmap
                            using(var bitmap_screen = new Bitmap(screen_width,screen_height,PixelFormat.Format32bppArgb)) {
                                var screen_capture_rect = new Rectangle(0,0,screen_width,screen_height);

                                // Copy pixels from screen capture Texture to GDI bitmap
                                var map_dest = bitmap_screen.LockBits(
                                    screen_capture_rect,
                                    ImageLockMode.WriteOnly,
                                    bitmap_screen.PixelFormat
                                );
                                var pointer_src = map_src.DataPointer;
                                var pointer_dest = map_dest.Scan0;
                                for(int y = 0;y<screen_height;y++) {
                                    Utilities.CopyMemory(pointer_dest,pointer_src,screen_width*4); // Copy a single line (4 bytes per pixel).

                                    // Advance pointers
                                    pointer_src=IntPtr.Add(pointer_src,map_src.RowPitch);
                                    pointer_dest=IntPtr.Add(pointer_dest,map_dest.Stride);
                                }

                                // Release source and destination locks
                                bitmap_screen.UnlockBits(map_dest);
                                device.ImmediateContext.UnmapSubresource(texture_screen,0);

                                using(var ms = new MemoryStream()) {
                                    bitmap_screen.Save(ms,ImageFormat.Bmp); // Save bitmap pixels in a stream
                                    onFrameReady?.Invoke(this,ms.ToArray()); // Return bitmap pixels from a stream to the caller
                                    _init=true;
                                }
                            }
                            output_frame_resource.Dispose();
                            output_clone.ReleaseFrame();
                        } catch(SharpDXException e) {
                            if(e.ResultCode.Code!=SharpDX.DXGI.ResultCode.WaitTimeout.Result.Code) {
                                Trace.TraceError(e.Message);
                                Trace.TraceError(e.StackTrace);
                            }
                        }
                    }
                }
            });
            while(!_init)
                System.Threading.Thread.Sleep(100);
        }

        public void Stop() {
            _run=false;
        }

        public EventHandler<byte[]> onFrameReady;
    }
}