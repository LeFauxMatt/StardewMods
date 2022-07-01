#nullable disable

namespace XSPlus.Features;

using Common.UI;
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
    private readonly PerScreen<ClickableTextureComponent> _configureButton = new();
    private readonly PerScreen<ItemGrabMenu> _returnMenu = new();
    private ItemGrabMenuChanged _itemGrabMenuChanged;
    private ItemGrabMenuSideButtons _itemGrabMenuSideButtons;
    private ModConfigService _modConfig;
    private StashToChestFeature _stashToChest;

    private CategorizeChestFeature(ServiceLocator serviceLocator)
        : base("CategorizeChest", serviceLocator)
    {
        // Dependencies
        this.AddDependency<ModConfigService>(service => this._modConfig = service as ModConfigService);
        this.AddDependency<ItemGrabMenuChanged>(service => this._itemGrabMenuChanged = service as ItemGrabMenuChanged);
        this.AddDependency<ItemGrabMenuSideButtons>(service => this._itemGrabMenuSideButtons = service as ItemGrabMenuSideButtons);
        this.AddDependency<StashToChestFeature>(service => this._stashToChest = service as StashToChestFeature);
    }

    /// <inheritdoc />
    public override void Activate()
    {
        // Events
        this._itemGrabMenuChanged.AddHandler(this.OnItemGrabMenuChanged);
        this._itemGrabMenuSideButtons.AddHandler(this.OnSideButtonPressed);
        this.Helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
        this.Helper.Events.Input.ButtonPressed += this.OnButtonPressed;
    }

    /// <inheritdoc />
    public override void Deactivate()
    {
        // Events
        this._itemGrabMenuChanged.RemoveHandler(this.OnItemGrabMenuChanged);
        this._itemGrabMenuSideButtons.RemoveHandler(this.OnSideButtonPressed);
        this.Helper.Events.GameLoop.GameLaunched -= this.OnGameLaunched;
        this.Helper.Events.Input.ButtonPressed -= this.OnButtonPressed;
    }

    private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
    {
        this._configureButton.Value = new(
            new(0, 0, 64, 64),
            this.Helper.Content.Load<Texture2D>("assets/configure.png"),
            Rectangle.Empty,
            Game1.pixelZoom)
        {
            name = "Configure",
        };
    }

    private void OnItemGrabMenuChanged(object sender, ItemGrabMenuChangedEventArgs e)
    {
        if (e.ItemGrabMenu is null || e.Chest is null || !this.IsEnabledForItem(e.Chest))
        {
            return;
        }

        this._itemGrabMenuSideButtons.AddButton(this._configureButton.Value);
        this._returnMenu.Value = e.ItemGrabMenu;
        this._chest.Value = e.Chest;
    }

    private bool OnSideButtonPressed(MenuComponentPressedEventArgs e)
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