namespace Common.Helpers
{
    using StardewModdingAPI;
    using StardewModdingAPI.Utilities;

    internal static class Input
    {
        /// <inheritdoc cref="IInputHelper" />
        private static IInputHelper Helper { get; set; }

        /// <summary>Initializes the <see cref="Input" /> class.</summary>
        /// <param name="inputHelper">The instance of IInputHelper from the Mod.</param>
        public static void Init(IInputHelper inputHelper)
        {
            Input.Helper = inputHelper;
        }

        public static void Suppress(SButton button)
        {
            Input.Helper.Suppress(button);
        }

        public static void Suppress(KeybindList keybindList)
        {
            Input.Helper.SuppressActiveKeybinds(keybindList);
        }
    }
}