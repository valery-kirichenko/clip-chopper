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
    public sealed partial class AdvancedSaveAudioPage : Page
    {
        private List<CheckBox> _checkBoxes = new();

        public AdvancedSaveAudioPage()
        {
            this.InitializeComponent();

            var tracks = new List<string>() { "System sounds", "Microphone", "Mix" };

            tracks.ForEach(track =>
            {
                var checkBox = new CheckBox()
                {
                    Content = track,
                    Margin = new Thickness(24, 0, 0, 0)
                };
                checkBox.Checked += Option_Checked;
                checkBox.Unchecked += Option_Unchecked;
                AudioTracksStackPanel.Children.Add(checkBox);
                _checkBoxes.Add(checkBox);
            });
            Loaded += AdvancedSaveAudioPage_Loaded;
        }

        private void AdvancedSaveAudioPage_Loaded(object sender, RoutedEventArgs e)
        {
            SetCheckedState();
        }

        private void Option_Checked(object sender, RoutedEventArgs e)
        {
            SetCheckedState();
        }

        private void Option_Unchecked(object sender, RoutedEventArgs e)
        {
            SetCheckedState();
        }

        private void SelectAll_Checked(object sender, RoutedEventArgs e)
        {
            _checkBoxes.ForEach(checkBox => { checkBox.IsChecked = true; });
        }

        private void SelectAll_Unchecked(object sender, RoutedEventArgs e)
        {
            _checkBoxes.ForEach(checkBox => { checkBox.IsChecked = false; });
        }

        private void SelectAll_Indeterminate(object sender, RoutedEventArgs e)
        {
            // If the SelectAll box is checked (all options are selected),
            // clicking the box will change it to its indeterminate state.
            // Instead, we want to uncheck all the boxes,
            // so we do this programatically. The indeterminate state should
            // only be set programatically, not by the user.
            if (_checkBoxes.TrueForAll(checkBox => checkBox.IsChecked == true))
            {
                // This will cause SelectAll_Unchecked to be executed, so
                // we don't need to uncheck the other boxes here.
                OptionsAllCheckBox.IsChecked = false;
            }
        }

        private void SetCheckedState()
        {
            if (_checkBoxes.TrueForAll(checkBox => checkBox.IsChecked == true))
            {
                OptionsAllCheckBox.IsChecked = true;
            } else if (_checkBoxes.TrueForAll(checkBox => checkBox.IsChecked == false))
            {
                OptionsAllCheckBox.IsChecked = false;
            } else
            {
                OptionsAllCheckBox.IsChecked = null;
            }
        }
    }
}
