﻿using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace GoHomeRobin
{
    public partial class ModEntry : Mod
    {
        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;
        public static ModEntry context;

        public bool startedWalking;
        public int startDay = 900;
        public int endDay = 1700;

        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();
            I18n.Init(helper.Translation);
            context = this;
            SMonitor = Monitor;
            SHelper = helper;

            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;
            helper.Events.GameLoop.TimeChanged += GameLoop_TimeChanged;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
        }

        /// <summary>
		///     Small method that handles Debug mode to make SMAPI logs a bit easier to read.
		/// </summary>
        /// <remarks>
        ///     Allows basic Log functions to upgrade Logs to <see cref="LogLevel.Debug"/>, excluding <see cref="LogLevel.Error"/>, when debugging for ease of reading.<br/>
        ///     For <b>Debug Only</b> Logs -- use <c>debugOnly: true</c> and omit <see cref="LogLevel"/> <code>Log(message, debugOnly: true);</code><br/>
        ///     For Debug Logs that <b>always</b> show -- use <see cref="LogLevel"/> and omit <c>debugOnly</c> <code>Log(message, <see cref="LogLevel"/>);</code>.
        /// </remarks>
		/// <param name="message"></param>
		/// <param name="level"></param>

        public static void Log(string message, LogLevel level = LogLevel.Trace, bool debugOnly = false)
        {
            level = Config.Debug && level != LogLevel.Error ? LogLevel.Debug : level;
            if (!debugOnly) SMonitor.Log(message, level);
            else if (debugOnly && Config.Debug) SMonitor.Log(message, level);
            else return;
        }

        /// <summary>
		///     Small method that handles Debug mode to make SMAPI logs a bit easier to read.
		/// </summary>
        /// <remarks>
        ///     Allows basic Log functions to upgrade Logs to <see cref="LogLevel.Debug"/>, excluding <see cref="LogLevel.Error"/>, when debugging for ease of reading.<br/>
        ///     For <b>Debug Only</b> Logs -- use <c>debugOnly: true</c> and omit <see cref="LogLevel"/> <code>LogOnce(message, debugOnly: true);</code><br/>
        ///     For Debug Logs that <b>always</b> show -- use <see cref="LogLevel"/> and omit <c>debugOnly</c> <code>LogOnce(message, <see cref="LogLevel"/>);</code>
        /// </remarks>
		/// <param name="message"></param>
		/// <param name="level"></param>
		public static void LogOnce(string message, LogLevel level = LogLevel.Trace, bool debugOnly = false)
        {
            level = Config.Debug && level != LogLevel.Error ? LogLevel.Debug : level;
            if (!debugOnly) SMonitor.LogOnce(message, level);
            if (debugOnly && Config.Debug) SMonitor.LogOnce(message, level);
            else return;
        }

        private void GameLoop_DayStarted(object sender, DayStartedEventArgs e)
        {
            DayStarted();
        }

        private void GameLoop_TimeChanged(object sender, TimeChangedEventArgs e)
        {
            TimeChanged(e.NewTime);
        }

        private void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
        {
            Log("Launching with Debug mode enabled.", debugOnly: true);

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
                name: () => I18n.TestBool(),
                tooltip: () => I18n.TestBool_1(),
                getValue: () => Config.EnableMod,
                setValue: value => Config.EnableMod = value
                );

            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => I18n.Debug(),
                tooltip: () => I18n.Debug_1(),
                getValue: () => Config.Debug,
                setValue: value => Config.Debug = value
            );
        }

    }
}
