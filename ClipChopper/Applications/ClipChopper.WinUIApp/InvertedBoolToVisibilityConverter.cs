using CommunityToolkit.WinUI.Converters;
using Microsoft.UI.Xaml;

namespace ClipChopper.WinUIApp
{
    internal class InvertedBoolToVisibilityConverter : BoolToObjectConverter
    {
        public InvertedBoolToVisibilityConverter()
        {
            TrueValue = Visibility.Collapsed;
            FalseValue = Visibility.Visible;
        }
    }
}
