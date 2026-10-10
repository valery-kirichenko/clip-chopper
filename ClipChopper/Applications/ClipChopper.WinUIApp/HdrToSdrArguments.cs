using FFMpegCore.Arguments;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClipChopper.WinUIApp
{
    internal class FormatArgument : IVideoFilterArgument
    {
        public string Key => "format";

        public string Value { get; }

        public FormatArgument(string format = "p010")
        {
            Value = format;
        }
    }

    internal class HwUploadArgument : IVideoFilterArgument
    {
        public string Key => "";
        public string Value => "hwupload";
    }

    internal class HwDownloadArgument : IVideoFilterArgument
    {
        public string Key => "";
        public string Value => "hwdownload";
    }

    internal class ToneMapOpenCLArgument : IVideoFilterArgument
    {
        public string Key => "tonemap_opencl";
        public string Value { get; }

        public ToneMapOpenCLArgument(string tonemap = "hable", string? param = null, double desat = 0.5, double threshold = 0.2, string range = "pc", string primaries = "bt709", string transfer = "bt709", string matrix = "bt709", string format = "p010")
        {
            Value = $"tonemap={tonemap}" +
                $"{(param != null ? ":param=" + param : "")}" +
                $":desat={desat.ToString(CultureInfo.InvariantCulture)}" +
                $":threshold={threshold.ToString(CultureInfo.InvariantCulture)}" +
                $":format={format}" +
                $":range={range}" +
                $":primaries={primaries}" +
                $":transfer={transfer}" +
                $":matrix={matrix}";
        }
    }
}
