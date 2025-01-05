using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MapTeleport
{
    public class LocationDetails
    {
        public string Region { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public string Condition { get; set; }
    }

    public partial class ModEntry
    {
        public static Dictionary<string, LocationDetails> LoadLocations()
        {
            string jsonPath = Path.Combine(SHelper.DirectoryPath, @"assets\Locations.json"); // hope this shit actually works cross-platform
            Log($"Loading Warp locations from {jsonPath}", debugOnly: true);

            try
            {
                string jsonText = File.ReadAllText(jsonPath);
                var locations = JsonSerializer.Deserialize<Dictionary<string, LocationDetails>>(jsonText);

                if (locations != null)
                {
                    Log("Map Teleport locations loaded successfully!");
                    //string testVal = sve ? "Custom_IridiumQuarry/Custom_IridiumQuarry" : "Forest/MarnieRanch";
                    Log($"Data validation: {(locations.ContainsKey("Forest/MarnieRanch") ? "passed!" : "failed! :(")}", debugOnly: true);
                    return locations;
                }
                else { return new Dictionary<string, LocationDetails>(); }
                
            }
            catch (Exception e)
            {
                Log($"Error: {e.Message}", LogLevel.Error);
                return new Dictionary<string, LocationDetails>();
            }

        }

        public static void TryWarp(string loc)
        {
            
            Log($"MapWarp() called for {loc}", debugOnly: true);

            if (loc.Contains("Town/JojaMart") || loc.Contains("Town/MovieTheater_Community")) loc = "Town/JojaMart";
            if (loc.Contains("PierreStore")) loc = "Town/PierreStore";
            if (loc.Contains("Beach/FishShop")) loc = "Beach/FishShop";
            if (loc.Contains("PamHouse")) loc = "Town/Trailer";

            if (Locations.ContainsKey(loc))
            {
                LocationDetails entry = Locations[loc];
                if (loc.Equals("Farm/Default")) CheckFarm(entry);
                if (Config.AllowUnknown || (entry.Condition != null ? GameStateQuery.CheckConditions(entry.Condition) : true))
                    Game1.warpFarmer(entry.Region, entry.X, entry.Y, 2);
                else { 
                    Game1.showRedMessage(I18n.WarpFail());
                    Log(I18n.WarpFail_1() + entry.Condition, debugOnly: true);
                }
            }
            else { Log($"No Map Warp for {loc}, sorry!", debugOnly: true); }
            
        }
        /// <summary>
        ///     For testing warps to see what may be broken without freezing or crashing the game.
        /// </summary>
        public static void TestWarp(string loc)
        {
            
            Log($"TestWarp() called for {loc}", LogLevel.Alert);
            if (Locations.ContainsKey(loc))
            {
                LocationDetails entry = Locations[loc];
                if (loc.Equals("Farm/Default")) CheckFarm(entry);
                GameLocation location;
                try { location = Game1.getLocationFromNameInLocationsList(entry.Region); }
                catch (Exception e) { 
                    Log($"Failed to get GameLocation: {e.Message}", LogLevel.Error);
                    location = Game1.getLocationFromNameInLocationsList("Tent");
                }
                Log($"Teleport requested to {loc}!\n\tRegion: {entry.Region}\n\tX: {entry.X}\n\tY: {entry.Y}\n\tLocation:{(location?.name != null ? location.name : "null")}", LogLevel.Warn);
            }
            else { Log($"No Map Warp for {loc}, sorry!", LogLevel.Error); }
            
        }

        public static void CheckFarm(LocationDetails entry)
        {
            Log("Warping to farm. Checking farm type.", debugOnly: true);
            Point door = Farm.getFrontDoorPositionForFarmer(Game1.player);
            Log($"Original point: ({entry.X}, {entry.Y}). New point: ({door.X}, {door.Y}).", debugOnly: true);
            entry.X = door.X;
            entry.Y = ++door.Y;
        }

    }
}
