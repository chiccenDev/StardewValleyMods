using HarmonyLib;
using StardewValley;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoreRings
{
    public partial class ModEntry
    {

        /// <summary>
        /// Replacement for <see cref="ModEntry.OnItemEaten"/> to avoid limit prequisite APIs
        /// </summary>
        [HarmonyPatch(typeof(Farmer), nameof(Farmer.doneEating))]
        public class Farmer_doneEating_Patch
        {
            public static void Postfix()
            {
                if (HasRingEquipped("Ring_of_Diamond_Booze"))
                {
                    if (Game1.player.hasBuff(Buff.tipsy))
                    {
                        Game1.player.buffs.Remove(Buff.tipsy);
                    }
                }
            }
        }
        /// <summary>
        /// Replacement for AxePatcher.Before_DoFunction because I am just not familiar with that Harmony Patch formatting lol
        /// </summary>
        [HarmonyPatch(typeof(Axe), nameof(Axe.DoFunction))]
        public class Axe_DoFunction_Patch
        {
            public static bool Prefix(ref int x, ref int y, Farmer who)
            {
                if (HasRingEquipped("Ring_of_Far_Reaching"))
                {
                    x = (int)who.lastClick.X;
                    y = (int)who.lastClick.Y;
                }
                return true;
            }
        }
    }
}
