using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace WaterYourCrops
{
    partial class ModEntry
    {
        public static bool HasCan()
        {
            Item? item = Game1.player?.CurrentItem;
            if (item is not null)
            {
                //Log($"{Game1.player} has a watering can", debugOnly: true);
                return (item.Name.Contains("Watering Can"));
            }
            //Log($"{Game1.player} does not have a watering can", debugOnly: true);
            return false;
        }
    }
}
