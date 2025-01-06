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
         * c.Tooltips is a List<WorldMapTooltipData> of shit including KnownCondition for GameStateQuery -- !!!
         * 
         * 
         * more notes to myself for when i return from break
         * 
         * gamestatequery gonna b annoying because mappage.cs doesn't have all the same shiz as maparea.cs but its probably fine
         * 
        */

        [HarmonyPatch(typeof(MapPage), nameof(MapPage.receiveLeftClick))]
        public class MapPage_receiveLeftClick_Patch
        {
            public static void Postfix(MapPage __instance, int x, int y)
            {
                bool success;
                foreach (ClickableComponent c in __instance.points.Values)
                {
                    if (c.containsPoint(x, y))
                    {
                        if (Config.Simulate) success = TestWarp(c.name);
                        else success = TryWarp(c.name);

                        if (success) __instance.exitThisMenu();
                        
                    }
                }

                //return true;
            }
        }

    }
}
