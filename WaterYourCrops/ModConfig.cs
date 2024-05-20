using Microsoft.Xna.Framework;

namespace WaterYourCrops
{
    public class ModConfig
    {
        public bool EnableMod { get; set; } = true;
        public bool OnlyWaterCan { get; set; } = true;
        public Color IndicatorColor { get; set; } = Color.White;
        public float IndicatorOpacity { get; set; } = 1f;
        public bool Debug { get; set; } = false;

    }
}
