using StardewModdingAPI;

namespace MapTeleport
{
    public class ModConfig
    {

        public bool EnableMod { get; set; } = true;
        public bool AllowUnknown { get; set; } = false;
        public bool AutoFarmDoor { get; set; } = true;
        public bool EnableAudio { get; set; } = true;
        public SButton SaveHotkey { get; set; } = SButton.F2;
        public bool Debug { get; set; } = false;

    }
}
