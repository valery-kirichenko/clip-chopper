using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ClipChopper.WinUIApp
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            var themeName = GetLaunchOption("--theme");
            if (themeName != null
                && Enum.TryParse(themeName, ignoreCase: true, out ApplicationTheme theme)
                && Enum.IsDefined(theme))
            {
                RequestedTheme = theme;
            }
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            var window = new MainWindow();
            m_window = window;
            window.Activate();

            var path = GetLaunchOption("--open");
            if (path != null && File.Exists(path))
            {
                window.OpenFile(path);
            }
        }

        /// <summary>
        /// Returns the value after the given option in the command line, or null.
        /// The options "--theme" and "--open" are for UI tests. Without them the app starts as before.
        /// </summary>
        private static string? GetLaunchOption(string name)
        {
            var args = Environment.GetCommandLineArgs();
            var index = Array.IndexOf(args, name);
            return index > 0 && index + 1 < args.Length ? args[index + 1] : null;
        }

        private Window m_window;
    }
}
