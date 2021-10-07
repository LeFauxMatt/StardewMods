namespace CommonHarmony.Services
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading.Tasks;
    using Common.Helpers;
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
        private readonly PerScreen<IList<Func<SideButtonPressedEventArgs, bool>>> _buttonPressedHandlers = new()
        {
            Value = new List<Func<SideButtonPressedEventArgs, bool>>(),
        };
        private readonly PerScreen<Dictionary<ClickableTextureComponent, SideButton>> _buttons = new()
        {
            Value = new(),
        };
        private readonly PerScreen<HashSet<SideButton>> _hideButtons = new()
        {
            Value = new(),
        };
        private readonly PerScreen<string> _hoverText = new();
        private readonly PerScreen<ItemGrabMenuEventArgs> _menu = new();

        private ItemGrabMenuSideButtonsService(
            ItemGrabMenuChangedService itemGrabMenuChangedService,
            RenderedActiveMenuService renderedActiveMenuService)
            : base("ItemGrabMenuSideButtons")
        {
            // Events
            itemGrabMenuChangedService.AddHandler(this.OnItemGrabMenuChangedBefore);
            itemGrabMenuChangedService.AddHandler(this.OnItemGrabMenuChangedAfter);
            renderedActiveMenuService.AddHandler(this.OnRenderedActiveMenu);
            Events.Input.ButtonPressed += this.OnButtonPressed;
            Events.Input.CursorMoved += this.OnCursorMoved;

            // Patches
            Mixin.Prefix(
                AccessTools.Method(typeof(ItemGrabMenu), nameof(ItemGrabMenu.RepositionSideButtons)),
                typeof(ItemGrabMenuSideButtonsService),
                nameof(ItemGrabMenuSideButtonsService.ItemGrabMenu_RepositionSideButtons_prefix));
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

        /// <summary>
        ///     Returns and creates if needed an instance of the <see cref="ItemGrabMenuSideButtonsService" /> class.
        /// </summary>
        /// <param name="serviceManager">Service manager to request shared services.</param>
        /// <returns>Returns an instance of the <see cref="ItemGrabMenuSideButtonsService" /> class.</returns>
        public static async Task<ItemGrabMenuSideButtonsService> Create(ServiceManager serviceManager)
        {
            return ItemGrabMenuSideButtonsService.Instance ??= new(
                await serviceManager.Get<ItemGrabMenuChangedService>(),
                await serviceManager.Get<RenderedActiveMenuService>());
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

        [SuppressMessage("ReSharper", "SA1313", Justification = "Naming is determined by Harmony.")]
        [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Naming is determined by Harmony.")]
        private static bool ItemGrabMenu_RepositionSideButtons_prefix(ItemGrabMenu __instance)
        {
            if (ItemGrabMenuSideButtonsService.Instance._buttons.Value.Count == 0)
            {
                return true;
            }

            var sideButtons = new List<ClickableComponent>();
            foreach (var button in ItemGrabMenuSideButtonsService.Instance._buttons.Value)
            {
                if (ItemGrabMenuSideButtonsService.Instance._hideButtons.Value.Contains(button.Value))
                {
                    ItemGrabMenuSideButtonsService.HideButton(__instance, button.Value);
                }
                else
                {
                    sideButtons.Add(button.Key);
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

            this.ResetButtons(e.ItemGrabMenu);
            this._menu.Value = e;
        }

        [HandlerPriority(HandlerPriority.Low)]
        private void OnItemGrabMenuChangedAfter(object sender, ItemGrabMenuEventArgs e)
        {
            if (this._menu.Value is null || this._menu.Value.ScreenId != Context.ScreenId)
            {
                return;
            }

            this._menu.Value.ItemGrabMenu.SetupBorderNeighbors();
            this._menu.Value.ItemGrabMenu.RepositionSideButtons();
        }

        private void OnRenderedActiveMenu(object sender, RenderedActiveMenuEventArgs e)
        {
            if (this._menu.Value is null || this._menu.Value.ScreenId != Context.ScreenId)
            {
                return;
            }

            // Draw buttons
            foreach (var button in this._buttons.Value.Keys)
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
            var button = this._buttons.Value.FirstOrDefault(button => button.Key.containsPoint(point.X, point.Y));
            if (button.Key is not null)
            {
                var eventArgs = new SideButtonPressedEventArgs(button.Key, button.Value);
                Game1.playSound("drumkit6");
                foreach (var handler in this._buttonPressedHandlers.Value)
                {
                    if (handler(eventArgs))
                    {
                        Input.Suppress(SButton.MouseLeft);
                        return;
                    }
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
            foreach (var button in this._buttons.Value.Keys)
            {
                button.tryHover(point.X, point.Y, 0.25f);
                if (button.containsPoint(point.X, point.Y))
                {
                    this._hoverText.Value = Translations.Get($"button.{button.name}.name");
                }
            }
        }

        private void ResetButtons(ItemGrabMenu menu)
        {
            this._buttons.Value.Clear();
            this._hideButtons.Value.Clear();
            foreach (SideButton vanillaButton in Enum.GetValues(typeof(SideButton)))
            {
                var button = vanillaButton switch
                {
                    SideButton.OrganizeButton => menu.organizeButton,
                    SideButton.FillStacksButton => menu.fillStacksButton,
                    SideButton.ColorPickerToggleButton => menu.colorPickerToggleButton,
                    SideButton.SpecialButton => menu.specialButton,
                    SideButton.JunimoNoteIcon => menu.junimoNoteIcon,
                    _ => null,
                };

                if (button is not null)
                {
                    this._buttons.Value.Add(button, vanillaButton);
                }
            }
        }
    }
}