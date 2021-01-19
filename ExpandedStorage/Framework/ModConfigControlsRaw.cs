using StardewModdingAPI;

namespace ExpandedStorage.Framework
{
    public class ModConfigControlsRaw
    {
        public string ScrollUp { get; set; } = $"{SButton.LeftThumbstickUp}";
        public string ScrollDown { get; set; } = $"{SButton.LeftThumbstickDown}";
        public string PreviousTab { get; set; } = $"{SButton.LeftTrigger}";
        public string NextTab { get; set; } = $"{SButton.RightTrigger}";
        public string CarryChest { get; set; } = $"{SButton.ControllerX}";
    }
}