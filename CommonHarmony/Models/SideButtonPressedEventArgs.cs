namespace CommonHarmony.Models
{
    using StardewValley.Menus;

    internal record SideButtonPressed
    {
        public SideButtonPressed(ClickableTextureComponent cc)
        {
            this.Button = cc;
        }

        public ClickableTextureComponent Button { get; }
    }
}