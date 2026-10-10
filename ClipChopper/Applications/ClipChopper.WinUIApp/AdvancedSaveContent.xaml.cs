using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ClipChopper.WinUIApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AdvancedSaveContent : Page
    {
        public AdvancedSaveContent()
        {
            this.InitializeComponent();
            this.ContentFrame.Navigate(typeof(AdvancedSaveVideoPage));
        }

        private void NavigationView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            var item = args.InvokedItemContainer;
            switch (item.Tag)
            {
                case "AdvancedSaveVideoPage":
                    ContentFrame.Navigate(typeof(AdvancedSaveVideoPage));
                    break;
                case "AdvancedSaveAudioPage":
                    ContentFrame.Navigate(typeof(AdvancedSaveAudioPage));
                    break;
            }
        }
    }
}
