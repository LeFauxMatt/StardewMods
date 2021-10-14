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
        private readonly PerScreen<List<SideButton>> _allButtons = new(() => new());
        private readonly IList<Func<SideButtonPressedEventArgs, bool>> _buttonPressedHandlers = new List<Func<SideButtonPressedEventArgs, bool>>();
        private readonly PerScreen<IList<SideButton>> _customButtons = new(() => new List<SideButton>());
        private readonly Func<string, Translation> _getTranslation;
        private readonly PerScreen<HashSet<ButtonType>> _hideButtons = new(() => new());
        private readonly PerScreen<string> _hoverText = new();
        private readonly PerScreen<ItemGrabMenuEventArgs> _menu = new();
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
            this._buttonPressedHandlers.Add(eventHandler);
        }

        /// <inheritdoc />
        public void RemoveHandler(Func<SideButtonPressedEventArgs, bool> eventHandler)
        {
            this._buttonPressedHandlers.Remove(eventHandler);
        }

        public void AddButton(ClickableTextureComponent cc)
        {
            if (this._menu.Value is null)
            {
                return;
            }

            if (!this._customButtons.Value.Contains(cc))
            {
                this._customButtons.Value.Add(cc);
            }
        }

        public void HideButton(ButtonType buttonType)
        {
            if (this._menu.Value is null)
            {
                return;
            }

            this._hideButtons.Value.Add(buttonType);
        }

        [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Naming is determined by Harmony.")]
        private static bool ItemGrabMenu_RepositionSideButtons_prefix(ItemGrabMenu __instance)
        {
            if (ItemGrabMenuSideButtonsService.Instance._customButtons.Value.Count == 0)
            {
                return true;
            }

            var sideButtons = ItemGrabMenuSideButtonsService.Instance._allButtons.Value;
            sideButtons.Clear();
            sideButtons.AddRange(ItemGrabMenuSideButtonsService.Instance._customButtons.Value);
            foreach (ButtonType buttonType in Enum.GetValues(typeof(ButtonType)))
            {
                if (ItemGrabMenuSideButtonsService.Instance._hideButtons.Value.Contains(buttonType))
                {
                    ItemGrabMenuSideButtonsService.HideButton(__instance, buttonType);
                    continue;
                }

                var button = buttonType switch
                {
                    ButtonType.OrganizeButton => __instance.organizeButton,
                    ButtonType.FillStacksButton => __instance.fillStacksButton,
                    ButtonType.ColorPickerToggleButton => __instance.colorPickerToggleButton,
                    ButtonType.SpecialButton => __instance.specialButton,
                    ButtonType.JunimoNoteIcon => __instance.junimoNoteIcon,
                    _ => null,
                };

                if (button is not null)
                {
                    sideButtons.Add(new(buttonType, button));
                }
            }

            var stepSize = sideButtons.Count >= 4 ? 72 : 80;
            for (var i = 0; i < sideButtons.Count; i++)
            {
                var button = sideButtons[i].Button;
                if (i > 0 && sideButtons.Count > 1)
                {
                    button.downNeighborID = sideButtons[i - 1].Button.myID;
                }

                if (i < sideButtons.Count - 1 && sideButtons.Count > 1)
                {
                    button.upNeighborID = sideButtons[i + 1].Button.myID;
                }

                button.bounds.X = __instance.xPositionOnScreen + __instance.width;
                button.bounds.Y = __instance.yPositionOnScreen + __instance.height / 3 - 64 - stepSize * i;
            }

            return false;
        }

        private static void HideButton(ItemGrabMenu menu, ButtonType buttonType)
        {
            switch (buttonType)
            {
                case ButtonType.OrganizeButton:
                    menu.organizeButton = null;
                    break;
                case ButtonType.FillStacksButton:
                    menu.fillStacksButton = null;
                    break;
                case ButtonType.ColorPickerToggleButton:
                    menu.colorPickerToggleButton = null;
                    break;
                case ButtonType.SpecialButton:
                    menu.specialButton = null;
                    break;
                case ButtonType.JunimoNoteIcon:
                    menu.junimoNoteIcon = null;
                    break;
                case ButtonType.Custom:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(buttonType), buttonType, null);
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

            this._customButtons.Value.Clear();
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
            foreach (var button in this._customButtons.Value)
            {
                button.Button.draw(e.SpriteBatch);
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
            var button = this._allButtons.Value.FirstOrDefault(button => button.Button.containsPoint(point.X, point.Y));
            if (button is not null)
            {
                var eventArgs = new SideButtonPressedEventArgs(button.Button, button.ButtonType);
                Game1.playSound("drumkit6");
                if (this._buttonPressedHandlers.Any(handler => handler(eventArgs)))
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
            foreach (var button in this._customButtons.Value.Where(button => button.ButtonType is ButtonType.Custom).Select(button => button.Button))
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