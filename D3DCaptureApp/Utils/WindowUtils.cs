using System;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;

namespace D3DCaptureApp.Utils
{
    class WindowUtils
    {
        /// <summary>
        /// Centers a given Window.
        /// This is an advanced centering as it takes care of:
        /// - Your current monitor;
        /// - Your working area;
        /// - DPI scaling.
        /// 
        /// For more informations, check reference (ref: https://stackoverflow.com/a/32599760).
        /// </summary>
        /// <param name="window">The window to center.</param>
        public static void CenterWindow(Window window) {
            double myWindowWidth = window.Width;
            double myWindowHeight = window.Height;

            // Get current monitor data
            Screen currentMonitor = Screen.FromHandle(new System.Windows.Interop.WindowInteropHelper(window).Handle);

            // Get DPI scaling in case monitor scales our application
            PresentationSource source = PresentationSource.FromVisual(window);
            double dpiScaling = (source!=null && source.CompositionTarget!=null)
                ? source.CompositionTarget.TransformFromDevice.M11
                : 1;
            
            // Get monitor available area
            Rectangle workArea = currentMonitor.WorkingArea;
            var workAreaWidth = (int)Math.Floor(workArea.Width*dpiScaling);
            var workAreaHeight = (int)Math.Floor(workArea.Height*dpiScaling);

            // Center window
            window.Left=(((workAreaWidth-(window.Width*dpiScaling))/2)+(workArea.Left*dpiScaling));
            window.Top=(((workAreaHeight-(window.Height*dpiScaling))/2)+(workArea.Top*dpiScaling));
        }
    }
}
