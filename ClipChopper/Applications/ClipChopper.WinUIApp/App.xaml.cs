using ClipChopper.WinUIApp.CommandLine;
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
            _launchOptions = LaunchOptionsParser.Parse(Environment.GetCommandLineArgs()[1..]);
            if (_launchOptions.Theme is { } theme)
            {
                RequestedTheme = ToApplicationTheme(theme);
            }
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            var window = new MainWindow(_launchOptions);
            m_window = window;
            window.Activate();

            if (_launchOptions.OpenFile is { } path && File.Exists(path))
            {
                window.OpenFile(path);
            }
        }

        private static ApplicationTheme ToApplicationTheme(LaunchTheme theme)
        {
            return theme switch
            {
                LaunchTheme.Light => ApplicationTheme.Light,
                LaunchTheme.Dark => ApplicationTheme.Dark,
                _ => throw new ArgumentOutOfRangeException(nameof(theme), theme, null),
            };
        }

        private readonly LaunchOptions _launchOptions;

        private Window m_window;
    }
}
