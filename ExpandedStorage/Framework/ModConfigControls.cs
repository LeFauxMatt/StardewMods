using System;
using StardewModdingAPI;

namespace ExpandedStorage.Framework
{
    internal class ModConfigControls
    {
        internal SButton ScrollUp;
        internal SButton ScrollDown;
        internal SButton CarryChest;

        internal ModConfigControls(ModConfigControlsRaw modConfigControls)
        {
            if (Enum.TryParse(modConfigControls.ScrollUp, out SButton scrollUp))
                ScrollUp = scrollUp;
            if (Enum.TryParse(modConfigControls.ScrollDown, out SButton scrollDown))
                ScrollDown = scrollDown;
            if (Enum.TryParse(modConfigControls.CarryChest, out SButton carryChest))
                CarryChest = carryChest;
        }
    }
}