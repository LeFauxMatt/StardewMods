namespace XSPlus.Features
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection.Emit;
    using Common.Extensions;
    using Common.Services;
    using CommonHarmony;
    using HarmonyLib;
    using Microsoft.Xna.Framework.Graphics;
    using Models;
    using Services;
    using StardewModdingAPI;
    using StardewModdingAPI.Events;
    using StardewModdingAPI.Utilities;
    using StardewValley;
    using StardewValley.Menus;
    using StardewValley.Objects;

    /// <inheritdoc />
    internal class ExpandedMenuFeature : FeatureWithParam<int>
    {
        private static readonly Type[] ItemGrabMenuConstructorParams = { typeof(IList<Item>), typeof(bool), typeof(bool), typeof(InventoryMenu.highlightThisItem), typeof(ItemGrabMenu.behaviorOnItemSelect), typeof(string), typeof(ItemGrabMenu.behaviorOnItemSelect), typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(int), typeof(Item), typeof(int), typeof(object) };
        private static readonly Type[] MenuWithInventoryDrawParams = { typeof(SpriteBatch), typeof(bool), typeof(bool), typeof(int), typeof(int), typeof(int) };
        private static ExpandedMenuFeature Instance = null!;
        private readonly IInputHelper _inputHelper;
        private readonly ModConfigService _modConfigService;
        private readonly ItemGrabMenuConstructedService _itemGrabMenuConstructedService;
        private readonly ItemGrabMenuChangedService _itemGrabMenuChangedService;
        private readonly DisplayedInventoryService _displayedChestInventoryService;
        private readonly PerScreen<Chest> _chest = new();
        private readonly PerScreen<bool> _attached = new();
        private readonly PerScreen<int> _menuCapacity = new();
        private readonly PerScreen<int> _menuRows = new();
        private readonly PerScreen<int> _menuOffset = new() { Value = -1 };

        /// <summary>Initializes a new instance of the <see cref="ExpandedMenuFeature"/> class.</summary>
        /// <param name="inputHelper">Provides an API for checking and changing input state.</param>
        /// <param name="modConfigService">Service to handle read/write to ModConfig.</param>
        /// <param name="itemGrabMenuConstructedService">Service to handle creation/invocation of ItemGrabMenuConstructed event.</param>
        /// <param name="itemGrabMenuChangedService">Service to handle creation/invocation of ItemGrabMenuChanged event.</param>
        /// <param name="displayedInventoryService">Service for manipulating the displayed items in an inventory menu.</param>
        public ExpandedMenuFeature(
            IInputHelper inputHelper,
            ModConfigService modConfigService,
            ItemGrabMenuConstructedService itemGrabMenuConstructedService,
            ItemGrabMenuChangedService itemGrabMenuChangedService,
            DisplayedInventoryService displayedChestInventoryService)
            : base("ExpandedMenu")
        {
            ExpandedMenuFeature.Instance = this;
            this._inputHelper = inputHelper;
            this._modConfigService = modConfigService;
            this._itemGrabMenuConstructedService = itemGrabMenuConstructedService;
            this._itemGrabMenuChangedService = itemGrabMenuChangedService;
            this._displayedChestInventoryService = displayedChestInventoryService;
        }

        /// <inheritdoc/>
        public override void Activate(IModEvents modEvents, Harmony harmony)
        {
            // Events
            this._itemGrabMenuConstructedService.AddHandler(this.OnItemGrabMenuConstructedEvent);
            this._itemGrabMenuChangedService.AddHandler(this.OnItemGrabMenuChanged);
            modEvents.Input.ButtonsChanged += this.OnButtonsChanged;
            modEvents.Input.MouseWheelScrolled += this.OnMouseWheelScrolled;

            // Patches
            harmony.Patch(
                original: AccessTools.Constructor(typeof(ItemGrabMenu), ExpandedMenuFeature.ItemGrabMenuConstructorParams),
                transpiler: new HarmonyMethod(typeof(ExpandedMenuFeature), nameof(ExpandedMenuFeature.ItemGrabMenu_constructor_transpiler)));
            harmony.Patch(
                original: AccessTools.Method(typeof(ItemGrabMenu), nameof(ItemGrabMenu.draw), new[] { typeof(SpriteBatch) }),
                transpiler: new HarmonyMethod(typeof(ExpandedMenuFeature), nameof(ExpandedMenuFeature.ItemGrabMenu_draw_transpiler)));
            harmony.Patch(
                original: AccessTools.Method(typeof(MenuWithInventory), nameof(MenuWithInventory.draw), ExpandedMenuFeature.MenuWithInventoryDrawParams),
                transpiler: new HarmonyMethod(typeof(ExpandedMenuFeature), nameof(ExpandedMenuFeature.MenuWithInventory_draw_transpiler)));
        }

        /// <inheritdoc/>
        public override void Deactivate(IModEvents modEvents, Harmony harmony)
        {
            // Events
            this._itemGrabMenuConstructedService.RemoveHandler(this.OnItemGrabMenuConstructedEvent);
            this._itemGrabMenuChangedService.RemoveHandler(this.OnItemGrabMenuChanged);
            modEvents.Input.ButtonsChanged -= this.OnButtonsChanged;
            modEvents.Input.MouseWheelScrolled -= this.OnMouseWheelScrolled;

            // Patches
            harmony.Unpatch(
                original: AccessTools.Constructor(typeof(ItemGrabMenu), ExpandedMenuFeature.ItemGrabMenuConstructorParams),
                patch: AccessTools.Method(typeof(ExpandedMenuFeature), nameof(ExpandedMenuFeature.ItemGrabMenu_constructor_transpiler)));
            harmony.Unpatch(
                original: AccessTools.Method(typeof(ItemGrabMenu), nameof(ItemGrabMenu.draw), new[] { typeof(SpriteBatch) }),
                patch: AccessTools.Method(typeof(ExpandedMenuFeature), nameof(ExpandedMenuFeature.ItemGrabMenu_draw_transpiler)));
            harmony.Unpatch(
                original: AccessTools.Method(typeof(MenuWithInventory), nameof(MenuWithInventory.draw), ExpandedMenuFeature.MenuWithInventoryDrawParams),
                patch: AccessTools.Method(typeof(ExpandedMenuFeature), nameof(ExpandedMenuFeature.MenuWithInventory_draw_transpiler)));
        }

        /// <summary>Generate additional slots/rows for top inventory menu.</summary>
        [SuppressMessage("ReSharper", "HeapView.BoxingAllocation", Justification = "Boxing allocation is required for Harmony.")]
        private static IEnumerable<CodeInstruction> ItemGrabMenu_constructor_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            Log.Trace("Changing jump condition from Beq 36 to Bge 10.");
            var jumpPatch = new PatternPatch(PatchType.Replace);
            jumpPatch
                .Find(
                    new[]
                    {
                        new CodeInstruction(OpCodes.Isinst, typeof(Chest)),
                        new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(Chest), nameof(Chest.GetActualCapacity))),
                        new CodeInstruction(OpCodes.Ldc_I4_S, (sbyte)36),
                        new CodeInstruction(OpCodes.Beq_S),
                    })
                .Patch(delegate(LinkedList<CodeInstruction> list)
                {
                    CodeInstruction jumpCode = list.Last.Value;
                    list.RemoveLast();
                    list.RemoveLast();
                    list.AddLast(new CodeInstruction(OpCodes.Ldc_I4_S, (sbyte)10));
                    list.AddLast(new CodeInstruction(OpCodes.Bge_S, jumpCode.operand));
                });

            Log.Trace("Overriding default values for capacity and rows.");
            var capacityPatch = new PatternPatch(PatchType.Replace);
            capacityPatch
                .Find(
                    new[]
                    {
                        new CodeInstruction(OpCodes.Newobj, AccessTools.Constructor(typeof(InventoryMenu), new[] { typeof(int), typeof(int), typeof(bool), typeof(IList<Item>), typeof(InventoryMenu.highlightThisItem), typeof(int), typeof(int), typeof(int), typeof(int), typeof(bool) })),
                        new CodeInstruction(OpCodes.Stfld, AccessTools.Field(typeof(ItemGrabMenu), nameof(ItemGrabMenu.ItemsToGrabMenu))),
                    })
                .Find(
                    new[]
                    {
                        new CodeInstruction(OpCodes.Ldc_I4_M1),
                        new CodeInstruction(OpCodes.Ldc_I4_3),
                    })
                .Patch(delegate(LinkedList<CodeInstruction> list)
                {
                    list.RemoveLast();
                    list.RemoveLast();
                    list.AddLast(new CodeInstruction(OpCodes.Ldarg_0));
                    list.AddLast(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ExpandedMenuFeature), nameof(ExpandedMenuFeature.MenuCapacity))));
                    list.AddLast(new CodeInstruction(OpCodes.Ldarg_0));
                    list.AddLast(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ExpandedMenuFeature), nameof(ExpandedMenuFeature.MenuRows))));
                });

            var patternPatches = new PatternPatches(instructions);
            patternPatches.AddPatch(jumpPatch);
            patternPatches.AddPatch(capacityPatch);

            foreach (var patternPatch in patternPatches)
            {
                yield return patternPatch;
            }

            if (!patternPatches.Done)
            {
                Log.Warn($"Failed to apply all patches in {typeof(ExpandedMenuFeature)}::{nameof(ExpandedMenuFeature.ItemGrabMenu_constructor_transpiler)}");
            }
        }

        /// <summary>Move/resize backpack by expanded menu height.</summary>
        private static IEnumerable<CodeInstruction> ItemGrabMenu_draw_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            Log.Trace("Moving backpack icon down by expanded menu extra height.");
            var moveBackpackPatch = new PatternPatch(PatchType.Replace);
            moveBackpackPatch
                .Find(new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(ItemGrabMenu), nameof(ItemGrabMenu.showReceivingMenu))))
                .Find(new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(IClickableMenu), nameof(IClickableMenu.yPositionOnScreen))))
                .Patch(delegate(LinkedList<CodeInstruction> list)
                {
                    list.AddLast(new CodeInstruction(OpCodes.Ldarg_0));
                    list.AddLast(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ExpandedMenuFeature), nameof(ExpandedMenuFeature.MenuOffset))));
                    list.AddLast(new CodeInstruction(OpCodes.Add));
                })
                .Repeat(3);

            var patternPatches = new PatternPatches(instructions, moveBackpackPatch);

            foreach (CodeInstruction patternPatch in patternPatches)
            {
                yield return patternPatch;
            }

            if (!patternPatches.Done)
            {
                Log.Warn($"Failed to apply all patches in {typeof(ItemGrabMenu)}::{nameof(ItemGrabMenu.draw)}.");
            }
        }

        /// <summary>Move/resize bottom dialogue box by search bar height.</summary>
        [SuppressMessage("ReSharper", "HeapView.BoxingAllocation", Justification = "Boxing allocation is required for Harmony.")]
        private static IEnumerable<CodeInstruction> MenuWithInventory_draw_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            Log.Trace("Moving bottom dialogue box down by search bar height.");
            var moveDialogueBoxPatch = new PatternPatch(PatchType.Replace);
            moveDialogueBoxPatch
                .Find(
                    new[]
                    {
                        new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(IClickableMenu), nameof(IClickableMenu.yPositionOnScreen))),
                        new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(IClickableMenu), nameof(IClickableMenu.borderWidth))),
                        new CodeInstruction(OpCodes.Add),
                        new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(IClickableMenu), nameof(IClickableMenu.spaceToClearTopBorder))),
                        new CodeInstruction(OpCodes.Add), new CodeInstruction(OpCodes.Ldc_I4_S, (sbyte)64), new CodeInstruction(OpCodes.Add),
                    })
                .Patch(
                    list =>
                    {
                        list.AddLast(new CodeInstruction(OpCodes.Ldarg_0));
                        list.AddLast(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ExpandedMenuFeature), nameof(ExpandedMenuFeature.MenuOffset))));
                        list.AddLast(new CodeInstruction(OpCodes.Add));
                    });

            Log.Trace("Shrinking bottom dialogue box height by search bar height.");
            var resizeDialogueBoxPatch = new PatternPatch(PatchType.Replace);
            resizeDialogueBoxPatch
                .Find(
                    new[]
                    {
                        new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(IClickableMenu), nameof(IClickableMenu.height))),
                        new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(IClickableMenu), nameof(IClickableMenu.borderWidth))),
                        new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(IClickableMenu), nameof(IClickableMenu.spaceToClearTopBorder))),
                        new CodeInstruction(OpCodes.Add),
                        new CodeInstruction(OpCodes.Ldc_I4, 192),
                        new CodeInstruction(OpCodes.Add),
                    })
                .Patch(
                    list =>
                    {
                        list.AddLast(new CodeInstruction(OpCodes.Ldarg_0));
                        list.AddLast(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ExpandedMenuFeature), nameof(ExpandedMenuFeature.MenuOffset))));
                        list.AddLast(new CodeInstruction(OpCodes.Add));
                    });

            var patternPatches = new PatternPatches(instructions);
            patternPatches.AddPatch(moveDialogueBoxPatch);
            patternPatches.AddPatch(resizeDialogueBoxPatch);

            foreach (CodeInstruction patternPatch in patternPatches)
            {
                yield return patternPatch;
            }

            if (!patternPatches.Done)
            {
                Log.Warn($"Failed to apply all patches in {typeof(MenuWithInventory)}::{nameof(MenuWithInventory.draw)}.");
            }
        }

        private static int MenuCapacity(MenuWithInventory menu)
        {
            if (ExpandedMenuFeature.Instance._menuCapacity.Value != 0)
            {
                return ExpandedMenuFeature.Instance._menuCapacity.Value;
            }

            if (menu is not ItemGrabMenu { context: Chest chest } || !ExpandedMenuFeature.Instance.IsEnabledForItem(chest))
            {
                return ExpandedMenuFeature.Instance._menuCapacity.Value = -1; // Vanilla
            }

            int capacity = chest.GetActualCapacity();
            int maxMenuRows = ExpandedMenuFeature.Instance._modConfigService.ModConfig.MenuRows;
            return ExpandedMenuFeature.Instance._menuCapacity.Value = capacity switch
            {
                < 72 => Math.Min(maxMenuRows * 12, capacity.RoundUp(12)), // Variable
                _ => maxMenuRows * 12, // Large
            };
        }

        private static int MenuRows(MenuWithInventory menu)
        {
            if (ExpandedMenuFeature.Instance._menuRows.Value != 0)
            {
                return ExpandedMenuFeature.Instance._menuRows.Value;
            }

            if (menu is not ItemGrabMenu { context: Chest chest } || !ExpandedMenuFeature.Instance.IsEnabledForItem(chest))
            {
                return ExpandedMenuFeature.Instance._menuRows.Value = 3; // Vanilla
            }

            int capacity = chest.GetActualCapacity();
            int maxMenuRows = ExpandedMenuFeature.Instance._modConfigService.ModConfig.MenuRows;
            return ExpandedMenuFeature.Instance._menuRows.Value = capacity switch
            {
                < 72 => (int)Math.Min(maxMenuRows, Math.Ceiling(capacity / 12f)),
                _ => maxMenuRows,
            };
        }

        private static int MenuOffset(MenuWithInventory menu)
        {
            if (ExpandedMenuFeature.Instance._menuOffset.Value != -1)
            {
                return ExpandedMenuFeature.Instance._menuOffset.Value;
            }

            if (menu is not ItemGrabMenu { context: Chest chest } || !ExpandedMenuFeature.Instance.IsEnabledForItem(chest))
            {
                return ExpandedMenuFeature.Instance._menuOffset.Value = 0; // Vanilla
            }

            int rows = ExpandedMenuFeature.MenuRows(menu);
            return ExpandedMenuFeature.Instance._menuOffset.Value = Game1.tileSize * (rows - 3);
        }

        private void OnItemGrabMenuConstructedEvent(object sender, ItemGrabMenuEventArgs e)
        {
            if (e.ItemGrabMenu is null || e.Chest is null || !this.IsEnabledForItem(e.Chest))
            {
                return;
            }

            if (!ReferenceEquals(this._chest.Value, e.Chest))
            {
                this._chest.Value = e.Chest;
                this._attached.Value = false;
                this._menuCapacity.Value = 0;
                this._menuRows.Value = 0;
                this._menuOffset.Value = -1;
            }

            int offset = ExpandedMenuFeature.MenuOffset(e.ItemGrabMenu);
            e.ItemGrabMenu.height += offset;
            e.ItemGrabMenu.inventory.movePosition(0, offset);
            if (e.ItemGrabMenu.okButton is not null)
            {
                e.ItemGrabMenu.okButton.bounds.Y += offset;
            }

            if (e.ItemGrabMenu.trashCan is not null)
            {
                e.ItemGrabMenu.trashCan.bounds.Y += offset;
            }

            if (e.ItemGrabMenu.dropItemInvisibleButton is not null)
            {
                e.ItemGrabMenu.dropItemInvisibleButton.bounds.Y += offset;
            }

            e.ItemGrabMenu.RepositionSideButtons();
        }

        private void OnItemGrabMenuChanged(object sender, ItemGrabMenuEventArgs e)
        {
            if (e.ItemGrabMenu is null || e.Chest is null || !this.IsEnabledForItem(e.Chest))
            {
                this._attached.Value = false;
                this._menuCapacity.Value = 0;
                this._menuRows.Value = 0;
                this._menuOffset.Value = -1;
                return;
            }

            this._attached.Value = true;
        }

        private void OnMouseWheelScrolled(object sender, MouseWheelScrolledEventArgs e)
        {
            if (!this._attached.Value)
            {
                return;
            }

            switch (e.Delta)
            {
                case > 0:
                    this._displayedChestInventoryService.Offset--;
                    break;
                case < 0:
                    this._displayedChestInventoryService.Offset++;
                    break;
                default:
                    return;
            }
        }

        private void OnButtonsChanged(object sender, ButtonsChangedEventArgs e)
        {
            if (!this._attached.Value)
            {
                return;
            }

            if (this._modConfigService.ModConfig.ScrollUp.JustPressed())
            {
                this._displayedChestInventoryService.Offset--;
                this._inputHelper.SuppressActiveKeybinds(this._modConfigService.ModConfig.ScrollUp);
                return;
            }

            if (this._modConfigService.ModConfig.ScrollDown.JustPressed())
            {
                this._displayedChestInventoryService.Offset++;
                this._inputHelper.SuppressActiveKeybinds(this._modConfigService.ModConfig.ScrollDown);
            }
        }
    }
}