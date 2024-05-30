using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace WaterYourCrops
{
    partial class ModEntry
    {
        public static bool HasCan()
        {
            Item item = Game1.player.CurrentItem;
            if (item is not null)
            {
                //Log($"{Game1.player} has a watering can", debugOnly: true);
                return (item.Name.Contains("Watering Can"));
            }
            //Log($"{Game1.player} does not have a watering can", debugOnly: true);
            return false;
        }

        public static void DrawWater()
        {
            if (!Config.EnableMod) return;
            GameLocation location = Game1.getFarm();
            foreach (TerrainFeature f in location.terrainFeatures.Values)
            {
                if (f is null || f is not HoeDirt || f is HoeDirt { crop: null }) continue;
                HoeDirt h = f as HoeDirt;

            }
        }
    }
}
