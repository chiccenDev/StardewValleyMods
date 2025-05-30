using StardewModdingAPI;

namespace Skateboard
{
    public class ModConfig
    {
        public bool ModEnabled { get; set; } = true;
        public SButton RideButton { get; set; } = SButton.MouseRight;
        public float MaxSpeed { get; set; } = 5;
        public float Acceleration { get; set; } = 2;
        public float Deceleration { get; set; } = 0.5f;
        public string CraftingRequirements { get; set; } = "709 4 337 1 335 1 338 1 92 10";
    }
}