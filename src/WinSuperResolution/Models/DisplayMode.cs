namespace WinSuperResolution.Models
{
    public sealed class DisplayMode
    {
        public string DeviceName { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int Frequency { get; set; }
        public int BitsPerPixel { get; set; }

        public string DisplayText
        {
            get
            {
                string frequency = Frequency > 1 ? " @ " + Frequency + " Hz" : string.Empty;
                return Width + " x " + Height + frequency;
            }
        }
    }
}
