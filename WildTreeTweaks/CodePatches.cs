using StardewValley;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Object = StardewValley.Object;
using StardewValley.TerrainFeatures;
using StardewValley.GameData.WildTrees;
using StardewValley.Extensions;
using StardewValley.Tools;
using StardewValley.Locations;
using StardewValley.Constants;
using StardewValley.Enchantments;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using xTile.Tiles;
using StardewModdingAPI;
using System.Reflection.Emit;

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
        
        public static Dictionary<GameLocation, Dictionary<Vector2, List<Leaf>>> leaves = new();

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

        [HarmonyPatch(typeof(Tree), nameof(Tree.IsGrowthBlockedByNearbyTree))]
        public class Tree_IsGrowthBlockedByNearbyTree_Patch
        {
            public static void Postfix(Tree __instance, ref bool __result)
            {
                __result = !(Config.EnableMod && Config.GrowNearTrees);
                return;
            }
        }

        [HarmonyPatch(typeof(Object), nameof(Object.placementAction))]
        public class Object_placementAction_Patch
        {
            public static bool Prefix(Object __instance, GameLocation location, int x, int y, ref bool __result)
            {

                // TileIndexProperties((Type: Dirt), (Water: T)) for beach farm beach water

                if (!Config.EnableMod || !__instance.IsWildTreeSapling() || ((!location.IsFarm || !location.IsGreenhouse) && Config.OnlyOnFarm)) return true;

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
                if (!Config.EnableMod || !Object.isWildTreeSeed(__instance.ItemId) || __result || (!l.IsFarm && Config.OnlyOnFarm) || (!l.IsOutdoors && (!l.treatAsOutdoors.Value && !l.IsGreenhouse)))
                    return;

                if (!canPlaceWildTreeSeed(__instance, l, tile, out var deniedMessage))
                {
                    if (showError && deniedMessage is not null)
                        Game1.showRedMessage(deniedMessage);
                    return;
                }
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
                    data = new WildTreeData();
                    return true;
                }

                if (!(Tree.GetWildTreeDataDictionary().TryGetValue(id, out data))) return true;

                data.GrowthChance = Config.GrowthChance; // 1f = always true, called in Tree.dayUpdate()
                data.GrowsInWinter = Config.GrowInWinter; // called in IsInSeason() => GetMaxSizeHere() => dayUpdate()
                //data.IsStumpDuringWinter = !Config.GrowInWinter; // called in dayUpdate() -- misunderstood. wild trees never become stumps in winter, only decorative town trees
                data.SeedSpreadChance = Config.SeedSpreadChance; // 1f = always true, called in Tree.dayUpdate()
                float difChance = (data.SeedOnShakeChance - Config.SeedChance) * 10f; // seed chop scales with seed shake. lowest possible val for seed chop = 0.25 = 25% chance
                data.SeedOnShakeChance = Config.SeedChance; // 1f = always true, called in Tree.dayUpdate()
                data.SeedOnChopChance = (difChance + data.SeedOnChopChance) > 1f ? 1f : data.SeedOnChopChance + difChance;

                List<WildTreeChopItemData> chopItems = data.ChopItems;

                __result = true;
                return false;
                //data.SeedOnChopChance = Config.SeedChance * 15f;
            }
        }

        [HarmonyPatch(typeof(Tree), nameof(Tree.performToolAction))]
        public class Tree_performToolAction_Patch
        {
            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                if (!Config.EnableMod || Config.MysteryBoxChance == 0.005f) return instructions;
                Log($"Transpiling Tree.performToolAction", LogLevel.Alert);
                int found = 0;

                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count - 4; i++)
                {
                    if (codes[i].opcode == OpCodes.Ldc_R8 && (float)codes[i].operand == 0.005f && codes[i + 1].opcode == OpCodes.Ldnull)
                    {
                        codes[i] = new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ModConfig), nameof(ModConfig.MysteryBoxChance)));
                        Log("Patched MysteryBoxChance!", debugOnly: true);
                        found += 1;
                    }
                } // I should really make the book chance a transpiler as well but I am feeling sooooo lazy... I'll give it a day. If I am still lazy, I'll just make it a post-fix lol

                if (found < 3) Log($"Failed to find {3 - found} performToolAction targets! Please report this on NexusMods and be aware that some mod functions may not work as intended!", LogLevel.Error);
                return codes.AsEnumerable();
            }
        }

        [HarmonyPatch(typeof(Tree), nameof(Tree.tickUpdate))]
        public class Tree_tickUpdate_Patch
        {
            public static bool Prefix(Tree __instance, GameTime time, ref bool __result)
            {
                if (!Config.EnableMod || !__instance.falling.Value || (bool)__instance.destroy.Value || Config.WoodMultiplier == 1f || (__instance.Location.IsFarm && Config.OnlyOnFarm)) return true;

                GameLocation location = __instance.Location;
                __instance.alpha = Math.Min(1f, __instance.alpha + 0.05f);
                Vector2 tileLocation = __instance.Tile;

                if (!leaves.ContainsKey(__instance.Location))
                    leaves.Add(__instance.Location, new Dictionary<Vector2, List<Leaf>>() { { __instance.Tile, new List<Leaf>() } });
                if (!leaves.TryGetValue(__instance.Location, out var dict) || !dict.TryGetValue(__instance.Tile, out var list))
                    dict?.Add(__instance.Tile, new List<Leaf>());

                leaves.TryGetValue(__instance.Location, out Dictionary<Vector2, List<Leaf>> trees);
                trees.TryGetValue(__instance.Tile, out List<Leaf> leafs);

                if (__instance.falling.Value)
                {
                    __instance.shakeRotation += (__instance.shakeLeft.Value ? (0f - __instance.maxShake * __instance.maxShake) : (__instance.maxShake * __instance.maxShake));
                    __instance.maxShake += 0.0015339808f;
                    WildTreeData data = __instance.GetData();
                    if (data != null && Game1.random.NextDouble() < 0.01 && __instance.IsLeafy())
                    {
                        location.localSound("leafrustle");
                    }
                    if ((double)Math.Abs(__instance.shakeRotation) > Math.PI / 2.0)
                    {
                        __instance.falling.Value = false;
                        __instance.maxShake = 0f;
                        if (data != null)
                        {
                            location.localSound("treethud");
                            if (__instance.IsLeafy())
                            {
                                int leavesToAdd = Game1.random.Next(90, 120);
                                for (int j = 0; j < leavesToAdd; j++)
                                {
                                    leafs?.Add(new Leaf(new Vector2(Game1.random.Next((int)(tileLocation.X * 64f), (int)(tileLocation.X * 64f + 192f)) + (__instance.shakeLeft.Value ? (-320) : 256), tileLocation.Y * 64f - 64f), (float)Game1.random.Next(-10, 10) / 100f, Game1.random.Next(4), (float)Game1.random.Next(10, 40) / 10f));
                                }
                            }
                            Random r;
                            if (Game1.IsMultiplayer)
                            {
                                Game1.recentMultiplayerRandom = Utility.CreateRandom((double)tileLocation.X * 1000.0, tileLocation.Y);
                                r = Game1.recentMultiplayerRandom;
                            }
                            else
                            {
                                r = Utility.CreateRandom(Game1.uniqueIDForThisGame, Game1.stats.DaysPlayed, (double)tileLocation.X * 7.0, (double)tileLocation.Y * 11.0);
                            }
                            if (data.DropWoodOnChop)
                            {
                                int numToDrop = (int)((Game1.getFarmer(__instance.lastPlayerToHit.Value).professions.Contains(12) ? 1.25 : 1.0) * (double)((12 + extraWoodCalculator(tileLocation)) * Config.WoodMultiplier));
                                if (Game1.getFarmer(__instance.lastPlayerToHit.Value).stats.Get("Book_Woodcutting") != 0 && r.NextDouble() < 0.05)
                                {
                                    numToDrop *= 2;
                                }
                                Game1.createRadialDebris(location, 12, (int)tileLocation.X + (__instance.shakeLeft.Value ? (-4) : 4), (int)tileLocation.Y, numToDrop, resource: true);
                                Game1.createRadialDebris(location, 12, (int)tileLocation.X + (__instance.shakeLeft.Value ? (-4) : 4), (int)tileLocation.Y, (int)((Game1.getFarmer(__instance.lastPlayerToHit.Value).professions.Contains(12) ? 1.25 : 1.0) * (double)(12 + extraWoodCalculator(tileLocation))), resource: false);
                            }
                            Farmer targetFarmer = Game1.getFarmer(__instance.lastPlayerToHit.Value);
                            if (data.DropWoodOnChop)
                            {
                                Game1.createMultipleObjectDebris("(O)92", (int)tileLocation.X + (__instance.shakeLeft.Value ? (-4) : 4), (int)tileLocation.Y, 5, __instance.lastPlayerToHit.Value, location);
                            }
                            int numHardwood = 0;
                            if (data.DropHardwoodOnLumberChop && targetFarmer != null)
                            {
                                while (targetFarmer.professions.Contains(14) && r.NextBool())
                                {
                                    numHardwood++;
                                }
                            }
                            List<WildTreeChopItemData> chopItems = data.ChopItems;
                            if (chopItems != null && chopItems.Count > 0 && targetFarmer is not null)
                            {
                                bool addedAdditionalHardwood = false;
                                foreach (WildTreeChopItemData drop in data.ChopItems)
                                {
                                    Item item = TryGetDrop(__instance, drop, r, targetFarmer, "ChopItems", null, false);
                                    if (item != null)
                                    {
                                        if (drop.ItemId == "709")
                                        {
                                            numHardwood += item.Stack;
                                            addedAdditionalHardwood = true;
                                        }
                                        else
                                        {
                                            Game1.createMultipleItemDebris(item, new Vector2(tileLocation.X + (float)(__instance.shakeLeft.Value ? (-4) : 4), tileLocation.Y) * 64f, -2, location);
                                        }
                                    }
                                }
                                if (addedAdditionalHardwood && targetFarmer != null && targetFarmer.professions.Contains(14))
                                {
                                    numHardwood += (int)((float)numHardwood * 0.25f + 0.9f);
                                }
                            }
                            if (numHardwood > 0)
                            {
                                Game1.createMultipleObjectDebris("(O)709", (int)tileLocation.X + (__instance.shakeLeft.Value ? (-4) : 4), (int)tileLocation.Y, numHardwood, __instance.lastPlayerToHit.Value, location);
                            }
                            float seedOnChopChance = data.SeedOnChopChance;
                            if (Game1.getFarmer(__instance.lastPlayerToHit.Value).getEffectiveSkillLevel(2) >= 1 && data != null && data.SeedItemId != null && r.NextDouble() < (double)seedOnChopChance)
                            {
                                Game1.createMultipleObjectDebris(data.SeedItemId, (int)tileLocation.X + (__instance.shakeLeft.Value ? (-4) : 4), (int)tileLocation.Y, r.Next(1, 3), __instance.lastPlayerToHit.Value, location);
                            }
                        }
                        if (__instance.health.Value == -100f)
                        {
                            __result = true;
                            return false;
                        }
                        if (__instance.health.Value <= 0f)
                        {
                            __instance.health.Value = -100f;
                        }
                    }
                }
                for (int i = leafs.Count - 1; i >= 0; i--)
                {
                    Leaf leaf = leafs[i];
                    leaf.position.Y -= leaf.yVelocity - 3f;
                    leaf.yVelocity = Math.Max(0f, leaf.yVelocity - 0.01f);
                    leaf.rotation += leaf.rotationRate;
                    if (leaf.position.Y >= tileLocation.Y * 64f + 64f)
                    {
                        leafs.RemoveAt(i);
                    }
                }
                return true;
            }
        }
    }
}
