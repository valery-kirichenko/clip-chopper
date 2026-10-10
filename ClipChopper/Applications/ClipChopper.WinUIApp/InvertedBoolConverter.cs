using CommunityToolkit.WinUI.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClipChopper.WinUIApp
{
    internal class InvertedBoolConverter : BoolToObjectConverter
    {
        public InvertedBoolConverter() {
            TrueValue = false;
            FalseValue = true;
        }
    }
}
