namespace BetterChests.Services
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection.Emit;
    using Common.Extensions;
    using Common.Helpers;
    using Common.Services;
    using CommonHarmony;
    using CommonHarmony.Enums;
    using CommonHarmony.Models;
    using CommonHarmony.Services;
    using HarmonyLib;
    using Microsoft.Xna.Framework.Graphics;
    using Models;
    using StardewModdingAPI;
    using StardewModdingAPI.Events;
    using StardewModdingAPI.Utilities;
    using StardewValley;
    using StardewValley.Menus;
    using StardewValley.Objects;

    internal class ResizeChestMenuService : BaseService, IFeatureService
    {
        private static ManagedChestService ManagedChestService;
        private static ModConfig ModConfig;
        private readonly IModHelper _helper;
        private readonly PerScreen<ItemGrabMenuEventArgs> _menu = new();
        private DisplayedInventoryService _displayedInventory;
        private HarmonyService _harmony;
        private ItemGrabMenuChangedService _itemGrabMenuChanged;

        private ResizeChestMenuService(ServiceManager serviceManager)
            : base("ResizeChestMenu")
        {
            // Init
            this._helper = serviceManager.Helper;

            // Dependencies
            this.AddDependency<DisplayedInventoryService>(service => this._displayedInventory = service as DisplayedInventoryService);
            this.AddDependency<ItemGrabMenuChangedService>(service => this._itemGrabMenuChanged = service as ItemGrabMenuChangedService);
            this.AddDependency<ManagedChestService>(service => ResizeChestMenuService.ManagedChestService = service as ManagedChestService);
            this.AddDependency<ModConfigService>(service => ResizeChestMenuService.ModConfig = (service as ModConfigService)?.ModConfig);
            this.AddDependency<HarmonyService>(
                service =>
                {
                    // Init
                    this._harmony = service as HarmonyService;
                    var ctorItemGrabMenu = new[]
                    {
                        typeof(IList<Item>), typeof(bool), typeof(bool), typeof(InventoryMenu.highlightThisItem), typeof(ItemGrabMenu.behaviorOnItemSelect), typeof(string), typeof(ItemGrabMenu.behaviorOnItemSelect), typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(int), typeof(Item), typeof(int), typeof(object),
                    };

                    var drawMenuWithInventory = new[]
                    {
                        typeof(SpriteBatch), typeof(bool), typeof(bool), typeof(int), typeof(int), typeof(int),
                    };

                    // Patches
                    this._harmony?.AddPatch(
                        this.ServiceName,
                        AccessTools.Constructor(typeof(ItemGrabMenu), ctorItemGrabMenu),
                        typeof(ResizeChestMenuService),
                        nameof(ResizeChestMenuService.ItemGrabMenu_constructor_transpiler),
                        PatchType.Transpiler);

                    this._harmony?.AddPatch(
                        this.ServiceName,
                        AccessTools.Method(
                            typeof(ItemGrabMenu),
                            nameof(ItemGrabMenu.draw),
                            new[]
                            {
                                typeof(SpriteBatch),
                            }),
                        typeof(ResizeChestMenuService),
                        nameof(ResizeChestMenuService.ItemGrabMenu_draw_transpiler),
                        PatchType.Transpiler);

                    this._harmony?.AddPatch(
                        this.ServiceName,
                        AccessTools.Method(typeof(MenuWithInventory), nameof(MenuWithInventory.draw), drawMenuWithInventory),
                        typeof(ResizeChestMenuService),
                        nameof(ResizeChestMenuService.MenuWithInventory_draw_transpiler),
                        PatchType.Transpiler);
                });
        }

        /// <inheritdoc />
        public void Activate()
        {
            // Events
            this._itemGrabMenuChanged.AddHandler(this.OnItemGrabMenuChanged);
            this._helper.Events.Input.ButtonsChanged += this.OnButtonsChanged;
            this._helper.Events.Input.MouseWheelScrolled += this.OnMouseWheelScrolled;

            // Patches
            this._harmony.ApplyPatches(this.ServiceName);
        }

        /// <inheritdoc />
        public void Deactivate()
        {
            // Events
            this._itemGrabMenuChanged.RemoveHandler(this.OnItemGrabMenuChanged);
            this._helper.Events.Input.ButtonsChanged -= this.OnButtonsChanged;
            this._helper.Events.Input.MouseWheelScrolled -= this.OnMouseWheelScrolled;

            // Patches
            this._harmony.UnapplyPatches(this.ServiceName);
        }

        /// <summary>Generate additional slots/rows for top inventory menu.</summary>
        [SuppressMessage("ReSharper", "HeapView.BoxingAllocation", Justification = "Boxing allocation is required for Harmony.")]
        private static IEnumerable<CodeInstruction> ItemGrabMenu_constructor_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            Log.Trace("Changing jump condition from Beq 36 to Bge 10.");
            var jumpPatch = new PatternPatch();
            jumpPatch
                .Find(
                    new[]
                    {
                        new CodeInstruction(OpCodes.Isinst, typeof(Chest)), new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(Chest), nameof(Chest.GetActualCapacity))), new CodeInstruction(OpCodes.Ldc_I4_S, (sbyte)36), new CodeInstruction(OpCodes.Beq_S),
                    })
                .Patch(
                    delegate(LinkedList<CodeInstruction> list)
                    {
                        var jumpCode = list.Last?.Value;
                        list.RemoveLast();
                        list.RemoveLast();
                        list.AddLast(new CodeInstruction(OpCodes.Ldc_I4_S, (sbyte)10));
                        list.AddLast(new CodeInstruction(OpCodes.Bge_S, jumpCode?.operand));
                    });

            Log.Trace("Overriding default values for capacity and rows.");
            var capacityPatch = new PatternPatch();
            capacityPatch
                .Find(
                    new[]
                    {
                        new CodeInstruction(
                            OpCodes.Newobj,
                            AccessTools.Constructor(
                                typeof(InventoryMenu),
                                new[]
                                {
                                    typeof(int), typeof(int), typeof(bool), typeof(IList<Item>), typeof(InventoryMenu.highlightThisItem), typeof(int), typeof(int), typeof(int), typeof(int), typeof(bool),
                                })),
                        new CodeInstruction(OpCodes.Stfld, AccessTools.Field(typeof(ItemGrabMenu), nameof(ItemGrabMenu.ItemsToGrabMenu))),
                    })
                .Find(
                    new[]
                    {
                        new CodeInstruction(OpCodes.Ldc_I4_M1), new CodeInstruction(OpCodes.Ldc_I4_3),
                    })
                .Patch(
                    delegate(LinkedList<CodeInstruction> list)
                    {
                        list.RemoveLast();
                        list.RemoveLast();
                        list.AddLast(new CodeInstruction(OpCodes.Ldarg_0));
                        list.AddLast(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ResizeChestMenuService), nameof(ResizeChestMenuService.MenuCapacity))));
                        list.AddLast(new CodeInstruction(OpCodes.Ldarg_0));
                        list.AddLast(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ResizeChestMenuService), nameof(ResizeChestMenuService.MenuRows))));
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
                Log.Warn($"Failed to apply all patches in {typeof(ResizeChestMenuService)}::{nameof(ResizeChestMenuService.ItemGrabMenu_constructor_transpiler)}");
            }
        }

        /// <summary>Move/resize backpack by expanded menu height.</summary>
        private static IEnumerable<CodeInstruction> ItemGrabMenu_draw_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            Log.Trace("Moving backpack icon down by expanded menu extra height.");
            var moveBackpackPatch = new PatternPatch();
            moveBackpackPatch
                .Find(new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(ItemGrabMenu), nameof(ItemGrabMenu.showReceivingMenu))))
                .Find(new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(IClickableMenu), nameof(IClickableMenu.yPositionOnScreen))))
                .Patch(
                    delegate(LinkedList<CodeInstruction> list)
                    {
                        list.AddLast(new CodeInstruction(OpCodes.Ldarg_0));
                        list.AddLast(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ResizeChestMenuService), nameof(ResizeChestMenuService.MenuOffset))));
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
            var moveDialogueBoxPatch = new PatternPatch();
            moveDialogueBoxPatch
                .Find(
                    new[]
                    {
                        new CodeInstruction(
                            OpCodes.Ldfld,
                            AccessTools.Field(
                                typeof(IClickableMenu),
                                nameof(IClickableMenu.yPositionOnScreen))),
                        new CodeInstruction(
                            OpCodes.Ldsfld,
                            AccessTools.Field(
                                typeof(IClickableMenu),
                                nameof(IClickableMenu.borderWidth))),
                        new CodeInstruction(OpCodes.Add),
                        new CodeInstruction(
                            OpCodes.Ldsfld,
                            AccessTools.Field(
                                typeof(IClickableMenu),
                                nameof(IClickableMenu.spaceToClearTopBorder))),
                        new CodeInstruction(OpCodes.Add),
                        new CodeInstruction(
                            OpCodes.Ldc_I4_S,
                            (sbyte)64),
                        new CodeInstruction(OpCodes.Add),
                    })
                .Patch(
                    list =>
                    {
                        list.AddLast(new CodeInstruction(OpCodes.Ldarg_0));
                        list.AddLast(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ResizeChestMenuService), nameof(ResizeChestMenuService.MenuOffset))));
                        list.AddLast(new CodeInstruction(OpCodes.Add));
                    });

            Log.Trace("Shrinking bottom dialogue box height by expanded menu height.");
            var resizeDialogueBoxPatch = new PatternPatch();
            resizeDialogueBoxPatch
                .Find(
                    new[]
                    {
                        new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(IClickableMenu), nameof(IClickableMenu.height))), new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(IClickableMenu), nameof(IClickableMenu.borderWidth))), new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(IClickableMenu), nameof(IClickableMenu.spaceToClearTopBorder))), new CodeInstruction(OpCodes.Add), new CodeInstruction(OpCodes.Ldc_I4, 192), new CodeInstruction(OpCodes.Add),
                    })
                .Patch(
                    list =>
                    {
                        list.AddLast(new CodeInstruction(OpCodes.Ldarg_0));
                        list.AddLast(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ResizeChestMenuService), nameof(ResizeChestMenuService.MenuOffset))));
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
            if (menu is not ItemGrabMenu {context: Chest {playerChest: {Value: true}} chest})
            {
                return -1; // Vanilla
            }

            if (!ResizeChestMenuService.ManagedChestService.TryGetChestConfig(chest, out var chestConfig) || chestConfig.Capacity == 0)
            {
                return -1;
            }

            var capacity = chest.GetActualCapacity();
            var maxMenuRows = ResizeChestMenuService.ModConfig.MenuRows;
            return capacity switch
            {
                < 72 => Math.Min(maxMenuRows * 12, capacity.RoundUp(12)), // Variable
                _ => maxMenuRows * 12, // Large
            };
        }

        private static int MenuRows(MenuWithInventory menu)
        {
            if (menu is not ItemGrabMenu {context: Chest {playerChest: {Value: true}} chest})
            {
                return 3; // Vanilla
            }

            if (!ResizeChestMenuService.ManagedChestService.TryGetChestConfig(chest, out var chestConfig) || chestConfig.Capacity == 0)
            {
                return 3;
            }

            var capacity = chest.GetActualCapacity();
            var maxMenuRows = ResizeChestMenuService.ModConfig.MenuRows;
            return capacity switch
            {
                < 72 => (int)Math.Min(maxMenuRows, Math.Ceiling(capacity / 12f)),
                _ => maxMenuRows,
            };
        }

        private static int MenuOffset(MenuWithInventory menu)
        {
            if (menu is not ItemGrabMenu {context: Chest {playerChest: {Value: true}} chest})
            {
                return 0; // Vanilla
            }

            if (!ResizeChestMenuService.ManagedChestService.TryGetChestConfig(chest, out var chestConfig) || chestConfig.Capacity == 0)
            {
                return 0;
            }

            var rows = ResizeChestMenuService.MenuRows(menu);
            return Game1.tileSize * (rows - 3);
        }

        private void OnItemGrabMenuChanged(object sender, ItemGrabMenuEventArgs e)
        {
            if (e.ItemGrabMenu is null || e.Chest is null || !ResizeChestMenuService.ManagedChestService.TryGetChestConfig(e.Chest, out var chestConfig) || chestConfig.Capacity == 0)
            {
                this._menu.Value = null;
                return;
            }

            if (e.IsNew)
            {
                var offset = ResizeChestMenuService.MenuOffset(e.ItemGrabMenu);
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

            this._menu.Value = e;
        }

        private void OnMouseWheelScrolled(object sender, MouseWheelScrolledEventArgs e)
        {
            if (this._menu.Value is null || this._menu.Value.ScreenId != Context.ScreenId)
            {
                return;
            }

            switch (e.Delta)
            {
                case > 0:
                    this._displayedInventory.Offset--;
                    break;
                case < 0:
                    this._displayedInventory.Offset++;
                    break;
                default:
                    return;
            }
        }

        private void OnButtonsChanged(object sender, ButtonsChangedEventArgs e)
        {
            if (this._menu.Value is null || this._menu.Value.ScreenId != Context.ScreenId)
            {
                return;
            }

            if (ResizeChestMenuService.ModConfig.ScrollUp.JustPressed())
            {
                this._displayedInventory.Offset--;
                this._helper.Input.SuppressActiveKeybinds(ResizeChestMenuService.ModConfig.ScrollUp);
                return;
            }

            if (ResizeChestMenuService.ModConfig.ScrollDown.JustPressed())
            {
                this._displayedInventory.Offset++;
                this._helper.Input.SuppressActiveKeybinds(ResizeChestMenuService.ModConfig.ScrollDown);
            }
        }
    }
}