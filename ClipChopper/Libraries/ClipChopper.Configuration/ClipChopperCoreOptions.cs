namespace ClipChopper.Configuration
{
    public sealed class ClipChopperCoreOptions : IOptions
    {
        public string ExifToolFileName { get; set; } = "exiftool.exe";


        public ClipChopperCoreOptions()
        {
        }
    }
}
