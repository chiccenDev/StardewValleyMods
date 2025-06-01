using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System.Globalization;

namespace Skateboard
{
    /// <summary>The mod entry point.</summary>
    public partial class ModEntry : Mod
    {

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;
        public static ModEntry context;
        
        public static readonly string boardKey = "aedenthorn.Skateboard/Board"; // modData tag to identify Skateboard
        public static readonly string sourceKey = "aedenthorn.Skateboard/SourceRect"; // modData containing sourceRect i.e. tileIndex for skateboard sprite
        public static readonly string boardIndex = "chiccen.Skateboard"; // Skateboard ItemId
        public static readonly string skateboardingKey = "aedenthorn.Skateboard/Skateboarding"; // "player is skateboarding" player state kinda thing
        public static readonly string textureKey = "Mods/chiccen.SkateboardCP/Skateboard"; // texture as loaded by content patcher

        public static bool accelerating;
        private static Texture2D boardTexture;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();

            if (!Config.ModEnabled)
                return;

            I18n.Init(helper.Translation);
            context = this;
            SMonitor = Monitor;
            SHelper = helper;

            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            helper.Events.Player.Warped += Player_Warped;
            helper.Events.Input.ButtonPressed += Input_ButtonPressed;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
            helper.ConsoleCommands.Add("skateboard", "Spawn a skateboard for free!", (command, args) => SpawnSkateboard());
        }

        private void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
        {
            var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is null)
                return;

            configMenu.Register(
                mod: ModManifest,
                reset: () => Config = new ModConfig(),
                save: () => Helper.WriteConfig(Config)
            );

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => I18n.EnableMod(),
                getValue: () => Config.ModEnabled,
                setValue: value => Config.ModEnabled = value
            );
            configMenu.AddKeybind(
                mod: ModManifest,
                name: () => I18n.RideButton(),
                tooltip: () => I18n.RideButton_1(),
                getValue: () => Config.RideButton,
                setValue: value => Config.RideButton = value
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => I18n.MaxSpeed(),
                tooltip: () => I18n.MaxSpeed_1(),
                getValue: () => Config.MaxSpeed + "",
                setValue: delegate (string value) { try { Config.MaxSpeed = float.Parse(value, NumberStyles.Any, CultureInfo.InvariantCulture); } catch { } }
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => I18n.AccelRate(),
                tooltip: () => I18n.AccelRate_1(),
                getValue: () => Config.Acceleration + "",
                setValue: delegate (string value) { try { Config.Acceleration = float.Parse(value, NumberStyles.Any, CultureInfo.InvariantCulture); } catch { } }
            );
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => I18n.DeaccelRate(),
                tooltip: () => I18n.DeaccelRate_1(),
                getValue: () => Config.Deceleration + "",
                setValue: delegate (string value) { try { Config.Deceleration = float.Parse(value, NumberStyles.Any, CultureInfo.InvariantCulture); } catch { } }
            );/*
            configMenu.AddTextOption(
                mod: ModManifest,
                name: () => "Crafting Reqs",
                getValue: () => Config.CraftingRequirements + "",
                setValue: value => Config.CraftingRequirements = value
            );*/
        }

        private void Player_Warped(object sender, WarpedEventArgs e)
        {
            foreach (var key in e.NewLocation.Objects.Keys) // for every object in the new area
            {
                if (e.NewLocation.Objects[key]?.modData.ContainsKey(boardKey) == true) // if one of em is the skateboard
                {
                    e.NewLocation.Objects.Remove(key); // remove it
                }
            }
            if (e.NewLocation.currentEvent is not null)
            {
                e.Player.drawOffset = Vector2.Zero; // if there is an event in the location, reset the player draw offset
            }
        }

        private void GameLoop_SaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            Monitor.Log("Loading skatebaord texture...");
            boardTexture = Game1.content.Load<Texture2D>(textureKey);
            Monitor.Log("loaded skateboard texture!");
        }

        private void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (Config.ModEnabled && Context.CanPlayerMove && e.Button == Config.RideButton && !Game1.currentLocation.isActionableTile((int)Game1.currentCursorTile.X, (int)Game1.currentCursorTile.Y, Game1.player)) // if pressed ride button + other stuff
            {
                if (Game1.player.modData.ContainsKey(skateboardingKey)) // if already skateboarding
                {
                    SpawnSkateboard(); // produce an item
                    speed = Vector2.Zero; // stop the player movements
                    Game1.player.modData.Remove(skateboardingKey); // remove the modData tag that says player is skateboarding
                    Game1.player.drawOffset = Vector2.Zero; // repair player draw offset
                }
                else if (Game1.player.CurrentItem is not null && Game1.player.CurrentItem.modData.ContainsKey(boardKey)) // if the player isnot riding a skateboard but is holding one
                {
                    Game1.player.reduceActiveItemByOne(); // remove Skateboard from inventory (to put under feet)
                    speed = Vector2.Zero; // stop player movements
                    Game1.player.modData[skateboardingKey] = "true"; // set modData tag to say player is skateboarding
                }
            }
        }
    }
}