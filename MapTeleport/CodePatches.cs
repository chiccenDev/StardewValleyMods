using Microsoft.CodeAnalysis.FlowAnalysis;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapTeleport
{
    public partial class ModEntry
    {
        public static bool CheckClickableComponents(List<ClickableComponent> components, int topX, int topY, int x, int y)
        {
            if (!Config.EnableMod) return false;

            return true;
        }
    }
}
