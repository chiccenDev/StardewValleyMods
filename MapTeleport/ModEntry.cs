using HarmonyLib;
using Microsoft.CodeAnalysis.FlowAnalysis;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using System.Collections.Generic;
using System.IO;

namespace MapTeleport
{
    public partial class ModEntry : Mod
    {
        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;
        public static ModEntry context;
        public static bool hasSVE;

        public static Dictionary<string, LocationDetails> Locations = new();
        public static string MapDataSource;

        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();
            I18n.Init(helper.Translation);
            context = this;
            SMonitor = Monitor;
            SHelper = helper;

            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            helper.Events.Input.ButtonPressed += Input_ButtonPressed;

            // rebug commands for trouble shooting or updating Locations
            helper.ConsoleCommands.Add("mtp_load", "Force Map Teleport to reload locations. Run \"mtp_farm\" to repair farm warp after using this command.", (command, args) => Locations = LoadLocations());
            helper.ConsoleCommands.Add("mtp_farm", "Force Map Teleports to re-check and fix Farm warp coordinates.", (command, args) => CheckFarm(Locations["Farm/Default"]));

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
        }

        /// <summary>
		///     Small method that handles Debug mode to make SMAPI logs a bit easier to read in bug reports.
		/// </summary>
        /// <remarks>
        ///     Allows basic Log functions to upgrade <see cref="LogLevel.Trace"/> Logs to <see cref="LogLevel.Debug"/> when debugging for ease of reading.<br/>
        ///     For <b>Debug Only</b> Logs -- use <c>debugOnly: true</c> and omit <see cref="LogLevel"/> <code>Log(message, debugOnly: true);</code><br/>
        ///     For Debug Logs that <b>always</b> show -- use <see cref="LogLevel"/> and omit <c>debugOnly</c> <code>Log(message, <see cref="LogLevel"/>);</code>
        /// </remarks>
		/// <param name="message"></param>
		/// <param name="level"></param>

        public static void Log(string message, LogLevel level = LogLevel.Trace, bool debugOnly = false)
        {
            level = Config.Debug && level == LogLevel.Trace ? LogLevel.Debug : level;
            if (!debugOnly) SMonitor.Log(message, level);
            else if (debugOnly && Config.Debug) SMonitor.Log(message, level);
            else return;
        }

        /// <summary>
		///     Small method that handles Debug mode to make SMAPI logs a bit easier to read in bug reports.
		/// </summary>
        /// <remarks>
        ///     Allows basic Log functions to upgrade <see cref="LogLevel.Trace"/> Logs to <see cref="LogLevel.Debug"/> when debugging for ease of reading.<br/>
        ///     For <b>Debug Only</b> Logs -- use <c>debugOnly: true</c> and omit <see cref="LogLevel"/> <code>LogOnce(message, debugOnly: true);</code><br/>
        ///     For Debug Logs that <b>always</b> show -- use <see cref="LogLevel"/> and omit <c>debugOnly</c> <code>LogOnce(message, <see cref="LogLevel"/>);</code>
        /// </remarks>
		/// <param name="message"></param>
		/// <param name="level"></param>
		public static void LogOnce(string message, LogLevel level = LogLevel.Trace, bool debugOnly = false)
        {
            level = Config.Debug && level == LogLevel.Trace ? LogLevel.Debug : level;
            if (!debugOnly) SMonitor.LogOnce(message, level);
            if (debugOnly && Config.Debug) SMonitor.LogOnce(message, level);
            else return;
        }

        private void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
        {
            Log("Launching with Debug mode enabled.", debugOnly: true);
            Log($"\tEnableMod: {Config.EnableMod}\n\tAllowUnknown: {Config.AllowUnknown}\n\tEnableAudio: {Config.EnableAudio}", debugOnly: true);


            // GMCM //
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
                tooltip: () => I18n.EnableMod(),
                getValue: () => Config.EnableMod,
                setValue: value => Config.EnableMod = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => I18n.AllowUnknown(),
                tooltip: () => I18n.AllowUnknown_1(),
                getValue: () => Config.AllowUnknown,
                setValue: value => Config.AllowUnknown = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => I18n.EnableAudio(),
                tooltip: () => I18n.EnableAudio_1(),
                getValue: () => Config.EnableAudio,
                setValue: value => Config.EnableAudio = value
            );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => I18n.Debug(),
                tooltip: () => I18n.Debug_1(),
                getValue: () => Config.Debug,
                setValue: value => Config.Debug = value
            );

            // SVE check, purely for situational awareness
            hasSVE = (Helper.ModRegistry.IsLoaded("FlashShifter.StardewValleyExpandedCP")) ;
            Log($"User {(hasSVE ? "has" : "does not have")} Stardew Valley Expanded", debugOnly: true);

            // Load Locations
            MapDataSource = Path.Combine(SHelper.DirectoryPath, "assets", "Locations.json");
            Locations = LoadLocations();
        }

        private void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!Config.Debug || !Config.EnableMod || (e.Button != SButton.F2)) return;
            SaveLocations(Locations);
        }

        private void GameLoop_SaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            if (Config.EnableMod) CheckFarm(Locations["Farm/Default"]);
        }

    }
}
