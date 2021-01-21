using System;
using System.Diagnostics.CodeAnalysis;
using StardewModdingAPI;

namespace ExpandedStorage.Framework
{
    [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Global")]
    internal class ModConfigControls
    {
        internal SButton ScrollUp;
        internal SButton ScrollDown;
        internal SButton PreviousTab;
        internal SButton NextTab;

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
        }
    }
}