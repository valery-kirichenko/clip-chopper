namespace ClipChopper.WinUIApp.Automation
{
    /// <summary>
    /// The text that the app shows in the ItemStatus of the volume slider in test mode.
    /// The text tells whether the player is silent. It is silent when its volume is 0 or when it is muted.
    /// </summary>
    public static class MuteStatusFormat
    {
        public const string Muted = "Muted";

        public const string NotMuted = "NotMuted";

        public static string Format(bool isMuted)
        {
            return isMuted ? Muted : NotMuted;
        }
    }
}
