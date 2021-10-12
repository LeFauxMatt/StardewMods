namespace XSPlus.Features
{
    using Common.Services;
    using Common.UI;
    using CommonHarmony.Models;
    using CommonHarmony.Services;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Services;
    using StardewModdingAPI;
    using StardewModdingAPI.Events;
    using StardewModdingAPI.Utilities;
    using StardewValley;
    using StardewValley.Menus;
    using StardewValley.Objects;

    /// <inheritdoc />
    internal class CategorizeChestFeature : BaseFeature
    {
        private readonly PerScreen<Chest> _chest = new();
        private readonly PerScreen<ItemGrabMenu> _returnMenu = new();
        private ItemGrabMenuChangedService _itemGrabMenuChanged;
        private ItemGrabMenuSideButtonsService _itemGrabMenuSideButtons;
        private ModConfigService _modConfig;
        private StashToChestFeature _stashToChest;

        private CategorizeChestFeature(ServiceManager serviceManager)
            : base("CategorizeChest", serviceManager)
        {
            // Dependencies
            this.AddDependency<ModConfigService>(service => this._modConfig = service as ModConfigService);
            this.AddDependency<ItemGrabMenuChangedService>(service => this._itemGrabMenuChanged = service as ItemGrabMenuChangedService);
            this.AddDependency<ItemGrabMenuSideButtonsService>(service => this._itemGrabMenuSideButtons = service as ItemGrabMenuSideButtonsService);
            this.AddDependency<StashToChestFeature>(service => this._stashToChest = service as StashToChestFeature);
        }

        /// <inheritdoc />
        public override void Activate()
        {
            // Events
            this._itemGrabMenuChanged.AddHandler(this.OnItemGrabMenuChanged);
            this._itemGrabMenuSideButtons.AddHandler(this.OnSideButtonPressed);
            this.Helper.Events.Input.ButtonPressed += this.OnButtonPressed;
        }

        /// <inheritdoc />
        public override void Deactivate()
        {
            // Events
            this._itemGrabMenuChanged.RemoveHandler(this.OnItemGrabMenuChanged);
            this._itemGrabMenuSideButtons.RemoveHandler(this.OnSideButtonPressed);
            this.Helper.Events.Input.ButtonPressed -= this.OnButtonPressed;
        }

        private void OnItemGrabMenuChanged(object sender, ItemGrabMenuEventArgs e)
        {
            if (e.ItemGrabMenu is null || e.Chest is null || !this.IsEnabledForItem(e.Chest))
            {
                return;
            }

            this._itemGrabMenuSideButtons.AddButton(
                new(
                    new(0, 0, 64, 64),
                    this.Helper.Content.Load<Texture2D>("assets/configure.png"),
                    Rectangle.Empty,
                    Game1.pixelZoom)
                {
                    name = "Configure",
                });

            this._returnMenu.Value = e.ItemGrabMenu;
            this._chest.Value = e.Chest;
        }

        private bool OnSideButtonPressed(SideButtonPressedEventArgs e)
        {
            if (e.Button.name != "Configure")
            {
                return false;
            }

            var filterItems = this._chest.Value.GetFilterItems();
            Game1.activeClickableMenu = new ItemSelectionMenu(
                this._modConfig.ModConfig.SearchTagSymbol,
                this.ReturnToMenu,
                filterItems,
                this._chest.Value.SetFilterItems,
                this.IsModifierDown);

            return true;
        }

        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (Game1.activeClickableMenu is not ItemSelectionMenu itemSelectionMenu)
            {
                return;
            }

            switch (e.Button)
            {
                case SButton.Escape when itemSelectionMenu.readyToClose():
                    itemSelectionMenu.exitThisMenu();
                    this.Helper.Input.Suppress(e.Button);
                    return;
                case SButton.Escape:
                    this.Helper.Input.Suppress(e.Button);
                    return;
                case SButton.MouseLeft when itemSelectionMenu.LeftClick(Game1.getMousePosition(true)):
                    this.Helper.Input.Suppress(e.Button);
                    break;
                case SButton.MouseRight when itemSelectionMenu.RightClick(Game1.getMousePosition(true)):
                    this.Helper.Input.Suppress(e.Button);
                    break;
            }
        }

        private bool IsModifierDown()
        {
            return this.Helper.Input.IsDown(SButton.LeftShift) || this.Helper.Input.IsDown(SButton.RightShift);
        }

        private void ReturnToMenu()
        {
            this._stashToChest.ResetCachedChests(true, true);
            Game1.activeClickableMenu = this._returnMenu.Value;
        }
    }
}