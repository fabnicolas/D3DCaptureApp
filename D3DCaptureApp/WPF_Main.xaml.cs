using System.Windows;

namespace D3DCaptureApp {
    public partial class WPF_Main:Window {
        private int _collapsed_height;
        private byte _mode = 0;

        public WPF_Main() {
            InitializeComponent();
            _collapsed_height=0;
        }

        private void listener_button_capture_test(object sender,RoutedEventArgs e) {
            WPF_CaptureScreen w = new WPF_CaptureScreen();
            w.Show();
            this.Close();
        }

        private void listener_button_show_host(object sender,RoutedEventArgs e) {
            _mode=0;
            expand(100);
        }

        private void listener_button_show_join(object sender,RoutedEventArgs e) {
            _mode=1;
            expand(100);
        }

        private async void listener_button_start(object sender,RoutedEventArgs e) {
            if (_mode==0) {
                WPF_Host w = new WPF_Host();
                w.Show();
                await w.StartServer(textbox_ip.Text,int.Parse(textbox_port.Text));
            } else if(_mode==1) {
                WPF_Join w = new WPF_Join();
                w.Show();
                await w.StartClient(textbox_ip.Text, int.Parse(textbox_port.Text));
            }
            this.Close();
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
