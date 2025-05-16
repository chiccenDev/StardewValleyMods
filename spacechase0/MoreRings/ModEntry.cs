﻿using HarmonyLib;
using MoreRings.Framework;
using StardewHack.WearMoreRings;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;

namespace MoreRings
{
    /// <summary>
    /// 
    /// !!!IMPORTANT!!!
    /// BEFORE USING THIS TEMPLATE:
    /// Change "MyMod" to your mod's name/namespace in the following locations:
    ///     (1) ModEntry.cs namespace
    ///     (2) IGenericModConfigMenuApi.cs namespace
    ///     (3) ModConfig.cs namespace
    ///     (4) Manifest.json
    ///         - Name
    ///         - Description (optional)
    ///         - UniqueID
    ///         - EntryDll
    ///         
    /// It is also recommended you update "Description" and Nexus update key in manifest.json
    /// You can delete this entire comment summary once these tasks are completed.
    /// 
    /// </summary>


    public partial class ModEntry : Mod
    {
        private IWearMoreRingsAPI_2 WearMoreRings;

        public static IMonitor SMonitor;
        public static IModHelper SHelper;
        public static ModConfig Config;
        public static ModEntry context;

        private float HealthRegenRemainder;
        private float StaminaRegenRemainder;

        public bool HasWearMoreRings => WearMoreRings != null;

        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<ModConfig>();
            I18n.Init(helper.Translation);
            context = this;
            SMonitor = Monitor;
            SHelper = helper;

            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.Display.MenuChanged += Display_OnMenuChanged;
            helper.Events.Display.RenderedWorld += Display_OnRenderedWorld;
            helper.Events.GameLoop.UpdateTicked += GameLoop_OnUpdateTicked;

            var harmony = new Harmony(ModManifest.UniqueID);
            harmony.PatchAll();
        }

        /// <summary>
		///     Small method that handles Debug mode to make SMAPI logs a bit easier to read in bug reports.
		/// </summary>
        /// <remarks>
        ///     Allows basic Log functions to upgrade <see cref="LogLevel.Trace"/> Logs to <see cref="LogLevel.Debug"/> when debugging for ease of reading.<br/>
        ///     For <b>Debug Only</b> Logs -- use <c>debugOnly: true</c> and omit <see cref="LogLevel"/> <code>Log(message, debugOnly: true);</code><br/>
        ///     For Debug Logs that <b>always</b> show -- use <see cref="LogLevel"/> and omit <c>debugOnly</c> <code>Log(message, <see cref="LogLevel"/>);</code>.
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

            var WearMoreRings = Helper.ModRegistry.GetApi<IWearMoreRingsAPI_2>("bcmpinc.WearMoreRings");
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
                name: () => I18n.Debug(),
                tooltip: () => I18n.Debug_1(),
                getValue: () => Config.Debug,
                setValue: value => Config.Debug = value
            );
        }

        private void Display_OnMenuChanged(object sender, MenuChangedEventArgs e)
        {
            if (e.NewMenu is BobberBar bobber && HasRingEquipped("Ring_of_Wide_Nets"))
            {
                var field = Helper.Reflection.GetField<int>(bobber, "bobberBarHeight");
                field.SetValue((int)(field.GetValue() * Config.RingOfWideNets_BarSizeMultiplier));
            }
        }

        private void Display_OnRenderedWorld(object sender, RenderedWorldEventArgs e)
        {
            if (!Context.IsWorldReady || !HasRingEquipped("Ring of True Sight")) return;
            TrueSight.DrawOverWorld(e.SpriteBatch);
        }

        private void GameLoop_OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (!Context.IsPlayerFree || !e.IsOneSecond) return;

            int hasHealthsRings = CountRingsEquipped("Ring_of_Regeneration");
            int hasStaminaRings = CountRingsEquipped("Refreshing_Ring");

            if (hasHealthsRings > 0)
            {
                HealthRegenRemainder += Config.RingOfRegeneration_RegenPerSecond * hasHealthsRings;
                if (HealthRegenRemainder > 0)
                {
                    Game1.player.health = Math.Min(Game1.player.health + (int)HealthRegenRemainder, Game1.player.maxHealth);
                    HealthRegenRemainder %= 1;
                }
            }

            if (hasStaminaRings > 0)
            {
                StaminaRegenRemainder += Config.RefreshingRing_RegenPerSecond * hasStaminaRings;
                if (StaminaRegenRemainder > 0)
                {
                    Game1.player.Stamina = Math.Min(Game1.player.Stamina + (int)StaminaRegenRemainder, Game1.player.MaxStamina);
                    StaminaRegenRemainder %= 1;
                }
            }
        }

        private void OnItemEaten()
        {
            if (HasRingEquipped("Ring_Of_Diamond_Booze"))
            {
                if (Game1.player.hasBuff(Buff.tipsy))
                {
                    Game1.player.buffs.Remove(Buff.tipsy);
                }
            }
        }

    }
}
