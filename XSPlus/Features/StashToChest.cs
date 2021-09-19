namespace XSPlus.Features
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Common.Extensions;
    using HarmonyLib;
    using StardewModdingAPI;
    using StardewModdingAPI.Events;
    using StardewModdingAPI.Utilities;
    using StardewValley;
    using StardewValley.Objects;

    /// <inheritdoc />
    internal class StashToChest : FeatureWithParam<string>
    {
        private readonly IInputHelper _inputHelper;
        private readonly Func<string> _getSearchTagSymbol;
        private readonly Func<KeybindList> _getStashingButton;
        private readonly Func<string> _getConfigRange;
        private readonly PerScreen<List<Chest>> _cachedEnabledChests = new();

        /// <summary>Initializes a new instance of the <see cref="StashToChest"/> class.</summary>
        /// <param name="inputHelper">API for changing state of input.</param>
        /// <param name="getStashingButton">Get method for configured stashing button.</param>
        /// <param name="getConfigRange">Get method for configured default range.</param>
        /// <param name="getSearchTagSymbol">Get method for configured search tag symbol.</param>
        public StashToChest(IInputHelper inputHelper, Func<KeybindList> getStashingButton, Func<string> getConfigRange, Func<string> getSearchTagSymbol)
            : base("StashToChest")
        {
            this._inputHelper = inputHelper;
            this._getStashingButton = getStashingButton;
            this._getConfigRange = getConfigRange;
            this._getSearchTagSymbol = getSearchTagSymbol;
        }

        private List<Chest> EnabledChests
        {
            get => this._cachedEnabledChests.Value ??= Game1.player.Items.OfType<Chest>()
                .Union(XSPlus.AccessibleChests)
                .Where(this.IsEnabledForItem)
                .Distinct()
                .ToList();
        }

        /// <inheritdoc/>
        public override void Activate(IModEvents modEvents, Harmony harmony)
        {
            // Events
            modEvents.Player.InventoryChanged += this.OnInventoryChanged;
            modEvents.Player.Warped += this.OnWarped;
            modEvents.Input.ButtonsChanged += this.OnButtonsChanged;
        }

        /// <inheritdoc/>
        public override void Deactivate(IModEvents modEvents, Harmony harmony)
        {
            // Events
            modEvents.Player.InventoryChanged -= this.OnInventoryChanged;
            modEvents.Player.Warped -= this.OnWarped;
            modEvents.Input.ButtonsChanged -= this.OnButtonsChanged;
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

        /// <summary>Stash inventory items into all supported chests.</summary>
        private void OnButtonsChanged(object sender, ButtonsChangedEventArgs e)
        {
            KeybindList stashingButton = this._getStashingButton();

            if (!Context.IsPlayerFree || !stashingButton.JustPressed() || !this.EnabledChests.Any())
            {
                return;
            }

            for (int i = Game1.player.Items.Count - 1; i >= 0; i--)
            {
                Item item = Game1.player.Items[i];
                if (item is null)
                {
                    continue;
                }

                uint stack = (uint)item.Stack;
                foreach (Chest chest in this.EnabledChests)
                {
                    bool allowList = FilterItems.Instance.IsEnabledForItem(chest);
                    chest.GetModDataList("Favorites", out var favorites);

                    switch (favorites.Count)
                    {
                        // Skip chest if it has favorites and none are matched
                        case > 0 when !item.SearchTags(favorites, this._getSearchTagSymbol()):
                        // Skip chest if no favorites and no built-in filter
                        case 0 when !allowList:
                            continue;
                    }

                    // Attempt to add item into chest
                    Item tmp = chest.addItem(item);
                    if (tmp == null)
                    {
                        Game1.player.Items[i] = null;
                        break;
                    }

                    if (tmp.Stack != stack)
                    {
                        item.Stack = tmp.Stack;
                    }
                }
            }

            Game1.playSound("Ship");
            this._inputHelper.SuppressActiveKeybinds(stashingButton);
        }
    }
}