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
            if (Locations.ContainsKey(loc))
            {
                LocationDetails entry = Locations[loc];
                Game1.warpFarmer(entry.Region, entry.X, entry.Y, false);
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

    }
}
