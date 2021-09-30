namespace XSPlus.Services
{
    using System;
    using System.Collections.Generic;
    using Common.Models;
    using Common.Services;
    using CommonHarmony.Services;
    using StardewModdingAPI.Utilities;
    using StardewValley.Menus;
    using StardewValley.Objects;

    /// <inheritdoc />
    internal class ItemGrabMenuSideButtonsService : BaseService
    {
        private static ItemGrabMenuSideButtonsService Instance;
        private readonly PerScreen<ItemGrabMenu> _menu = new();
        private readonly PerScreen<IList<ClickableTextureComponent>> _buttons = new() { Value = new List<ClickableTextureComponent>() };
        private readonly PerScreen<HashSet<VanillaButton>> _hideButtons = new() { Value = new HashSet<VanillaButton>() };

        private ItemGrabMenuSideButtonsService(
            ItemGrabMenuConstructedService itemGrabMenuConstructedService,
            ItemGrabMenuChangedService itemGrabMenuChangedService)
            : base("ItemGrabMenuSideButtons")
        {
            itemGrabMenuConstructedService.AddHandler(this.OnItemGrabMenuEvent);
            itemGrabMenuChangedService.AddHandler(this.OnItemGrabMenuEvent);
        }

        private event EventHandler<ItemGrabMenuEventArgs> ItemGrabMenuChanged;

        /// <summary>
        /// Default side buttons alongside the <see cref="ItemGrabMenu"/>
        /// </summary>
        public enum VanillaButton
        {
            OrganizeButton,
            FillStacksButton,
            ColorPickerToggleButton,
            SpecialButton,
            JunimoNoteIcon,
        }

        /// <summary>
        /// Returns and creates if needed an instance of the <see cref="ItemGrabMenuSideButtonsService"/> class.
        /// </summary>
        /// <param name="serviceManager">Service manager to request shared services.</param>
        /// <returns>Returns an instance of the <see cref="ItemGrabMenuSideButtonsService"/> class.</returns>
        public static ItemGrabMenuSideButtonsService GetSingleton(ServiceManager serviceManager)
        {
            var itemGrabMenuConstructedService = serviceManager.RequestService<ItemGrabMenuConstructedService>();
            var itemGrabMenuChangedService = serviceManager.RequestService<ItemGrabMenuChangedService>();
            return ItemGrabMenuSideButtonsService.Instance ??= new ItemGrabMenuSideButtonsService(itemGrabMenuConstructedService, itemGrabMenuChangedService);
        }

        public void HideButton(VanillaButton button)
        {
            this._hideButtons.Value.Add(button);
        }

        private void OnItemGrabMenuEvent(object sender, ItemGrabMenuEventArgs e)
        {
            if (e.ItemGrabMenu is not null && !ReferenceEquals(this._menu.Value, e.ItemGrabMenu))
            {
                this._menu.Value = e.ItemGrabMenu;
                this._buttons.Value.Clear();
                this._hideButtons.Value.Clear();
                this.InvokeAll(e.ItemGrabMenu, e.Chest);
                e.ItemGrabMenu.SetupBorderNeighbors();
                e.ItemGrabMenu.RepositionSideButtons();
            }
        }

        private void InvokeAll(ItemGrabMenu itemGrabMenu, Chest chest)
        {
            var eventArgs = new ItemGrabMenuEventArgs(itemGrabMenu, chest);
            this.ItemGrabMenuChanged?.Invoke(this, eventArgs);
        }
    }
}