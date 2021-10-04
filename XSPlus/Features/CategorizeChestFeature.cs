namespace XSPlus.Features
{
    using System.Threading.Tasks;
    using Common.Helpers;
    using Common.UI;
    using CommonHarmony.Models;
    using CommonHarmony.Services;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Services;
    using StardewModdingAPI.Utilities;
    using StardewValley;
    using StardewValley.Menus;
    using StardewValley.Objects;

    /// <inheritdoc />
    internal class CategorizeChestFeature : BaseFeature
    {
        private readonly PerScreen<Chest> _chest = new();
        private readonly PerScreen<ClickableTextureComponent> _configButton = new();
        private readonly ItemGrabMenuChangedService _itemGrabMenuChangedService;
        private readonly ItemGrabMenuSideButtonsService _itemGrabMenuSideButtonsService;
        private readonly PerScreen<ItemGrabMenu> _returnMenu = new();

        private CategorizeChestFeature(
            ModConfigService modConfigService,
            ItemGrabMenuChangedService itemGrabMenuChangedService,
            ItemGrabMenuSideButtonsService itemGrabMenuSideButtonsService)
            : base("CategorizeChest", modConfigService)
        {
            this._configButton.Value = new(
                new(0, 0, 64, 64),
                Content.FromMod<Texture2D>("assets/configure.png"),
                Rectangle.Empty,
                Game1.pixelZoom)
            {
                name = "Configure",
            };

            this._itemGrabMenuChangedService = itemGrabMenuChangedService;
            this._itemGrabMenuSideButtonsService = itemGrabMenuSideButtonsService;
        }

        /// <summary>
        ///     Gets or sets the instance of <see cref="CategorizeChestFeature" />.
        /// </summary>
        private static CategorizeChestFeature Instance { get; set; }

        /// <summary>
        ///     Returns and creates if needed an instance of the <see cref="CategorizeChestFeature" /> class.
        /// </summary>
        /// <param name="serviceManager">Service manager to request shared services.</param>
        /// <returns>Returns an instance of the <see cref="CategorizeChestFeature" /> class.</returns>
        public static async Task<CategorizeChestFeature> Create(ServiceManager serviceManager)
        {
            return CategorizeChestFeature.Instance ??= new(
                await serviceManager.Get<ModConfigService>(),
                await serviceManager.Get<ItemGrabMenuChangedService>(),
                await serviceManager.Get<ItemGrabMenuSideButtonsService>());
        }

        /// <inheritdoc />
        public override void Activate()
        {
            // Events
            this._itemGrabMenuChangedService.AddHandler(this.OnItemGrabMenuChanged);
            this._itemGrabMenuSideButtonsService.AddHandler(this.OnSideButtonPressed);
        }

        /// <inheritdoc />
        public override void Deactivate()
        {
            // Events
            this._itemGrabMenuChangedService.RemoveHandler(this.OnItemGrabMenuChanged);
            this._itemGrabMenuSideButtonsService.RemoveHandler(this.OnSideButtonPressed);
        }

        private void OnItemGrabMenuChanged(object sender, ItemGrabMenuEventArgs e)
        {
            if (e.ItemGrabMenu is null || e.Chest is null || !this.IsEnabledForItem(e.Chest))
            {
                return;
            }

            this._itemGrabMenuSideButtonsService.AddButton(this._configButton.Value);
            this._returnMenu.Value = e.ItemGrabMenu;
            this._chest.Value = e.Chest;
        }

        private void OnSideButtonPressed(object sender, SideButtonPressed e)
        {
            if (e.Button.name == "Configure")
            {
                var filterItems = this._chest.Value.GetFilterItems();
                Game1.activeClickableMenu = new ItemSelectionMenu(
                    string.Empty,
                    this.ReturnToMenu,
                    filterItems,
                    this._chest.Value.SetFilterItems);
            }
        }

        private void ReturnToMenu()
        {
            Game1.activeClickableMenu = this._returnMenu.Value;
        }
    }
}