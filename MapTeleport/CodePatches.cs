using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FlowAnalysis;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;

namespace MapTeleport
{
    public partial class ModEntry
    {
        /* notes to myself before starting
         * 
         * GameStateQuery.CheckConditions(WorldMapTooltipData)
         * 
         * Content/Data/WorldMap.json is where you will find data about name/id, condition, knowncondition, etc
         * 
         * Game1.warpFarmer(string locationName, int tileX, int tileY, bool flip
         * 
        */

        [HarmonyPatch(typeof(MapPage), nameof(MapPage.receiveLeftClick))]
        public class MapPage_receiveLeftClick_Patch
        {
            public static void Postfix(MapPage __instance, int x, int y)
            {
                foreach (ClickableComponent c in __instance.points.Values)
                {
                    if (c.containsPoint(x, y))
                    {
                        if (Config.Simulate) TestWarp(c.name);
                        else TryWarp(c.name);
                    }
                }

                //return true;
            }
        }

    }
}
