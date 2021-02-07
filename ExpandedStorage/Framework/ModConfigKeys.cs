using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace ExpandedStorage.Framework
{
    public class ModConfigKeys
    {
        public KeybindList OpenCrafting { get; set; } = KeybindList.ForSingle(SButton.K);
        public KeybindList ScrollUp { get; set; } = KeybindList.ForSingle(SButton.DPadUp);
        public KeybindList ScrollDown { get; set; } = KeybindList.ForSingle(SButton.DPadDown);
        public KeybindList PreviousTab { get; set; } = KeybindList.ForSingle(SButton.DPadLeft);
        public KeybindList NextTab { get; set; } = KeybindList.ForSingle(SButton.DPadRight);
    }
}