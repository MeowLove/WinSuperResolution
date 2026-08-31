namespace WinSuperResolution.Models
{
    public sealed class DisplayMode
    {
        public string DeviceName { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int Frequency { get; set; }
        public int BitsPerPixel { get; set; }
        public bool IsVirtualDesktopMode { get; set; }
        public bool IsCurrent { get; set; }
        public string ModeKindText { get; set; }

        public string DisplayText
        {
            get
            {
                string frequency = Frequency > 1 ? " @ " + Frequency + " Hz" : string.Empty;
                string kind = string.IsNullOrEmpty(ModeKindText) ? string.Empty : " - " + ModeKindText;
                return Width + " x " + Height + frequency + kind;
            }
        }
    }
}
