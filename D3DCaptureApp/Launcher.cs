using System;

namespace D3DCaptureApp {
    /// <summary>
    /// Custom launcher for WPF application. It might be useful in case there is need to do
    /// some initialization before launching WPF window or taking care of command-line arguments.
    /// </summary>
    class Launcher {
        /// <summary>
        /// The entry point of our application.
        /// 
        /// Notice that in order to execute main we marked it with [STAThread] in codebase as a requirement to communicate
        /// with COM components (ref: https://stackoverflow.com/a/1361048).
        /// 
        /// Notice: in order to replace WPF Main(), do this (ref: https://stackoverflow.com/a/23361831):
        /// 1. Solution Explorer -> right-click your project -> Properties -> Application tab -> Startup object (Select this class)
        /// 2. Solution Explorer -> right click on "App.xaml" -> Properties -> Build Action (Set "Page")
        /// </summary>
        /// <param name="args">The command-line arguments.</param>
        [STAThread]
        public static void Main(string[] args) {
            var app = new App();
            app.InitializeComponent();
            app.Run();
        }
    }
}
