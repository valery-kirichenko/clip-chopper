using System;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Markup;
using ClipChopper.Common;
using ClipChopper.Logging;
using Unosquare.FFME;

namespace ClipChopper
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Logger instance for current class.
        /// </summary>
        private static readonly ILogger _logger = LoggerFactory.CreateLoggerFor<App>();


        public App()
        {
            // Set current culture for app globally.
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            FrameworkElement.LanguageProperty.OverrideMetadata(
                typeof(FrameworkElement),
                new FrameworkPropertyMetadata(
                    XmlLanguage.GetLanguage(CultureInfo.InvariantCulture.Name)
                )
            );

            UnhandledExceptionEventRegistrator.Register(Application_OnUnhandledException);

            _logger.PrintHeader("Desktop client application started.");

            Library.FFmpegDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ffmpeg");
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            _logger.PrintFooter("Desktop client application stopped.");
        }

        private void Application_OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = (Exception) e.ExceptionObject;
            _logger.Error(ex, "Unhandled exception has been occurred.");
        }
    }
}
