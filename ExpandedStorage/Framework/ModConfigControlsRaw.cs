using StardewModdingAPI;

namespace ExpandedStorage.Framework
{
    public class ModConfigControlsRaw
    {
        public string ScrollUp { get; set; } = $"{SButton.DPadUp}";
        public string ScrollDown { get; set; } = $"{SButton.DPadDown}";
        public string PreviousTab { get; set; } = $"{SButton.DPadLeft}";
        public string NextTab { get; set; } = $"{SButton.DPadRight}";
    }
}