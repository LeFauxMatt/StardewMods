namespace XSPlus.Features
{
    using System.Diagnostics.CodeAnalysis;
    using Common.UI;
    using HarmonyLib;
    using Microsoft.Xna.Framework;
    using Models;
    using Services;
    using StardewModdingAPI;
    using StardewModdingAPI.Events;
    using StardewModdingAPI.Utilities;
    using StardewValley;
    using StardewValley.Menus;
    using StardewValley.Objects;

    /// <inheritdoc />
    internal class ColorPickerFeature : BaseFeature
    {
        // TODO: Add toggle button
        private const int Width = 58;
        private const int Height = 558;
        private static ColorPickerFeature Instance = null!;
        private readonly IContentHelper _contentHelper;
        private readonly ItemGrabMenuConstructedService _itemGrabMenuConstructedService;
        private readonly ItemGrabMenuChangedService _itemGrabMenuChangedService;
        private readonly RenderedActiveMenuService _renderedActiveMenuService;
        private readonly PerScreen<Chest> _fakeChest = new();
        private readonly PerScreen<Chest> _chest = new();
        private readonly PerScreen<ItemGrabMenu?> _menu = new();
        private readonly PerScreen<bool> _attached = new();
        private readonly PerScreen<HSLSlider> _hslSlider = new();

        /// <summary>Initializes a new instance of the <see cref="ColorPickerFeature"/> class.</summary>
        /// <param name="contentHelper">Provides an API for loading content assets.</param>
        /// <param name="itemGrabMenuConstructedService">Service to handle creation/invocation of ItemGrabMenuConstructed event.</param>
        /// <param name="itemGrabMenuChangedService">Service to handle creation/invocation of ItemGrabMenuChanged event.</param>
        /// <param name="renderedActiveMenuService">Service to handle creation/invocation of RenderedActiveMenu event.</param>
        internal ColorPickerFeature(
            IContentHelper contentHelper,
            ItemGrabMenuConstructedService itemGrabMenuConstructedService,
            ItemGrabMenuChangedService itemGrabMenuChangedService,
            RenderedActiveMenuService renderedActiveMenuService)
            : base("ColorPicker")
        {
            ColorPickerFeature.Instance = this;
            this._contentHelper = contentHelper;
            this._itemGrabMenuConstructedService = itemGrabMenuConstructedService;
            this._itemGrabMenuChangedService = itemGrabMenuChangedService;
            this._renderedActiveMenuService = renderedActiveMenuService;
        }

        /// <inheritdoc/>
        public override void Activate(IModEvents modEvents, Harmony harmony)
        {
            // Events
            this._itemGrabMenuConstructedService.AddHandler(this.OnItemGrabMenuConstructedEvent);
            this._itemGrabMenuChangedService.AddHandler(this.OnItemGrabMenuChanged);
            this._renderedActiveMenuService.AddHandler(this.OnRenderedActiveMenu);
            modEvents.GameLoop.GameLaunched += this.OnGameLaunched;
            modEvents.Input.ButtonPressed += this.OnButtonPressed;
            modEvents.Input.ButtonReleased += this.OnButtonReleased;
            modEvents.Input.CursorMoved += this.OnCursorMoved;
            modEvents.Input.MouseWheelScrolled += this.OnMouseWheelScrolled;

            // Patches
            harmony.Patch(
                original: AccessTools.Method(typeof(ItemGrabMenu), nameof(ItemGrabMenu.setSourceItem)),
                postfix: new HarmonyMethod(typeof(ColorPickerFeature), nameof(ColorPickerFeature.ItemGrabMenu_setSourceItem_postfix)));
        }

        /// <inheritdoc/>
        public override void Deactivate(IModEvents modEvents, Harmony harmony)
        {
            // Events
            this._itemGrabMenuConstructedService.RemoveHandler(this.OnItemGrabMenuConstructedEvent);
            this._itemGrabMenuChangedService.RemoveHandler(this.OnItemGrabMenuChanged);
            this._renderedActiveMenuService.RemoveHandler(this.OnRenderedActiveMenu);
            modEvents.GameLoop.GameLaunched -= this.OnGameLaunched;
            modEvents.Input.ButtonPressed -= this.OnButtonPressed;
            modEvents.Input.ButtonReleased -= this.OnButtonReleased;
            modEvents.Input.CursorMoved -= this.OnCursorMoved;
            modEvents.Input.MouseWheelScrolled -= this.OnMouseWheelScrolled;

            // Patches
            harmony.Unpatch(
                original: AccessTools.Method(typeof(ItemGrabMenu), nameof(ItemGrabMenu.setSourceItem)),
                patch: AccessTools.Method(typeof(ColorPickerFeature), nameof(ColorPickerFeature.ItemGrabMenu_setSourceItem_postfix)));
        }

        [SuppressMessage("ReSharper", "SA1313", Justification = "Naming is determined by Harmony.")]
        [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Naming is determined by Harmony.")]
        private static void ItemGrabMenu_setSourceItem_postfix(ItemGrabMenu __instance)
        {
            if (__instance.context is not Chest chest || !ColorPickerFeature.Instance.IsEnabledForItem(chest))
            {
                return;
            }

            __instance.chestColorPicker = null;
            __instance.colorPickerToggleButton = null;
            __instance.discreteColorPickerCC = null;
            __instance.RepositionSideButtons();
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            this._hslSlider.Value = new HSLSlider(this._contentHelper);
        }

        private void OnItemGrabMenuConstructedEvent(object sender, ItemGrabMenuEventArgs e)
        {
            if (e.ItemGrabMenu is null || e.Chest is null || !this.IsEnabledForItem(e.Chest))
            {
                return;
            }

            // Remove vanilla color picker
            e.ItemGrabMenu.colorPickerToggleButton = null;
            e.ItemGrabMenu.chestColorPicker = null;
            e.ItemGrabMenu.discreteColorPickerCC = null;
            e.ItemGrabMenu.SetupBorderNeighbors();
            e.ItemGrabMenu.RepositionSideButtons();
        }

        private void OnItemGrabMenuChanged(object sender, ItemGrabMenuEventArgs e)
        {
            if (e.ItemGrabMenu is null || e.Chest is null || !this.IsEnabledForItem(e.Chest))
            {
                this._attached.Value = false;
                this._menu.Value = null;
                return;
            }

            this._attached.Value = true;
            this._menu.Value = e.ItemGrabMenu;
            this._chest.Value = e.Chest;
            this._fakeChest.Value = new Chest(true, e.Chest.ParentSheetIndex)
            {
                Name = e.Chest.Name,
                lidFrameCount = { Value = e.Chest.lidFrameCount.Value },
                playerChoiceColor = { Value = e.Chest.playerChoiceColor.Value },
            };
            foreach (SerializableDictionary<string, string> modData in e.Chest.modData)
            {
                this._fakeChest.Value.modData.CopyFrom(modData);
            }

            this._fakeChest.Value.resetLidFrame();

            this._hslSlider.Value.Area = new Rectangle(e.ItemGrabMenu.xPositionOnScreen + e.ItemGrabMenu.width + 96 + (IClickableMenu.borderWidth / 2), e.ItemGrabMenu.yPositionOnScreen - 56 + (IClickableMenu.borderWidth / 2), ColorPickerFeature.Width, ColorPickerFeature.Height);
            this._hslSlider.Value.Color = e.Chest.playerChoiceColor.Value;
        }

        private void OnRenderedActiveMenu(object sender, RenderedActiveMenuEventArgs e)
        {
            if (!this._attached.Value)
            {
                return;
            }

            int x = this._hslSlider.Value.Area.Left;
            int y = this._hslSlider.Value.Area.Top - (IClickableMenu.borderWidth / 2) - Game1.tileSize;
            this._fakeChest.Value.draw(e.SpriteBatch, x, y, 1f, true);
            this._hslSlider.Value.Draw(e.SpriteBatch);
        }

        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!this._attached.Value || e.Button != SButton.MouseLeft)
            {
                return;
            }

            if (this._hslSlider.Value.MouseLeftButtonPressed())
            {
                Game1.playSound("coin");
                this._fakeChest.Value.playerChoiceColor.Value = this._hslSlider.Value.Color;
            }
        }

        private void OnButtonReleased(object sender, ButtonReleasedEventArgs e)
        {
            if (!this._attached.Value || e.Button != SButton.MouseLeft)
            {
                return;
            }

            if (this._hslSlider.Value.MouseLeftButtonReleased())
            {
                this._fakeChest.Value.playerChoiceColor.Value = this._hslSlider.Value.Color;
                this._chest.Value.playerChoiceColor.Value = this._fakeChest.Value.playerChoiceColor.Value;
            }
        }

        private void OnCursorMoved(object sender, CursorMovedEventArgs e)
        {
            if (!this._attached.Value)
            {
                return;
            }

            if (this._hslSlider.Value.MouseHover())
            {
                this._fakeChest.Value.playerChoiceColor.Value = this._hslSlider.Value.Color;
            }
        }

        private void OnMouseWheelScrolled(object sender, MouseWheelScrolledEventArgs e)
        {
            if (!this._attached.Value)
            {
                return;
            }

            if (this._hslSlider.Value.MouseWheelScroll(e.Delta))
            {
                this._fakeChest.Value.playerChoiceColor.Value = this._hslSlider.Value.Color;
                this._chest.Value.playerChoiceColor.Value = this._fakeChest.Value.playerChoiceColor.Value;
            }
        }
    }
}