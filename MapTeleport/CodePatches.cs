using StardewValley.Menus;
using HarmonyLib;

namespace MapTeleport
{
    public partial class ModEntry
    {

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
                        success = Config.Simulate ? TestWarp(c.name) : TryWarp(c.name);

                        if (success) __instance.exitThisMenu();
                        
                    }
                }

                //return true;
            }
        }

    }
}
