using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoreRings
{
    public partial class ModEntry
    {
        public static int CountRingsEquipped(string id)
        {
            int count = (Game1.player.leftRing.Value?.GetEffectsOfRingMultiplier(id) ?? 0) + (Game1.player.rightRing.Value?.GetEffectsOfRingMultiplier(id) ?? 0);

            return count;
        }

        public static bool HasRingEquipped(string id)
        {
            return CountRingsEquipped(id) > 0;
        }
    }
}
