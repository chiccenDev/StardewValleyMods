using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Pathfinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoHomeRobin
{
    public partial class ModEntry
    {

        public bool getRobin(out NPC robin)
        {
            robin = Game1.getCharacterFromName("Robin");
            if (robin is null)
            {
                Log($"Robin could not be found.", LogLevel.Warn);
                return false;
            }
            return true;
        }

        public bool isFestivalDay()
        {
            return Utility.isFestivalDay(Game1.dayOfMonth, Game1.season);
        }

        public void DayStarted()
        {
            startedWalking = false;
            if (!Config.EnableMod || Utility.isFestivalDay(Game1.dayOfMonth, Game1.season)) return;

            var robin = Game1.getCharacterFromName("Robin");
            if (robin is null)
            {
                Log($"Robin could not be found.", LogLevel.Warn);
                return;
            }
            robin.shouldPlayRobinHammerAnimation.Value = false;
            robin.ignoreScheduleToday = false;
            robin.resetCurrentDialogue();
            robin.reloadDefaultLocation();
            Game1.warpCharacter(robin, robin.DefaultMap, robin.DefaultPosition / 64f);
            Farm farm = Game1.getFarm();
        }

        public void TimeChanged(int time)
        {
            if (!Config.EnableMod || !Game1.IsMasterGame || isFestivalDay() || (!Game1.getFarm().isThereABuildingUnderConstruction() && Game1.player.daysUntilHouseUpgrade.Value <= 0 && (Game1.getLocationFromName("Town") as Town).daysUntilCommunityUpgrade.Value <= 0)) return;

            if (getRobin(out NPC robin)) return;

            string destination;
            int destX, destY;
            int travelTime;

            if (Game1.getFarm().isThereABuildingUnderConstruction() || Game1.player.daysUntilHouseUpgrade.Value > 0)
            {
                destination = "BusStop";
                travelTime = 160;
                destX = -1;
                destY = 23;

            }
            else if (Game1.MasterPlayer.mailReceived.Contains("pamHouseUpgrade"))
            {
                destination = "BusStop";
                travelTime = 170;
                destX = 11;
                destY = 10;
            }
            else
            {
                destination = "Town";
                travelTime = 120;
                destX = 72;
                destY = 60;
            }
            travelTime = Utility.ModifyTime(900, -travelTime);

            if (!startedWalking && time >= travelTime && time < endDay && !robin.shouldPlayRobinHammerAnimation.Value)
            {
                startedWalking = true;
                if (robin.currentLocation.Name == destination && robin.Tile.X == destX && robin.Tile.Y == destY)
                {
                    Log($"Robin is staring work in {destination} at {time}", debugOnly: true);
                    AccessTools.Method(typeof(NPC), "updateConstructionAnimation").Invoke(robin, new object[0]);
                    return;
                }
                Log($"Robin is walking to work in {destination} at {time}");
                robin.ignoreScheduleToday = false;
                robin.reloadSprite();
                robin.lastAttemptedSchedule = -1;
                robin.temporaryController = null;
                Dictionary<int, SchedulePathDescription> schedule = new Dictionary<int, SchedulePathDescription>() { { Game1.timeOfDay, (SchedulePathDescription)AccessTools.Method(typeof(NPC), "pathfindToNextScheduleLocation").Invoke(robin, new object[] { robin.currentLocation.Name, robin.Tile.X, robin.Tile.Y, destination, destX, destY, 3, null, null }) } };
                robin.TryLoadSchedule("work", schedule);
                robin.checkSchedule(Game1.timeOfDay);
            }
        }

    }
}
