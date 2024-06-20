using FFMpegCore.Arguments;
using FFMpegCore.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClipChopper.WinUIApp
{
    internal class CropArgument : IVideoFilterArgument
    {
        public readonly int width;
        public readonly int height;
        public readonly int x;
        public readonly int y;

        public string Key { get; } = "crop";


        public string Value
        {
            get
            {
                return $"{width}:{height}:{x}:{y}";
            }
        }

        public CropArgument(int width, int height, int x, int y)
        {
            this.width = width;
            this.height = height;
            this.x = x;
            this.y = y;
        }
    }
}
