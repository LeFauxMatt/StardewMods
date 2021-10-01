namespace XSPlus.Services
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Common.Interfaces;
    using Common.Models;
    using Common.Services;
    using CommonHarmony.Services;
    using HarmonyLib;
    using StardewModdingAPI.Utilities;
    using StardewValley.Menus;
    using StardewValley.Objects;

    /// <inheritdoc cref="Common.Services.BaseService" />
    internal class ItemGrabMenuSideButtonsService : BaseService, IEventHandlerService<EventHandler<ItemGrabMenuEventArgs>>
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
        private readonly PerScreen<IList<ClickableComponent>> _buttons = new()
        {
            Value = new List<ClickableComponent>(),
        };
        private readonly PerScreen<HashSet<VanillaButton>> _hideButtons = new()
        {
            Value = new HashSet<VanillaButton>(),
        };
        private readonly PerScreen<ItemGrabMenu> _menu = new();

        private ItemGrabMenuSideButtonsService(
            ItemGrabMenuConstructedService itemGrabMenuConstructedService,
            ItemGrabMenuChangedService itemGrabMenuChangedService)
            : base("ItemGrabMenuSideButtons")
        {
            // Events
            itemGrabMenuConstructedService.AddHandler(this.OnItemGrabMenuEvent);
            itemGrabMenuChangedService.AddHandler(this.OnItemGrabMenuEvent);

            // Patches
            Mixin.Prefix(
                AccessTools.Method(typeof(ItemGrabMenu), nameof(ItemGrabMenu.RepositionSideButtons)),
                typeof(ItemGrabMenuSideButtonsService),
                nameof(ItemGrabMenuSideButtonsService.ItemGrabMenu_RepositionSideButtons_prefix));
        }

        /// <inheritdoc />
        public void AddHandler(EventHandler<ItemGrabMenuEventArgs> eventHandler)
        {
            this.ItemGrabMenuChanged += eventHandler;
        }

        /// <inheritdoc />
        public void RemoveHandler(EventHandler<ItemGrabMenuEventArgs> eventHandler)
        {
            this.ItemGrabMenuChanged -= eventHandler;
        }

        private event EventHandler<ItemGrabMenuEventArgs> ItemGrabMenuChanged;

        /// <summary>
        ///     Returns and creates if needed an instance of the <see cref="ItemGrabMenuSideButtonsService" /> class.
        /// </summary>
        /// <param name="serviceManager">Service manager to request shared services.</param>
        /// <returns>Returns an instance of the <see cref="ItemGrabMenuSideButtonsService" /> class.</returns>
        public static ItemGrabMenuSideButtonsService GetSingleton(ServiceManager serviceManager)
        {
            var itemGrabMenuConstructedService = serviceManager.RequestService<ItemGrabMenuConstructedService>();
            var itemGrabMenuChangedService = serviceManager.RequestService<ItemGrabMenuChangedService>();
            return ItemGrabMenuSideButtonsService.Instance ??= new ItemGrabMenuSideButtonsService(itemGrabMenuConstructedService, itemGrabMenuChangedService);
        }

        public void AddButton(ClickableComponent clickableComponent)
        {
            this._buttons.Value.Add(clickableComponent);
        }

        public void HideButton(VanillaButton button)
        {
            this._hideButtons.Value.Add(button);
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
            if (__instance.organizeButton is not null)
            {
                sideButtons.Add(__instance.organizeButton);
            }

            if (__instance.fillStacksButton is not null)
            {
                sideButtons.Add(__instance.fillStacksButton);
            }

            if (__instance.colorPickerToggleButton is not null)
            {
                sideButtons.Add(__instance.colorPickerToggleButton);
            }

            if (__instance.specialButton is not null)
            {
                sideButtons.Add(__instance.specialButton);
            }

            if (__instance.junimoNoteIcon is not null)
            {
                sideButtons.Add(__instance.junimoNoteIcon);
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

        private void OnItemGrabMenuEvent(object sender, ItemGrabMenuEventArgs e)
        {
            if (e.ItemGrabMenu is not null && !ReferenceEquals(this._menu.Value, e.ItemGrabMenu))
            {
                this._menu.Value = e.ItemGrabMenu;
                this._buttons.Value.Clear();
                this._hideButtons.Value.Clear();
                this.InvokeAll(e.ItemGrabMenu, e.Chest);
                foreach (var hideButton in this._hideButtons.Value)
                {
                    switch (hideButton)
                    {
                        case VanillaButton.OrganizeButton:
                            e.ItemGrabMenu.organizeButton = null;
                            break;
                        case VanillaButton.FillStacksButton:
                            e.ItemGrabMenu.fillStacksButton = null;
                            break;
                        case VanillaButton.ColorPickerToggleButton:
                            e.ItemGrabMenu.colorPickerToggleButton = null;
                            break;
                        case VanillaButton.SpecialButton:
                            e.ItemGrabMenu.specialButton = null;
                            break;
                        case VanillaButton.JunimoNoteIcon:
                            e.ItemGrabMenu.junimoNoteIcon = null;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

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