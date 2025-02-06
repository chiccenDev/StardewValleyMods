using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using System.Text.Json;

namespace MapTeleport
{
    /// <summary>
    ///     LocationDetails class for Stardew Valley MapToolTip data
    /// </summary>
    /// <remarks>
    ///     Mimicks Stardew Valley's native <seealso cref="StardewValley.WorldMaps.MapAreaTooltip"/> class to store minimal data for all locations, including custom modded locations.
    /// </remarks>
    public class LocationDetails
    {
        public string? Region { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public string? Condition { get; set; }
    }

    public partial class ModEntry
    {
        #region Load/Save
        /// <summary>
        /// Load Locations.json and store into Dictionary<\string, LocationDetails> for future reference.
        /// </summary>
        /// <returns>Dictionary<<see cref="string"/>, <see cref="LocationDetails"/>></returns>
        public static Dictionary<string, LocationDetails> LoadLocations()
        {
            if (!Config.EnableMod) return new Dictionary<string, LocationDetails>();

            Log($"Loading Warp locations from {MapDataSource}", debugOnly: true);

            try
            {
                string jsonText = File.ReadAllText(MapDataSource);
                var locations = JsonSerializer.Deserialize<Dictionary<string, LocationDetails>>(jsonText);

                if (locations != null)
                {
                    Log("Map Teleport locations loaded successfully!");
                    var postLoadQC = locations.ContainsKey("Forest/MarnieRanch"); // bit mediocre of a test but it'll check if file is messed up
                    Log($"Data validation: {(postLoadQC ? "passed!" : "failed! Please re-download MapTeleport from Nexus or send in a bug report! :(")}",(postLoadQC ? LogLevel.Trace : LogLevel.Error), debugOnly: !postLoadQC); // and if it doesn't have such a basic location, spit out error
                    return locations;
                }
                else { 
                    Log("JSON data failed to deserialize. Please re-download from Nexus or post a bug report if re-downloading failed to resolve this error.", LogLevel.Error);
                    return new Dictionary<string, LocationDetails>();
                }
                
            }
            catch (Exception e)
            {
                Log($"Error: {e.Message}", LogLevel.Error);
                return new Dictionary<string, LocationDetails>();
            }

        }
        /// <summary>
        /// Save Dictionary<\string, LocationDetails> into the Locations.json file for future re-use.
        /// </summary>
        /// <param name="loc"></param>
        public static void SaveLocations(Dictionary<string, LocationDetails> loc)
        {
            if (!Config.EnableMod || !Config.Debug) return;

            string json = JsonSerializer.Serialize(loc);
            File.WriteAllText(MapDataSource, json);
            Log($"Locations have been saved to {MapDataSource}!", LogLevel.Info);
        }
        #endregion
        public static bool TryWarp(string loc)
        {
            if (!Config.EnableMod) return false;

            Log($"MapWarp() called for {loc}", debugOnly: true);

            if (hasSVE)
            {
                switch (loc)
                {
                    case "Mountain/AdventureGuild":
                        loc = "Custom/AdventureGuild"; // shares a tooltip name, but is actually located in different "region"
                        break;
                    case "SecretWoods/Default": // necessary because internal map layout is different, so different x/y needed
                        loc = "Woods/Default";
                        break;
                    default: break;
                }
            }

            if (Locations.ContainsKey(loc))
            {
                LocationDetails entry = Locations[loc];
                Log($"Warping to Region: {entry.Region}\n\tX: {entry.X}\n\tY: {entry.Y}", debugOnly: true);
                if (Config.AllowUnknown || (entry.Condition != null ? GameStateQuery.CheckConditions(entry.Condition) : true))
                {
                    Game1.warpFarmer(entry.Region, entry.X, entry.Y, 2);
                    if (Config.EnableAudio) PlayAudio();
                    return true;
                }
                else { 
                    Game1.showRedMessage(I18n.WarpFail());
                    Log(I18n.WarpFail_1() + entry.Condition);
                    return false;
                }
            }
            else if (Config.Debug)
            {
                Locations[loc] = new LocationDetails
                {
                    Region = null,
                    X = 0,
                    Y = 0,
                    Condition = null
                };
                Log($"{loc} has been added to Locations list. Press F2 to save.", LogLevel.Alert);
            }
            else { Log($"No Map Warp for {loc}, sorry!"); }
            return false;
            
        }
        #region auxiliary methods
        public static void CheckFarm(LocationDetails entry)
        {
            if (!Config.EnableMod) return;

            Log("Patching warp info for Farm");
            Point door = Farm.getFrontDoorPositionForFarmer(Game1.player);
            Log($"Original point: ({entry.X}, {entry.Y}). New point: ({door.X}, {door.Y}).");
            entry.X = door.X;
            entry.Y = ++door.Y;
        }

        public static void PlayAudio()
        {
            Game1.playSound("grassyStep");
            DelayedAction.playSoundAfterDelay("grassyStep", 400);
            DelayedAction.playSoundAfterDelay("grassyStep", 800);
        }
        #endregion
    }
}