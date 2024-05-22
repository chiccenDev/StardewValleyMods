using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.TerrainFeatures;
using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FruitTreeTweaks
{
    public partial class ModEntry
    {
        private static Dictionary<GameLocation, (List<Color> colors, List<float> sizes, List<Vector2> offsets)> fruitData = new();
        private static int fruitToday;

        private static float GetTreeBottomOffset(FruitTree tree)
        {
            if (!Config.EnableMod)
                return 1E-07f;
            return 1E-07f + Game1.getFarm().terrainFeatures.Pairs.FirstOrDefault(pair => pair.Value == tree).Key.X / 100000f;
        }
        private static bool CanPlantAnywhere()
        {
            return Config.PlantAnywhere;
        }
        private static int GetMaxFruit()
        {
            return !Config.EnableMod ? 3 : Math.Max(1, Config.MaxFruitPerTree);
        }
        private static int GetFruitPerDay()
        {
            return !Config.EnableMod ? 1 : Game1.random.Next(Config.MinFruitPerDay, Math.Max(Config.MinFruitPerDay, Config.MaxFruitPerDay + 1));
        }
        private static int ChangeDaysToMatureCheck(int oldValue)
        {
            if (!Config.EnableMod)
                return oldValue;
            switch (oldValue)
            {
                case 0:
                    return 0;
                case 7:
                    return Config.DaysUntilMature / 4;
                case 14:
                    return Config.DaysUntilMature / 2;
                case 21:
                    return Config.DaysUntilMature * 3 / 4;
            }
            return oldValue;
        }
        private static Color GetFruitColor(FruitTree tree, int index)
        {
            if (!Config.EnableMod)
                return Color.White;
            if (!fruitData.TryGetValue(tree.Location, out var data) || data.colors.Count < tree.fruit.Count)
            {
                ReloadFruit(tree.Location, tree.fruit.Count);
                fruitData.TryGetValue(tree.Location, out data);
            }
            return data.colors[index];
        }
        private static float GetFruitScale(FruitTree tree, int index)
        {
            if (!Config.EnableMod)
                return 4;
            if (!fruitData.TryGetValue(tree.Location, out var data) || data.sizes.Count < tree.fruit.Count)
            {
                ReloadFruit(tree.Location, tree.fruit.Count);
                fruitData.TryGetValue(tree.Location, out data);
            }
            return data.sizes[index];
        }
        private static Vector2 GetFruitOffsetForShake(FruitTree tree, int index)
        {
            if (!Config.EnableMod || index < 2)
                return Vector2.Zero;
            return GetFruitOffset(tree, index);
        }

        private static Vector2 GetFruitOffset(FruitTree tree, int index)
        {
            if (!fruitData.TryGetValue(tree.Location, out var data) || data.offsets.Count < tree.fruit.Count)
            {
                ReloadFruit(tree.Location, tree.fruit.Count);
                fruitData.TryGetValue(tree.Location, out data);
            }
            return data.offsets[index];
        }

        private static void ReloadFruit(GameLocation location, int max)
        {
            // init fruit data
            if (!fruitData.ContainsKey(location))
            {
                fruitData.Add(location, (new List<Color>(), new List<float>(), new List<Vector2>()));
            }
            fruitData.TryGetValue(location, out var data);

            // fruit colors
            if (data.colors.Count < max)
            {
                data.colors.Clear();
                for (int i = 0; i < max; i++)
                {
                    var color = Color.White;
                    color.R -= (byte)(Game1.random.NextDouble() * Config.ColorVariation);
                    color.G -= (byte)(Game1.random.NextDouble() * Config.ColorVariation);
                    color.B -= (byte)(Game1.random.NextDouble() * Config.ColorVariation);
                    data.colors.Add(color);
                }
            }
            // fruit sizes
            if (data.sizes.Count < max)
            {
                data.sizes.Clear();
                for (int i = 0; i < max; i++)
                {
                    data.sizes.Add(4 * (float)(1 + ((Game1.random.NextDouble() * 2 - 1) * Config.SizeVariation / 100)));
                }
            }
            // fruit offsets
            if (data.offsets.Count != max)
            {
                data.offsets.Clear();
                SMonitor.Log($"Resetting fruit offsets in {location.Name}");
                for (int i = 0; i < max; i++)
                {

                    if (i < 3)
                    {
                        data.offsets.Add(Vector2.Zero);
                        continue;
                    }
                    bool gotSpot = false;
                    Vector2 offset;
                    while (!gotSpot)
                    {
                        double distance = 24;
                        for (int j = 0; j < 100; j++)
                        {
                            gotSpot = true;
                            offset = new Vector2(Config.FruitSpawnBufferX + Game1.random.Next(34 * 4 - Config.FruitSpawnBufferX), Config.FruitSpawnBufferY + Game1.random.Next(58 * 4 - Config.FruitSpawnBufferY));
                            for (int k = 0; k < data.offsets.Count; k++)
                            {
                                if (Vector2.Distance(data.offsets[k], offset) < distance)
                                {
                                    distance--;
                                    gotSpot = false;
                                    break;
                                }
                            }
                            if (gotSpot)
                            {
                                data.offsets.Add(offset);
                                break;
                            }
                        }
                    }
                }
            }
        }
    }
}