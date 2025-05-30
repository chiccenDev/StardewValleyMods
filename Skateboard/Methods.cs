using StardewValley;

namespace Skateboard
{
    public partial class ModEntry
    {
        private static void SpawnSkateboard()
        {
            var s = ItemRegistry.Create(boardIndex); // create skateboard
            s.modData.Add(boardKey, "true"); // add modData boardKey to it so it can be identified as Skateboard
            if (!Game1.player.addItemToInventoryBool(s, true)) // try to add directly to players inventory. if that fails...
            {
                Game1.createItemDebris(s, Game1.player.Position, 1, Game1.player.currentLocation); // drop the item at the player's feet to be picked up instead
            }
            SMonitor.Log("Spawned a skateboard.");
        }
    }
}
