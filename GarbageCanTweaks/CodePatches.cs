using HarmonyLib;
using StardewValley;
using StardewValley.GameData.Characters;
using StardewValley.GameData.GarbageCans;
using StardewValley.Internal;

namespace GarbageCanTweaks
{
    public partial class ModEntry
    {

        public static NPC birthday;

        public static string bCan;

        public static List<string> noGift = new List<string>()
        {
            "???",
            "Bear",
            "Birdie",
            "Bouncer",
            "Gil",
            "Governor",
            "Grandpa",
            "Gunther",
            "Henchman",
            "Mister Qi",
            "Morris",
            "Old Mariner",
            "Welwick"
        };

        [HarmonyPatch(typeof(GameLocation), nameof(GameLocation.TryGetGarbageItem))]
        public class GameLocation_TryGetGarbageItem_Patch
        {

            public static bool Prefix(GameLocation __instance, string id, double dailyLuck, out Item item, out GarbageCanItemData selected, out Random garbageRandom, ref bool __result, Action<string> logError = null)
            {
                if (!Config.EnableMod)
                {
                    selected = null;
                    item = null;
                    garbageRandom = null;
                    return true;
                }

                GarbageCanData allData = DataLoader.GarbageCans(Game1.content);
                IDictionary<string, CharacterData> npcs = Game1.characterData;
                if (!allData.GarbageCans.TryGetValue(id, out var data))
                    data = null;
                float baseChance = ((data != null && data.BaseChance > 0f) ? data.BaseChance : allData.DefaultBaseChance);
                baseChance += (float)dailyLuck;
                if (Game1.player.stats.Get("Book_Trash") != 0)
                    baseChance += 0.2f;
                garbageRandom = Utility.CreateDaySaveRandom(777 + Game1.hash.GetDeterministicHashCode(id));
                int prewarm = garbageRandom.Next(0, 100);
                for (int j = 0; j < prewarm; j++)
                    garbageRandom.NextDouble();
                for (int i = 0; i < prewarm; i++)
                    garbageRandom.NextDouble();
                selected = null;

                // intermission for my silly birthday stuff
                if (Config.EnableBirthday && birthday is not null && (garbageRandom.NextDouble() < (double)Config.BirthdayChance))
                {
                    if (id.Equals(bCan))
                    {
                        Game1.showGlobalMessage($"You found a birthday item for {birthday.displayName}!");
                        __instance.localSound("cluck");
                        Log($"Happy birthday, {birthday.Name}!");
                        item = birthday.getFavoriteItem();
                        selected = allData.BeforeAll[0]; // just highjack this entry lol
                        selected.IsDoubleMegaSuccess = false;
                        selected.IsMegaSuccess = true;
                        __result = true;
                        return false;
                    }
                }
                Log($"Didn't find a birthday!", debugOnly: true);

                item = null;
                bool baseChancePassed = garbageRandom.NextDouble() < (double)Config.LootChance;
                ItemQueryContext itemQueryContext = new ItemQueryContext(__instance, Game1.player, garbageRandom);
                List<GarbageCanItemData>[] array = new List<GarbageCanItemData>[3]
                {
                    allData.BeforeAll,
                    data?.Items,
                    allData.AfterAll
                };
                for (int i = 0; i < array.Count(); i++)
                {
                    List<GarbageCanItemData> itemList = array[i];
                    if (itemList == null)
                    {
                        continue;
                    }
                    foreach (GarbageCanItemData entry in itemList)
                    {
                        if (string.IsNullOrWhiteSpace(entry.Id))
                        {
                            logError("ignored item entry with no Id field.");
                        }
                        else if ((baseChancePassed || entry.IgnoreBaseChance) && i != 0 && i != 2 && GameStateQuery.CheckConditions(entry.Condition, __instance, null, null, null, garbageRandom))
                        {
                            bool error = false;
                            Item result = ItemQueryResolver.TryResolveRandomItem(entry, itemQueryContext, avoidRepeat: false, null, null, null, delegate (string query, string message)
                            {
                                error = true;
                                logError("failed parsing item query '" + query + "': " + message);
                            });
                            if (!error)
                            {
                                selected = entry;
                                item = result;
                                break;
                            }
                        }
                    }
                    if (selected != null)
                    {
                        break;
                    }
                }
                __result = item != null;
                return false;
            }
        }

    }
}
