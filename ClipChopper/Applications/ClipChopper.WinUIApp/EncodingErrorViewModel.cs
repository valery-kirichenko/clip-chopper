using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClipChopper.WinUIApp
{
    internal class EncodingErrorViewModel
    {
        public string ErrorText { get; set; } = "Unknown error occurred when running FFmpeg";
        public string? Log { get; set; }
    }
}
