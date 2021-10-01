namespace XSPlus.Features
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection.Emit;
    using Common.Extensions;
    using Common.Helpers;
    using Common.Models;
    using Common.Services;
    using CommonHarmony;
    using CommonHarmony.Services;
    using HarmonyLib;
    using Microsoft.Xna.Framework.Graphics;
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
        private readonly ModConfigService _modConfigService;
        private readonly ItemGrabMenuConstructedService _itemGrabMenuConstructedService;
        private readonly ItemGrabMenuChangedService _itemGrabMenuChangedService;
        private readonly DisplayedInventoryService _displayedInventoryService;
        private readonly PerScreen<int> _screenId = new() { Value = -1 };
        private readonly PerScreen<Chest> _chest = new();
        private MixInfo _itemGrabMenuConstructorPatch;
        private MixInfo _itemGrabMenuDrawPatch;
        private MixInfo _menuWithInventoryDrawPatch;

        private ExpandedMenuFeature(
            ModConfigService modConfigService,
            ItemGrabMenuConstructedService itemGrabMenuConstructedService,
            ItemGrabMenuChangedService itemGrabMenuChangedService,
            DisplayedInventoryService displayedInventoryService)
            : base("ExpandedMenu", modConfigService)
        {
            this._modConfigService = modConfigService;
            this._itemGrabMenuConstructedService = itemGrabMenuConstructedService;
            this._itemGrabMenuChangedService = itemGrabMenuChangedService;
            this._displayedInventoryService = displayedInventoryService;
        }

        /// <summary>
        /// Gets or sets the instance of <see cref="ExpandedMenuFeature"/>.
        /// </summary>
        private static ExpandedMenuFeature Instance { get; set; }

        /// <summary>
        /// Returns and creates if needed an instance of the <see cref="ExpandedMenuFeature"/> class.
        /// </summary>
        /// <param name="serviceManager">Service manager to request shared services.</param>
        /// <returns>Returns an instance of the <see cref="ExpandedMenuFeature"/> class.</returns>
        public static ExpandedMenuFeature GetSingleton(ServiceManager serviceManager)
        {
            var modConfigService = serviceManager.RequestService<ModConfigService>();
            var itemGrabMenuConstructedService = serviceManager.RequestService<ItemGrabMenuConstructedService>();
            var itemGrabMenuChangedService = serviceManager.RequestService<ItemGrabMenuChangedService>();
            var displayedInventoryService = serviceManager.RequestService<DisplayedInventoryService>("DisplayedInventory");
            return ExpandedMenuFeature.Instance ??= new ExpandedMenuFeature(
                modConfigService,
                itemGrabMenuConstructedService,
                itemGrabMenuChangedService,
                displayedInventoryService);
        }

        /// <inheritdoc/>
        public override void Activate()
        {
            // Events
            this._itemGrabMenuConstructedService.AddHandler(this.OnItemGrabMenuConstructedEvent);
            this._itemGrabMenuChangedService.AddHandler(this.OnItemGrabMenuChanged);
            Events.Input.ButtonsChanged += this.OnButtonsChanged;
            Events.Input.MouseWheelScrolled += this.OnMouseWheelScrolled;

            // Patches
            this._itemGrabMenuConstructorPatch = Mixin.Transpiler(
                AccessTools.Constructor(typeof(ItemGrabMenu), ExpandedMenuFeature.ItemGrabMenuConstructorParams),
                typeof(ExpandedMenuFeature),
                nameof(ExpandedMenuFeature.ItemGrabMenu_constructor_transpiler));
            this._itemGrabMenuDrawPatch = Mixin.Transpiler(
                AccessTools.Method(typeof(ItemGrabMenu), nameof(ItemGrabMenu.draw), new[] { typeof(SpriteBatch) }),
                typeof(ExpandedMenuFeature),
                nameof(ExpandedMenuFeature.ItemGrabMenu_draw_transpiler));
            this._menuWithInventoryDrawPatch = Mixin.Transpiler(
                AccessTools.Method(typeof(MenuWithInventory), nameof(MenuWithInventory.draw), ExpandedMenuFeature.MenuWithInventoryDrawParams),
                typeof(ExpandedMenuFeature),
                nameof(ExpandedMenuFeature.MenuWithInventory_draw_transpiler));
        }

        /// <inheritdoc/>
        public override void Deactivate()
        {
            // Events
            this._itemGrabMenuConstructedService.RemoveHandler(this.OnItemGrabMenuConstructedEvent);
            this._itemGrabMenuChangedService.RemoveHandler(this.OnItemGrabMenuChanged);
            Events.Input.ButtonsChanged -= this.OnButtonsChanged;
            Events.Input.MouseWheelScrolled -= this.OnMouseWheelScrolled;

            // Patches
            Mixin.Unpatch(this._itemGrabMenuConstructorPatch);
            Mixin.Unpatch(this._itemGrabMenuDrawPatch);
            Mixin.Unpatch(this._menuWithInventoryDrawPatch);
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
                    var jumpCode = list.Last.Value;
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

            foreach (var patternPatch in patternPatches)
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
            Log.Trace("Moving bottom dialogue box down by expanded menu height.");
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

            Log.Trace("Shrinking bottom dialogue box height by expanded menu height.");
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

            foreach (var patternPatch in patternPatches)
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
            if (menu is not ItemGrabMenu { context: Chest { playerChest: { Value: true } } chest } || !ExpandedMenuFeature.Instance.IsEnabledForItem(chest))
            {
                return -1; // Vanilla
            }

            var capacity = chest.GetActualCapacity();
            var maxMenuRows = ExpandedMenuFeature.Instance._modConfigService.ModConfig.MenuRows;
            return capacity switch
            {
                < 72 => Math.Min(maxMenuRows * 12, capacity.RoundUp(12)), // Variable
                _ => maxMenuRows * 12, // Large
            };
        }

        private static int MenuRows(MenuWithInventory menu)
        {
            if (menu is not ItemGrabMenu { context: Chest { playerChest: { Value: true } } chest } || !ExpandedMenuFeature.Instance.IsEnabledForItem(chest))
            {
                return 3; // Vanilla
            }

            var capacity = chest.GetActualCapacity();
            var maxMenuRows = ExpandedMenuFeature.Instance._modConfigService.ModConfig.MenuRows;
            return capacity switch
            {
                < 72 => (int)Math.Min(maxMenuRows, Math.Ceiling(capacity / 12f)),
                _ => maxMenuRows,
            };
        }

        private static int MenuOffset(MenuWithInventory menu)
        {
            if (menu is not ItemGrabMenu { context: Chest { playerChest: { Value: true } } chest } || !ExpandedMenuFeature.Instance.IsEnabledForItem(chest))
            {
                return 0; // Vanilla
            }

            var rows = ExpandedMenuFeature.MenuRows(menu);
            return Game1.tileSize * (rows - 3);
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
            }

            var offset = ExpandedMenuFeature.MenuOffset(e.ItemGrabMenu);
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
                this._screenId.Value = -1;
                return;
            }

            this._screenId.Value = Context.ScreenId;
        }

        private void OnMouseWheelScrolled(object sender, MouseWheelScrolledEventArgs e)
        {
            if (this._screenId.Value != Context.ScreenId)
            {
                return;
            }

            switch (e.Delta)
            {
                case > 0:
                    this._displayedInventoryService.Offset--;
                    break;
                case < 0:
                    this._displayedInventoryService.Offset++;
                    break;
                default:
                    return;
            }
        }

        private void OnButtonsChanged(object sender, ButtonsChangedEventArgs e)
        {
            if (this._screenId.Value != Context.ScreenId)
            {
                return;
            }

            if (this._modConfigService.ModConfig.ScrollUp.JustPressed())
            {
                this._displayedInventoryService.Offset--;
                Input.Suppress(this._modConfigService.ModConfig.ScrollUp);
                return;
            }

            if (this._modConfigService.ModConfig.ScrollDown.JustPressed())
            {
                this._displayedInventoryService.Offset++;
                Input.Suppress(this._modConfigService.ModConfig.ScrollDown);
            }
        }
    }
}