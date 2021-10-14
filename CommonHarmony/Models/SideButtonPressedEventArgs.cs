namespace CommonHarmony.Models
{
    using Enums;
    using StardewValley.Menus;

    internal record SideButtonPressedEventArgs
    {
        public SideButtonPressedEventArgs(ClickableTextureComponent cc, ButtonType type)
        {
            this.Button = cc;
            this.Type = type;
        }

        public ClickableTextureComponent Button { get; }

        public ButtonType Type { get; }
    }
}