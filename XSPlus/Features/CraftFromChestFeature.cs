namespace XSPlus.Features
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection.Emit;
    using Common.Helpers;
    using Common.Services;
    using CommonHarmony.Enums;
    using CommonHarmony.Services;
    using HarmonyLib;
    using Services;
    using StardewModdingAPI;
    using StardewModdingAPI.Events;
    using StardewModdingAPI.Utilities;
    using StardewValley;
    using StardewValley.Menus;
    using StardewValley.Objects;

    /// <inheritdoc />
    internal class CraftFromChestFeature : FeatureWithParam<string>
    {
        private readonly PerScreen<IList<Chest>> _cachedPlayerChests = new();
        private readonly PerScreen<MultipleChestCraftingPage> _multipleChestCraftingPage = new();
        private IList<Chest> _cachedGameChests;
        private HarmonyService _harmony;
        private ModConfigService _modConfig;

        private CraftFromChestFeature(ServiceManager serviceManager)
            : base("CraftFromChest", serviceManager)
        {
            // Dependencies
            this.AddDependency<ModConfigService>(service => this._modConfig = service as ModConfigService);
            this.AddDependency<HarmonyService>(
                service =>
                {
                    // Init
                    this._harmony = service as HarmonyService;

                    // Patches
                    this._harmony?.AddPatch(
                        this.ServiceName,
                        AccessTools.Method(typeof(CraftingRecipe), nameof(CraftingRecipe.consumeIngredients)),
                        typeof(CraftFromChestFeature),
                        nameof(CraftFromChestFeature.CraftingRecipe_consumeIngredients_transpiler),
                        PatchType.Transpiler);

                    this._harmony?.AddPatch(
                        this.ServiceName,
                        AccessTools.Method(typeof(CraftingPage), "getContainerContents"),
                        typeof(CraftFromChestFeature),
                        nameof(CraftFromChestFeature.CraftingPage_getContainerContents_postfix),
                        PatchType.Postfix);
                });
        }

        /// <inheritdoc />
        public override void Activate()
        {
            // Events
            this.Helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;
            this.Helper.Events.Player.InventoryChanged += this.OnInventoryChanged;
            this.Helper.Events.Player.Warped += this.OnWarped;
            this.Helper.Events.World.ObjectListChanged += this.OnObjectListChanged;
            this.Helper.Events.Input.ButtonsChanged += this.OnButtonsChanged;

            // Patches
            this._harmony.ApplyPatches(this.ServiceName);
        }

        /// <inheritdoc />
        public override void Deactivate()
        {
            // Events
            this.Helper.Events.GameLoop.UpdateTicked -= this.OnUpdateTicked;
            this.Helper.Events.Player.InventoryChanged -= this.OnInventoryChanged;
            this.Helper.Events.Player.Warped -= this.OnWarped;
            this.Helper.Events.World.ObjectListChanged -= this.OnObjectListChanged;
            this.Helper.Events.Input.ButtonsChanged -= this.OnButtonsChanged;

            // Patches
            this._harmony.UnapplyPatches(this.ServiceName);
        }

        /// <inheritdoc />
        [SuppressMessage("ReSharper", "HeapView.BoxingAllocation", Justification = "Required for enumerating this collection.")]
        internal override bool IsEnabledForItem(Item item)
        {
            if (!base.IsEnabledForItem(item) || item is not Chest chest || !chest.playerChest.Value || !this.TryGetValueForItem(item, out var range))
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

        /// <inheritdoc />
        internal override bool TryGetValueForItem(Item item, out string param)
        {
            if (base.TryGetValueForItem(item, out param))
            {
                return true;
            }

            param = this._modConfig.ModConfig.CraftingRange;
            return !string.IsNullOrWhiteSpace(param);
        }

        [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Naming is determined by Harmony.")]
        [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Type is determined by Harmony.")]
        private static void CraftingPage_getContainerContents_postfix(CraftingPage __instance, ref IList<Item> __result)
        {
            if (__instance._materialContainers is null)
            {
                return;
            }

            __result.Clear();
            var items = new List<Item>();
            foreach (var chest in __instance._materialContainers)
            {
                items.AddRange(chest.GetItemsForPlayer(Game1.player.UniqueMultiplayerID));
            }

            __result = items;
        }

        [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Naming is determined by Harmony.")]
        private static IEnumerable<CodeInstruction> CraftingRecipe_consumeIngredients_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (var instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Ldfld && instruction.operand.Equals(AccessTools.Field(typeof(Chest), nameof(Chest.items))))
                {
                    yield return new(OpCodes.Call, AccessTools.Property(typeof(Game1), nameof(Game1.player)).GetGetMethod());
                    yield return new(OpCodes.Callvirt, AccessTools.Property(typeof(Farmer), nameof(Farmer.UniqueMultiplayerID)).GetGetMethod());
                    yield return new(OpCodes.Callvirt, AccessTools.Method(typeof(Chest), nameof(Chest.GetItemsForPlayer)));
                }
                else
                {
                    yield return instruction;
                }
            }
        }

        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (this._multipleChestCraftingPage.Value is null || this._multipleChestCraftingPage.Value.Timeout)
            {
                return;
            }

            this._multipleChestCraftingPage.Value.UpdateChests();
        }

        private void OnInventoryChanged(object sender, InventoryChangedEventArgs e)
        {
            if (!e.IsLocalPlayer)
            {
                return;
            }

            this._cachedPlayerChests.Value = null;
        }

        private void OnWarped(object sender, WarpedEventArgs e)
        {
            this._cachedGameChests = null;
        }

        private void OnObjectListChanged(object sender, ObjectListChangedEventArgs e)
        {
            this._cachedGameChests = null;
        }

        /// <summary>Open crafting menu for all chests in inventory.</summary>
        private void OnButtonsChanged(object sender, ButtonsChangedEventArgs e)
        {
            if (!Context.IsPlayerFree || !this._modConfig.ModConfig.OpenCrafting.JustPressed())
            {
                return;
            }

            this._cachedPlayerChests.Value ??= Game1.player.Items.OfType<Chest>().Where(this.IsEnabledForItem).ToList();
            this._cachedGameChests ??= XSPlus.AccessibleChests.Where(this.IsEnabledForItem).ToList();

            if (!this._cachedGameChests.Any() && !this._cachedPlayerChests.Value.Any())
            {
                Log.Trace("No eligible chests found to craft items from");
                return;
            }

            this._multipleChestCraftingPage.Value = new(this._cachedPlayerChests.Value.Concat(this._cachedGameChests).ToList());
            this.Helper.Input.SuppressActiveKeybinds(this._modConfig.ModConfig.OpenCrafting);
        }

        private class MultipleChestCraftingPage
        {
            private const int TimeOut = 60;
            private readonly List<Chest> _chests;
            private readonly MultipleMutexRequest _multipleMutexRequest;
            private int _timeOut = MultipleChestCraftingPage.TimeOut;

            public MultipleChestCraftingPage(List<Chest> chests)
            {
                this._chests = chests.Where(chest => !chest.mutex.IsLocked()).ToList();
                var mutexes = this._chests.Select(chest => chest.mutex).ToList();
                this._multipleMutexRequest = new(
                    mutexes,
                    this.SuccessCallback,
                    this.FailureCallback);
            }

            public bool Timeout
            {
                get => this._timeOut <= 0;
            }

            public void UpdateChests()
            {
                if (--this._timeOut <= 0)
                {
                    return;
                }

                foreach (var chest in this._chests)
                {
                    chest.mutex.Update(Game1.getOnlineFarmers());
                }
            }

            private void SuccessCallback()
            {
                this._timeOut = 0;
                var width = 800 + IClickableMenu.borderWidth * 2;
                var height = 600 + IClickableMenu.borderWidth * 2;
                var pos = Utility.getTopLeftPositionForCenteringOnScreen(width, height);
                Game1.activeClickableMenu = new CraftingPage((int)pos.X, (int)pos.Y, width, height, false, true, this._chests)
                {
                    exitFunction = this.ExitFunction,
                };
            }

            private void FailureCallback()
            {
                Game1.showRedMessage(Game1.content.LoadString("Strings\\UI:Workbench_Chest_Warning"));
                this._timeOut = 0;
            }

            private void ExitFunction()
            {
                this._multipleMutexRequest.ReleaseLocks();
                this._timeOut = MultipleChestCraftingPage.TimeOut;
            }
        }
    }
}