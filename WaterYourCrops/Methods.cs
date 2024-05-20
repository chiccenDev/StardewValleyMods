using Microsoft.Xna.Framework;
using StardewValley;

namespace WaterYourCrops
{
    partial class ModEntry
    {
        public static bool HasCan()
        {
            Item item = Game1.player.CurrentItem;
            if (item is not null)
            {
                return (item.Name.Contains("Watering Can"));
            }
            return false;
        }
    }
}
