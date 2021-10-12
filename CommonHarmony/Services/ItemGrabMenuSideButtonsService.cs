namespace CommonHarmony.Services
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Common.Services;
    using Enums;
    using HarmonyLib;
    using Interfaces;
    using Models;
    using StardewModdingAPI;
    using StardewModdingAPI.Events;
    using StardewModdingAPI.Utilities;
    using StardewValley;
    using StardewValley.Menus;

    /// <inheritdoc cref="BaseService" />
    internal class ItemGrabMenuSideButtonsService : BaseService, IEventHandlerService<Func<SideButtonPressedEventArgs, bool>>
    {
        private static ItemGrabMenuSideButtonsService Instance;
        private readonly PerScreen<IList<Func<SideButtonPressedEventArgs, bool>>> _buttonPressedHandlers = new(() => new List<Func<SideButtonPressedEventArgs, bool>>());
        private readonly PerScreen<Dictionary<ClickableTextureComponent, SideButton>> _buttons = new(() => new());
        private readonly Func<string, Translation> _getTranslation;
        private readonly PerScreen<HashSet<SideButton>> _hideButtons = new(() => new());
        private readonly PerScreen<string> _hoverText = new();
        private readonly PerScreen<ItemGrabMenuEventArgs> _menu = new();
        private readonly PerScreen<Dictionary<ClickableTextureComponent, SideButton>> _sideButtons = new(() => new());
        private readonly Action<SButton> _suppress;

        private ItemGrabMenuSideButtonsService(ServiceManager serviceManager)
            : base("ItemGrabMenuSideButtons")
        {
            ItemGrabMenuSideButtonsService.Instance ??= this;

            // Init
            this._suppress = serviceManager.Helper.Input.Suppress;
            this._getTranslation = serviceManager.Helper.Translation.Get;

            // Dependencies
            this.AddDependency<ItemGrabMenuChangedService>(
                service =>
                {
                    var itemGrabMenuChanged = service as ItemGrabMenuChangedService;
                    itemGrabMenuChanged?.AddHandler(this.OnItemGrabMenuChangedBefore);
                    itemGrabMenuChanged?.AddHandler(this.OnItemGrabMenuChangedAfter);
                });

            this.AddDependency<RenderedActiveMenuService>(service => (service as RenderedActiveMenuService)?.AddHandler(this.OnRenderedActiveMenu));
            this.AddDependency<HarmonyService>(
                service =>
                {
                    var harmony = service as HarmonyService;
                    harmony?.AddPatch(
                        this.ServiceName,
                        AccessTools.Method(typeof(ItemGrabMenu), nameof(ItemGrabMenu.RepositionSideButtons)),
                        typeof(ItemGrabMenuSideButtonsService),
                        nameof(ItemGrabMenuSideButtonsService.ItemGrabMenu_RepositionSideButtons_prefix));

                    harmony?.ApplyPatches(this.ServiceName);
                });

            // Events
            serviceManager.Helper.Events.Input.ButtonPressed += this.OnButtonPressed;
            serviceManager.Helper.Events.Input.CursorMoved += this.OnCursorMoved;
        }

        /// <inheritdoc />
        public void AddHandler(Func<SideButtonPressedEventArgs, bool> eventHandler)
        {
            this._buttonPressedHandlers.Value.Add(eventHandler);
        }

        /// <inheritdoc />
        public void RemoveHandler(Func<SideButtonPressedEventArgs, bool> eventHandler)
        {
            this._buttonPressedHandlers.Value.Remove(eventHandler);
        }

        public void AddButton(ClickableTextureComponent cc)
        {
            if (this._menu.Value is null)
            {
                return;
            }

            this._buttons.Value.Add(cc, SideButton.Custom);
        }

        public void HideButton(SideButton button)
        {
            if (this._menu.Value is null)
            {
                return;
            }

            this._hideButtons.Value.Add(button);
        }

        [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Naming is determined by Harmony.")]
        private static bool ItemGrabMenu_RepositionSideButtons_prefix(ItemGrabMenu __instance)
        {
            if (ItemGrabMenuSideButtonsService.Instance._buttons.Value.Count == 0)
            {
                return true;
            }

            ItemGrabMenuSideButtonsService.Instance._sideButtons.Value.Clear();
            var sideButtons = new List<ClickableTextureComponent>();
            foreach (var sideButton in ItemGrabMenuSideButtonsService.Instance._buttons.Value)
            {
                ItemGrabMenuSideButtonsService.Instance._sideButtons.Value.Add(sideButton.Key, sideButton.Value);
                sideButtons.Add(sideButton.Key);
            }

            foreach (SideButton vanillaButton in Enum.GetValues(typeof(SideButton)))
            {
                if (ItemGrabMenuSideButtonsService.Instance._hideButtons.Value.Contains(vanillaButton))
                {
                    ItemGrabMenuSideButtonsService.HideButton(__instance, vanillaButton);
                    continue;
                }

                var button = vanillaButton switch
                {
                    SideButton.OrganizeButton => __instance.organizeButton,
                    SideButton.FillStacksButton => __instance.fillStacksButton,
                    SideButton.ColorPickerToggleButton => __instance.colorPickerToggleButton,
                    SideButton.SpecialButton => __instance.specialButton,
                    SideButton.JunimoNoteIcon => __instance.junimoNoteIcon,
                    _ => null,
                };

                if (button is not null)
                {
                    ItemGrabMenuSideButtonsService.Instance._sideButtons.Value.Add(button, vanillaButton);
                    sideButtons.Add(button);
                }
            }

            var stepSize = sideButtons.Count >= 4 ? 72 : 80;
            for (var i = 0; i < sideButtons.Count; i++)
            {
                var button = sideButtons[i];
                if (i > 0 && sideButtons.Count > 1)
                {
                    button.downNeighborID = sideButtons[i - 1].myID;
                }

                if (i < sideButtons.Count - 1 && sideButtons.Count > 1)
                {
                    button.upNeighborID = sideButtons[i + 1].myID;
                }

                button.bounds.X = __instance.xPositionOnScreen + __instance.width;
                button.bounds.Y = __instance.yPositionOnScreen + __instance.height / 3 - 64 - stepSize * i;
            }

            return false;
        }

        private static void HideButton(ItemGrabMenu menu, SideButton button)
        {
            switch (button)
            {
                case SideButton.OrganizeButton:
                    menu.organizeButton = null;
                    break;
                case SideButton.FillStacksButton:
                    menu.fillStacksButton = null;
                    break;
                case SideButton.ColorPickerToggleButton:
                    menu.colorPickerToggleButton = null;
                    break;
                case SideButton.SpecialButton:
                    menu.specialButton = null;
                    break;
                case SideButton.JunimoNoteIcon:
                    menu.junimoNoteIcon = null;
                    break;
                case SideButton.Custom:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(button), button, null);
            }
        }

        [HandlerPriority(HandlerPriority.High)]
        private void OnItemGrabMenuChangedBefore(object sender, ItemGrabMenuEventArgs e)
        {
            if (e.ItemGrabMenu is null || e.Chest is null)
            {
                this._menu.Value = null;
                return;
            }

            this._buttons.Value.Clear();
            this._hideButtons.Value.Clear();
            this._menu.Value = e;
        }

        [HandlerPriority(HandlerPriority.Low)]
        private void OnItemGrabMenuChangedAfter(object sender, ItemGrabMenuEventArgs e)
        {
            if (this._menu.Value is null || this._menu.Value.ScreenId != Context.ScreenId)
            {
                return;
            }

            this._menu.Value.ItemGrabMenu.RepositionSideButtons();
            this._menu.Value.ItemGrabMenu.SetupBorderNeighbors();
        }

        private void OnRenderedActiveMenu(object sender, RenderedActiveMenuEventArgs e)
        {
            if (this._menu.Value is null || this._menu.Value.ScreenId != Context.ScreenId)
            {
                return;
            }

            // Draw custom buttons
            foreach (var button in this._buttons.Value.Where(button => button.Value is SideButton.Custom).Select(button => button.Key))
            {
                button.draw(e.SpriteBatch);
            }

            // Add hover text to menu
            if (string.IsNullOrWhiteSpace(this._menu.Value.ItemGrabMenu.hoverText) && !string.IsNullOrWhiteSpace(this._hoverText.Value))
            {
                this._menu.Value.ItemGrabMenu.hoverText = this._hoverText.Value;
            }
        }

        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (this._menu.Value is null || this._menu.Value.ScreenId != Context.ScreenId || e.Button != SButton.MouseLeft)
            {
                return;
            }

            var point = Game1.getMousePosition(true);
            var button = this._sideButtons.Value.FirstOrDefault(button => button.Key.containsPoint(point.X, point.Y));
            if (button.Key is not null)
            {
                var eventArgs = new SideButtonPressedEventArgs(button.Key, button.Value);
                Game1.playSound("drumkit6");
                if (this._buttonPressedHandlers.Value.Any(handler => handler(eventArgs)))
                {
                    this._suppress(SButton.MouseLeft);
                }
            }
        }

        private void OnCursorMoved(object sender, CursorMovedEventArgs e)
        {
            if (this._menu.Value is null || this._menu.Value.ScreenId != Context.ScreenId)
            {
                return;
            }

            var point = Game1.getMousePosition(true);
            this._hoverText.Value = string.Empty;
            foreach (var button in this._buttons.Value.Where(button => button.Value is SideButton.Custom).Select(button => button.Key))
            {
                button.tryHover(point.X, point.Y, 0.25f);
                if (button.containsPoint(point.X, point.Y))
                {
                    this._hoverText.Value = this._getTranslation($"button.{button.name}.name");
                }
            }
        }
    }
}