using System;
using StardewModdingAPI;

namespace ExpandedStorage.Framework
{
    internal class ModConfigControls
    {
        internal SButton ScrollUp;
        internal SButton ScrollDown;
        internal SButton PreviousTab;
        internal SButton NextTab;
        internal SButton CarryChest;

        internal ModConfigControls(ModConfigControlsRaw controlsRaw)
        {
            if (Enum.TryParse(controlsRaw.ScrollUp, out SButton scrollUp))
                ScrollUp = scrollUp;
            if (Enum.TryParse(controlsRaw.ScrollDown, out SButton scrollDown))
                ScrollDown = scrollDown;
            if (Enum.TryParse(controlsRaw.PreviousTab, out SButton previousTab))
                PreviousTab = previousTab;
            if (Enum.TryParse(controlsRaw.NextTab, out SButton nextTab))
                NextTab = nextTab;
            if (Enum.TryParse(controlsRaw.CarryChest, out SButton carryChest))
                CarryChest = carryChest;
        }
    }
}