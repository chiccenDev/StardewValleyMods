using StardewValley;
using StardewValley.TerrainFeatures;
using Microsoft.Xna.Framework;
using Object = StardewValley.Object;
using StardewModdingAPI.Events;

namespace WildTreeTweaks
{
    public partial class ModEntry
    {

        public static bool canPlaceWildTreeSeed(Object tree, GameLocation location, Vector2 tile, out string deniedMessage)
        {
            if (!tree.IsWildTreeSapling())
            {
                deniedMessage = string.Empty;
                return false;
            }

            if (location.getBuildingAt(tile) is not null)
            {
                deniedMessage = "Tile is occupied by a building.";
                return false;
            }
            if (location.terrainFeatures.TryGetValue(tile, out var terrainFeature) && !(terrainFeature is HoeDirt { crop: null }))
            {
                deniedMessage = "Tile is occupied by crops.";
                return false;
            }
            if (location.IsTileOccupiedBy(tile))
            {
                deniedMessage = "Tile is occupied.";
                return false;
            }
            if (terrainFeature is not null && !terrainFeature.isPassable())
            {
                deniedMessage = "Tile is blocked by terrain!";
                return false;
            }
            if (!location.isTilePlaceable(tile))
            {
                deniedMessage = "Tile is not placeable.";
                return false;
            }
            if (location.objects.ContainsKey(tile))
            {
                deniedMessage = "Tile is occupied by an object.";
                return false;
            }
            if(!location.IsOutdoors && !location.treatAsOutdoors.Value)
            {
                deniedMessage = "Cannot place indoors!";
                return false;
            }
            if (location.doesTileHaveProperty((int)tile.X, (int)tile.Y, "Water", "Back") is not null)
            {
                deniedMessage = "Cannot plant here.";
                return false;
            }

            deniedMessage = string.Empty;
            return true;
        }

        private void Player_Warped(object sender, WarpedEventArgs e)
        {
            GameLocation location = e.NewLocation;
            if (!Config.EnableMod || !updateTrees || (!location.IsFarm && Config.OnlyOnFarm) || (!location.IsOutdoors && !location.treatAsOutdoors.Value)) return;
            Log($"Updating location {e.NewLocation.NameOrUniqueName}", debugOnly: true);

            updateLocations.Add(location);
            updateTrees = (updateLocations.Count < Game1.locations.Count);

            foreach (TerrainFeature feature in location._activeTerrainFeatures)
            {
                if (feature is not Tree) continue;
                Tree tree = (Tree)feature;
                tree.GetData();
                tree.health.Value = Config.Health;
            }
        }

    }
}
