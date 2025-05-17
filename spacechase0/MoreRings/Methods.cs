using Microsoft.Xna.Framework;
using StardewValley;
using Object = StardewValley.Object;

namespace MoreRings
{
    public partial class ModEntry
    {

        private static Item? LastItem;

        public static int CountRingsEquipped(string id)
        {
            int count = (Game1.player.leftRing.Value?.GetEffectsOfRingMultiplier(id) ?? 0) + (Game1.player.rightRing.Value?.GetEffectsOfRingMultiplier(id) ?? 0);

            return count;
        }

        public static bool HasRingEquipped(string id)
        {
            return CountRingsEquipped(id) > 0;
        }

        public static Debris Game1_createItemDebris(Item item, Vector2 origin, int direction, GameLocation location, int groundLevel)
        {
            ModifyCropQuality(item);

            return Game1.createItemDebris(item, origin, direction, location, groundLevel);
        }

        public static bool Farmer_addItemToInventoryBool(Farmer farmer, Item item, bool makeActiveObject)
        {
            ModifyCropQuality(item);

            return farmer.addItemToInventoryBool(item, makeActiveObject);
        }

        private static void ModifyCropQuality(Item item)
        {
            if (item is not Object obj || object.ReferenceEquals(item, LastItem)) return;

            LastItem = item;
            if (Game1.random.NextDouble() < CountRingsEquipped("Quality+_Ring") * Config.QualityRing_ChancePerRing)
            {
                obj.Quality = obj.Quality switch
                {
                    Object.lowQuality => Object.medQuality,
                    Object.medQuality => Object.highQuality,
                    Object.highQuality => Object.bestQuality,
                    _ => obj.Quality
                };
            }
        }
    }
}
