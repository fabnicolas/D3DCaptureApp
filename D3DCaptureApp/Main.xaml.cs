using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace D3DCaptureApp {
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1:Window {
        private int _collapsed_height;

        public Window1() {
            InitializeComponent();
            _collapsed_height=0;
        }

        private void listener_button_capture_test(object sender,RoutedEventArgs e) {
            CaptureWindow w = new CaptureWindow();
            w.Show();
            this.Close();
        }

        private void listener_button_host(object sender,RoutedEventArgs e) {
            expand(100);
        }

        private void listener_button_join(object sender,RoutedEventArgs e) {
            expand(50);
        }

        private void collapse() {
            if(_collapsed_height>0) {
                Height=Height-_collapsed_height;
                _collapsed_height=0;
            }
        }

        private void expand(int height) {
            Height=Height-_collapsed_height+height;
            _collapsed_height=height;
        }
    }
}
