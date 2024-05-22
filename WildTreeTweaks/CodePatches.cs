using StardewValley;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Object = StardewValley.Object;
using StardewValley.TerrainFeatures;
using StardewValley.GameData.WildTrees;
using StardewValley.Extensions;

namespace WildTreeTweaks
{
    public partial class ModEntry
    {

        /// <summary>
        /// Tree IDs:
        /// 1 - Oak
        /// 2 - Maple
        /// 3 - Pine
        /// 6 - Palm tree
        /// 7 - Mushroom tree
        /// 8 - Mahogany
        /// 9 - Palm tree 2 (Ginger Island variant)
        /// 10 - Wild Oak Tree/Green Rain Oak
        /// 11 - Wild Maple Tree/Green Rain Maple
        /// 12 - Wild Pine Tree/Green Rain Pine
        /// 13 - Mystic Tree
        /// </summary>
        
        [HarmonyPatch(typeof(Tree))]
        [HarmonyPatch(MethodType.Constructor, new Type[] { })]
        public class Tree__Patch1
        {
            public static void Postfix(Tree __instance)
            {
                if (!Config.EnableMod) return;
                __instance.health.Value = Config.Health;
            }

        }

        [HarmonyPatch(typeof(Tree), new Type[] {typeof(string), typeof(int), typeof(bool)})]
        [HarmonyPatch(MethodType.Constructor)]
        public class Tree__Patch2
        {
            public static void Postfix(Tree __instance, string id, int growthStage, bool isGreenRainTemporaryTree)
            {
                if (!Config.EnableMod || isGreenRainTemporaryTree) return;
                __instance.health.Value = Config.Health;
            }
        }

        [HarmonyPatch(typeof(Tree), new Type[] {typeof(string)})]
        [HarmonyPatch(MethodType.Constructor)]
        public class Tree__Patch3
        {
            public static void Postfix(Tree __instance, string id)
            {
                if (!Config.EnableMod) return;
            }
        }

        [HarmonyPatch(typeof(Object), nameof(Object.placementAction))]
        public class Object_placementAction_Patch
        {
            public static bool Prefix(Object __instance, GameLocation location, int x, int y, ref bool __result)
            {

                // TileIndexProperties((Type: Dirt), (Water: T)) for beach farm beach water

                if (!Config.EnableMod || !__instance.IsWildTreeSapling() || (!location.IsFarm && Config.OnlyOnFarm)) return true;

                Vector2 placementTile = new Vector2(x / 64, y / 64);
                
                if (!canPlaceWildTreeSeed(__instance, location, placementTile, out var deniedMessage))
                {
                    deniedMessage ??= Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.13021");
                    Game1.showRedMessage(deniedMessage);
                    __result = false;
                    return false;
                }

                string treeType = Tree.ResolveTreeTypeFromSeed(__instance.QualifiedItemId);
                if (treeType != null)
                {
                    Game1.stats.Increment("wildtreesplanted");
                    location.terrainFeatures.Remove(placementTile);
                    location.terrainFeatures.Add(placementTile, new Tree(treeType, 0));
                    location.playSound("dirtyHit");
                    __result = true;
                    return false;
                }

                return true;

            }
        }

        [HarmonyPatch(typeof(Object), nameof(Object.canBePlacedHere))]
        public class Object_canBePlacedHere_Patch
        {
            public static void Postfix(Object __instance, GameLocation l, Vector2 tile, ref bool __result, bool showError = false)
            {
                if (!Config.EnableMod || !Object.isWildTreeSeed(__instance.ItemId) || __result || (!l.IsFarm && Config.OnlyOnFarm) || (!l.IsOutdoors && !l.treatAsOutdoors.Value))
                    return;

                if (!canPlaceWildTreeSeed(__instance, l, tile, out var deniedMessage)) return;
                if (!l.isTileOnMap(tile)) return;
                if (l.GetHoeDirtAtTile(tile)?.crop is not null) return;

                __result = true;
            }
        }

        [HarmonyPatch(typeof(Tree), nameof(Tree.TryGetData))]
        public class Tree_TryGetData_Patch
        {
            public static bool Prefix(string id, out WildTreeData data, ref bool __result)
            {
                if (!Config.EnableMod || id is null)
                {
                    Log("TryGetData: mod disabled or id is null.", debugOnly: true);
                    data = null;
                    return true;
                }

                if (!(Tree.GetWildTreeDataDictionary().TryGetValue(id, out data))) return true;

                data.GrowthChance = Config.GrowthChance; // 1f = always true, called in Tree.dayUpdate()
                data.GrowsInWinter = Config.GrowInWinter; // called in dayUpdate()
                data.IsStumpDuringWinter = Config.GrowInWinter; // called in dayUpdate()
                data.SeedOnShakeChance = Config.SeedChance; // 1f = always true, called in Tree.dayUpdate()
                data.SeedSpreadChance = Config.SeedSpreadChance; // 1f = always true, called in Tree.dayUpdate()
                float difChance = (data.SeedOnShakeChance - Config.SeedChance) * 10f; // seed chop scales with seed shake. lowest possible val for seed chop = 0.25 = 25% chance
                data.SeedOnChopChance = (difChance + data.SeedOnChopChance) > 1f ? 1f : data.SeedOnChopChance + difChance;

                List<WildTreeChopItemData> chopItems = data.ChopItems;

                __result = true;
                return false;
                //data.SeedOnChopChance = Config.SeedChance * 15f;
            }
        }
    }
}
