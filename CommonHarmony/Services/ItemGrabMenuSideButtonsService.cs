namespace CommonHarmony.Services
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading.Tasks;
    using Common.Helpers;
    using Common.Interfaces;
    using HarmonyLib;
    using Models;
    using StardewModdingAPI;
    using StardewModdingAPI.Events;
    using StardewModdingAPI.Utilities;
    using StardewValley;
    using StardewValley.Menus;

    /// <inheritdoc cref="BaseService" />
    internal class ItemGrabMenuSideButtonsService : BaseService, IEventHandlerService<EventHandler<SideButtonPressed>>
    {
        /// <summary>
        ///     Default side buttons alongside the <see cref="ItemGrabMenu" />
        /// </summary>
        public enum VanillaButton
        {
            /// <summary>The Organize Button.</summary>
            OrganizeButton,

            /// <summary>The Fill Stacks Button.</summary>
            FillStacksButton,

            /// <summary>The Color Picker Toggle Button.</summary>
            ColorPickerToggleButton,

            /// <summary>The Special Button.</summary>
            SpecialButton,

            /// <summary>The Junimo Note Icon.</summary>
            JunimoNoteIcon,
        }
        private static ItemGrabMenuSideButtonsService Instance;
        private readonly PerScreen<IList<ClickableTextureComponent>> _buttons = new()
        {
            Value = new List<ClickableTextureComponent>(),
        };
        private readonly PerScreen<HashSet<VanillaButton>> _hideButtons = new()
        {
            Value = new(),
        };
        private readonly PerScreen<string> _hoverText = new();
        private readonly PerScreen<ItemGrabMenu> _lastMenu = new();
        private readonly PerScreen<ItemGrabMenuEventArgs> _menu = new();

        private ItemGrabMenuSideButtonsService(
            ItemGrabMenuChangedService itemGrabMenuChangedService,
            RenderedActiveMenuService renderedActiveMenuService)
            : base("ItemGrabMenuSideButtons")
        {
            // Events
            itemGrabMenuChangedService.AddHandler(this.OnItemGrabMenuChanged);
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
        public void AddHandler(EventHandler<SideButtonPressed> eventHandler)
        {
            this.OnSideButtonPressed += eventHandler;
        }

        /// <inheritdoc />
        public void RemoveHandler(EventHandler<SideButtonPressed> eventHandler)
        {
            this.OnSideButtonPressed -= eventHandler;
        }

        private event EventHandler<SideButtonPressed> OnSideButtonPressed;

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

            if (!ReferenceEquals(this._menu.Value.ItemGrabMenu, this._lastMenu.Value))
            {
                this._lastMenu.Value = this._menu.Value.ItemGrabMenu;
                this._buttons.Value.Clear();
                this._hideButtons.Value.Clear();
            }

            this._buttons.Value.Add(cc);
            this._menu.Value.ItemGrabMenu.SetupBorderNeighbors();
            this._menu.Value.ItemGrabMenu.RepositionSideButtons();
        }

        public void HideButton(VanillaButton button)
        {
            if (this._menu.Value is null)
            {
                return;
            }

            if (!ReferenceEquals(this._menu.Value.ItemGrabMenu, this._lastMenu.Value))
            {
                this._lastMenu.Value = this._menu.Value.ItemGrabMenu;
                this._buttons.Value.Clear();
                this._hideButtons.Value.Clear();
            }

            this._hideButtons.Value.Add(button);
            this._menu.Value.ItemGrabMenu.SetupBorderNeighbors();
            this._menu.Value.ItemGrabMenu.RepositionSideButtons();
        }

        [SuppressMessage("ReSharper", "SA1313", Justification = "Naming is determined by Harmony.")]
        [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Naming is determined by Harmony.")]
        private static bool ItemGrabMenu_RepositionSideButtons_prefix(ItemGrabMenu __instance)
        {
            if (ItemGrabMenuSideButtonsService.Instance._hideButtons.Value.Count == 0 && ItemGrabMenuSideButtonsService.Instance._buttons.Value.Count == 0)
            {
                return true;
            }

            var sideButtons = new List<ClickableComponent>();
            foreach (VanillaButton vanillaButton in Enum.GetValues(typeof(VanillaButton)))
            {
                if (ItemGrabMenuSideButtonsService.Instance._hideButtons.Value.Contains(vanillaButton))
                {
                    ItemGrabMenuSideButtonsService.HideButton(__instance, vanillaButton);
                }
                else
                {
                    var button = ItemGrabMenuSideButtonsService.GetButton(__instance, vanillaButton);
                    if (button is not null)
                    {
                        sideButtons.Add(button);
                    }
                }
            }

            sideButtons.AddRange(ItemGrabMenuSideButtonsService.Instance._buttons.Value);

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

        private static void HideButton(ItemGrabMenu menu, VanillaButton button)
        {
            switch (button)
            {
                case VanillaButton.OrganizeButton:
                    menu.organizeButton = null;
                    break;
                case VanillaButton.FillStacksButton:
                    menu.fillStacksButton = null;
                    break;
                case VanillaButton.ColorPickerToggleButton:
                    menu.colorPickerToggleButton = null;
                    break;
                case VanillaButton.SpecialButton:
                    menu.specialButton = null;
                    break;
                case VanillaButton.JunimoNoteIcon:
                    menu.junimoNoteIcon = null;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(button), button, null);
            }
        }

        private static ClickableTextureComponent GetButton(ItemGrabMenu menu, VanillaButton button)
        {
            return button switch
            {
                VanillaButton.OrganizeButton => menu.organizeButton,
                VanillaButton.FillStacksButton => menu.fillStacksButton,
                VanillaButton.ColorPickerToggleButton => menu.colorPickerToggleButton,
                VanillaButton.SpecialButton => menu.specialButton,
                VanillaButton.JunimoNoteIcon => menu.junimoNoteIcon,
                _ => throw new ArgumentOutOfRangeException(nameof(button), button, null),
            };
        }

        private void OnItemGrabMenuChanged(object sender, ItemGrabMenuEventArgs e)
        {
            if (e.ItemGrabMenu is null || e.Chest is null)
            {
                this._menu.Value = null;
                return;
            }

            this._menu.Value = e;
        }

        private void OnRenderedActiveMenu(object sender, RenderedActiveMenuEventArgs e)
        {
            if (this._menu.Value is null || this._menu.Value.ScreenId != Context.ScreenId)
            {
                return;
            }

            // Draw buttons
            foreach (var button in this._buttons.Value)
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
            var button = this._buttons.Value.FirstOrDefault(button => button.containsPoint(point.X, point.Y));
            if (button is not null)
            {
                Game1.playSound("drumkit6");
                this.InvokeAll(button);
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
            foreach (var button in this._buttons.Value)
            {
                button.tryHover(point.X, point.Y, 0.25f);
                if (button.containsPoint(point.X, point.Y))
                {
                    this._hoverText.Value = Translations.Get($"button.{button.name}.name");
                }
            }
        }

        private void InvokeAll(ClickableTextureComponent cc)
        {
            var eventArgs = new SideButtonPressed(cc);
            this.OnSideButtonPressed?.Invoke(this, eventArgs);
        }
    }
}