namespace ClipChopper.DesktopApp
{
    public sealed class AudioTrack
    {
        public static readonly AudioTrack NoAudio = new()
        {
            Name = "No Audio",
            StreamIndex = 0
        };

        public string Name { get; set; } = string.Empty;
        public int StreamIndex { get; set; }


        public AudioTrack()
        {
        }
    }
}