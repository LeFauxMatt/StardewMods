namespace XSPlus.Features
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection.Emit;
    using HarmonyLib;
    using Microsoft.Xna.Framework;
    using StardewModdingAPI;
    using StardewModdingAPI.Events;
    using StardewModdingAPI.Utilities;
    using StardewValley;
    using StardewValley.Menus;
    using StardewValley.Objects;

    /// <inheritdoc />
    internal class CraftFromChestFeature : FeatureWithParam<string>
    {
        private readonly IInputHelper _inputHelper;
        private readonly Func<KeybindList> _getCraftingButton;
        private readonly Func<string> _getConfigRange;
        private readonly PerScreen<List<Chest>> _cachedEnabledChests = new();

        /// <summary>Initializes a new instance of the <see cref="CraftFromChestFeature"/> class.</summary>
        /// <param name="inputHelper">API for changing state of input.</param>
        /// <param name="getCraftingButton">Get method for configured crafting button.</param>
        /// <param name="getConfigRange">Get method for configured default range.</param>
        public CraftFromChestFeature(IInputHelper inputHelper, Func<KeybindList> getCraftingButton, Func<string> getConfigRange)
            : base("CraftFromChest")
        {
            this._inputHelper = inputHelper;
            this._getCraftingButton = getCraftingButton;
            this._getConfigRange = getConfigRange;
        }

        private List<Chest> EnabledChests
        {
            get => this._cachedEnabledChests.Value ??= Game1.player.Items.OfType<Chest>()
                .Union(XSPlus.AccessibleChests)
                .Where(this.IsEnabledForItem)
                .ToList();
        }

        /// <inheritdoc/>
        public override void Activate(IModEvents modEvents, Harmony harmony)
        {
            // Events
            modEvents.Player.InventoryChanged += this.OnInventoryChanged;
            modEvents.Player.Warped += this.OnWarped;
            modEvents.Input.ButtonsChanged += this.OnButtonsChanged;

            // Patches
            harmony.Patch(
                original: AccessTools.Method(typeof(CraftingPage), "getContainerContents"),
                postfix: new HarmonyMethod(typeof(CraftFromChestFeature), nameof(CraftFromChestFeature.CraftingPage_getContainerContents_postfix)));
            harmony.Patch(
                original: AccessTools.Method(typeof(CraftingRecipe), nameof(CraftingRecipe.consumeIngredients)),
                transpiler: new HarmonyMethod(typeof(CraftFromChestFeature), nameof(CraftFromChestFeature.CraftingRecipe_consumeIngredients_transpiler)));
        }

        /// <inheritdoc/>
        public override void Deactivate(IModEvents modEvents, Harmony harmony)
        {

            // Events
            modEvents.Player.InventoryChanged -= this.OnInventoryChanged;
            modEvents.Player.Warped -= this.OnWarped;
            modEvents.Input.ButtonsChanged -= this.OnButtonsChanged;

            // Patches
            harmony.Unpatch(
                original: AccessTools.Method(typeof(CraftingPage), "getContainerContents"),
                patch: AccessTools.Method(typeof(CraftFromChestFeature), nameof(CraftFromChestFeature.CraftingPage_getContainerContents_postfix)));
            harmony.Unpatch(
                original: AccessTools.Method(typeof(CraftingRecipe), nameof(CraftingRecipe.consumeIngredients)),
                patch: AccessTools.Method(typeof(CraftFromChestFeature), nameof(CraftFromChestFeature.CraftingRecipe_consumeIngredients_transpiler)));
        }

        /// <inheritdoc/>
        [SuppressMessage("ReSharper", "HeapView.BoxingAllocation", Justification = "Required for enumerating this collection.")]
        protected internal override bool IsEnabledForItem(Item item)
        {
            if (!base.IsEnabledForItem(item) || item is not Chest || !this.TryGetValueForItem(item, out string range))
            {
                return false;
            }

            return range switch
            {
                "Inventory" => Game1.player.Items.IndexOf(item) != -1,
                "Location" => Game1.currentLocation.Objects.Values.Contains(item),
                "World" => true,
                _ => false,
            };
        }

        /// <inheritdoc/>
        protected override bool TryGetValueForItem(Item item, out string param)
        {
            if (base.TryGetValueForItem(item, out param))
            {
                return true;
            }

            param = this._getConfigRange();
            return !string.IsNullOrWhiteSpace(param);
        }

        [SuppressMessage("ReSharper", "SA1313", Justification = "Naming is determined by Harmony.")]
        [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Naming is determined by Harmony.")]
        [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Type is determined by Harmony.")]
        private static void CraftingPage_getContainerContents_postfix(CraftingPage __instance, ref IList<Item> __result)
        {
            if (__instance._materialContainers == null)
            {
                return;
            }

            __result.Clear();
            var items = new List<Item>();
            foreach (Chest chest in __instance._materialContainers)
            {
                items.AddRange(chest.GetItemsForPlayer(Game1.player.UniqueMultiplayerID));
            }

            __result = items;
        }

        [SuppressMessage("ReSharper", "SA1313", Justification = "Naming is determined by Harmony.")]
        [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Naming is determined by Harmony.")]
        private static IEnumerable<CodeInstruction> CraftingRecipe_consumeIngredients_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Ldfld && instruction.operand.Equals(AccessTools.Field(typeof(Chest), nameof(Chest.items))))
                {
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Property(typeof(Game1), nameof(Game1.player)).GetGetMethod());
                    yield return new CodeInstruction(OpCodes.Callvirt, AccessTools.Property(typeof(Farmer), nameof(Farmer.UniqueMultiplayerID)).GetGetMethod());
                    yield return new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(Chest), nameof(Chest.GetItemsForPlayer)));
                }
                else
                {
                    yield return instruction;
                }
            }
        }

        private void OnInventoryChanged(object sender, InventoryChangedEventArgs e)
        {
            if (!e.IsLocalPlayer)
            {
                return;
            }

            this._cachedEnabledChests.Value = null;
        }

        private void OnWarped(object sender, WarpedEventArgs e)
        {
            this._cachedEnabledChests.Value = null;
        }

        /// <summary>Open crafting menu for all chests in inventory.</summary>
        [SuppressMessage("ReSharper", "AccessToModifiedClosure", Justification = "Needed for mutex release.")]
        private void OnButtonsChanged(object sender, ButtonsChangedEventArgs e)
        {
            KeybindList craftingButton = this._getCraftingButton();

            if (!Context.IsPlayerFree || !craftingButton.JustPressed() || !this.EnabledChests.Any())
            {
                return;
            }

            var mutexes = this.EnabledChests.Select(chest => chest.mutex).ToList();
            MultipleMutexRequest multipleMutexRequest = null;
            multipleMutexRequest = new MultipleMutexRequest(
                mutexes: mutexes,
                success_callback: () =>
                {
                    int width = 800 + (IClickableMenu.borderWidth * 2);
                    int height = 600 + (IClickableMenu.borderWidth * 2);
                    Vector2 pos = Utility.getTopLeftPositionForCenteringOnScreen(width, height);
                    Game1.activeClickableMenu = new CraftingPage((int)pos.X, (int)pos.Y, width, height, false, true, this.EnabledChests)
                    {
                        exitFunction = () => { multipleMutexRequest?.ReleaseLocks(); },
                    };
                },
                failure_callback: () =>
                {
                    Game1.showRedMessage(Game1.content.LoadString("Strings\\UI:Workbench_Chest_Warning"));
                });
            this._inputHelper.SuppressActiveKeybinds(craftingButton);
        }
    }
}