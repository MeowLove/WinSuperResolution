namespace WinSuperResolution.Models
{
    public sealed class LiveDisplayInfo
    {
        public string DeviceName { get; set; }
        public string FriendlyName { get; set; }
        public string AdapterName { get; set; }
        public string MonitorDeviceId { get; set; }
        public string MonitorDeviceKey { get; set; }
        public string ConnectionTechnology { get; set; }
        public string TopologyEvidence { get; set; }
        public int CurrentWidth { get; set; }
        public int CurrentHeight { get; set; }
        public int CurrentScalePercent { get; set; }
        public bool IsAttachedToDesktop { get; set; }

        public string CurrentModeText
        {
            get { return CurrentWidth > 0 && CurrentHeight > 0 ? CurrentWidth + " x " + CurrentHeight : "Unavailable"; }
        }

        public string ScaleText
        {
            get { return CurrentScalePercent > 0 ? CurrentScalePercent + "%" : "Unavailable"; }
        }
    }
}
