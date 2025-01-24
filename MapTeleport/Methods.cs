using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using System.Text.Json;

namespace MapTeleport
{
    public class LocationDetails
    {
        public string Region { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public string? Condition { get; set; }
    }

    public partial class ModEntry
    {
        #region Load/Save
        public static Dictionary<string, LocationDetails> LoadLocations()
        {
            if (!Config.EnableMod) return new Dictionary<string, LocationDetails>();

            string jsonPath = Path.Combine(SHelper.DirectoryPath, "assets", "Locations.json");
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
        public static void SaveLocations(Dictionary<string, LocationDetails> loc)
        {
            if (!Config.EnableMod || !Config.Debug) return;

            string savePath = Path.Combine(SHelper.DirectoryPath, "assets", "Locations.json");
            string json = JsonSerializer.Serialize(loc);
            File.WriteAllText(savePath, json);
        }
        #endregion
        public static bool TryWarp(string loc)
        {
            if (!Config.EnableMod) return false;

            
            Log($"MapWarp() called for {loc}", debugOnly: true);

            // sve patching, may find a more elegant solution in the future
            if (hasSVE)
            {
                switch (loc)
                {
                    case "Mountain/AdventureGuild":
                        loc = "Custom/AdventureGuild";
                        break;
                    case "SecretWoods/Default":
                        loc = "Woods/Default";
                        break;
                    default: break;
                }
            }

            if (Locations.ContainsKey(loc))
            {
                LocationDetails entry = Locations[loc];
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
                    X = 0,
                    Y = 0,
                    Condition = null
                };
                Log($"{loc} has been added to Locations list. Press F2 to save.", LogLevel.Alert);
            }
            else { Log($"No Map Warp for {loc}, sorry!"); }
            return false;
            
        }
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


    }
}