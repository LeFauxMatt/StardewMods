namespace XSPlus.Features
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Common.Helpers;
    using Common.Helpers.ItemMatcher;
    using Services;
    using StardewModdingAPI;
    using StardewModdingAPI.Events;
    using StardewModdingAPI.Utilities;
    using StardewValley;
    using StardewValley.Objects;

    /// <inheritdoc />
    internal class StashToChestFeature : FeatureWithParam<string>
    {
        private readonly ModConfigService _modConfigService;
        private readonly PerScreen<List<Chest>> _cachedEnabledChests = new();
        private readonly PerScreen<IList<Chest>> _cachedPlayerChests = new();
        private readonly PerScreen<IList<Chest>> _cachedGameChests = new();

        private StashToChestFeature(ModConfigService modConfigService)
            : base("StashToChest")
        {
            this._modConfigService = modConfigService;
        }

        /// <summary>
        /// Gets or sets the instance of <see cref="StashToChestFeature"/>.
        /// </summary>
        private static StashToChestFeature Instance { get; set; }

        private List<Chest> EnabledChests
        {
            get
            {
                this._cachedPlayerChests.Value ??= Game1.player.Items.OfType<Chest>().Where(this.IsEnabledForItem).ToList();
                this._cachedGameChests.Value ??= XSPlus.AccessibleChests.Where(this.IsEnabledForItem).ToList();
                return this._cachedEnabledChests.Value ??= this._cachedPlayerChests.Value.Union(this._cachedGameChests.Value).ToList();
            }
        }

        /// <inheritdoc/>
        public override void Activate()
        {
            // Events
            Events.Input.ButtonsChanged += this.OnButtonsChanged;
            Events.Player.InventoryChanged += this.OnInventoryChanged;
            Events.Player.Warped += this.OnWarped;
        }

        /// <inheritdoc/>
        public override void Deactivate()
        {
            // Events
            Events.Input.ButtonsChanged -= this.OnButtonsChanged;
            Events.Player.InventoryChanged -= this.OnInventoryChanged;
            Events.Player.Warped -= this.OnWarped;
        }

        /// <summary>
        /// Returns and creates if needed an instance of the <see cref="StashToChestFeature"/> class.
        /// </summary>
        /// <param name="serviceManager">Service manager to request shared services.</param>
        /// <returns>Returns an instance of the <see cref="StashToChestFeature"/> class.</returns>
        public static StashToChestFeature GetSingleton(ServiceManager serviceManager)
        {
            var modConfigService = serviceManager.RequestService<ModConfigService>();
            return StashToChestFeature.Instance ??= new StashToChestFeature(modConfigService);
        }

        /// <inheritdoc/>
        [SuppressMessage("ReSharper", "HeapView.BoxingAllocation", Justification = "Required for enumerating this collection.")]
        protected internal override bool IsEnabledForItem(Item item)
        {
            if (!base.IsEnabledForItem(item) || item is not Chest chest || !chest.playerChest.Value || !this.TryGetValueForItem(item, out string range))
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

            param = this._modConfigService.ModConfig.StashingRange;
            return !string.IsNullOrWhiteSpace(param);
        }

        private void OnInventoryChanged(object sender, InventoryChangedEventArgs e)
        {
            if (!e.IsLocalPlayer)
            {
                return;
            }

            this._cachedPlayerChests.Value = null;
            this._cachedEnabledChests.Value = null;
        }

        private void OnWarped(object sender, WarpedEventArgs e)
        {
            this._cachedGameChests.Value = null;
            this._cachedEnabledChests.Value = null;
        }

        /// <summary>Stash inventory items into all supported chests.</summary>
        private void OnButtonsChanged(object sender, ButtonsChangedEventArgs e)
        {
            if (!Context.IsPlayerFree || !this._modConfigService.ModConfig.StashItems.JustPressed() || !this.EnabledChests.Any())
            {
                return;
            }

            for (int i = Game1.player.Items.Count - 1; i >= 0; i--)
            {
                var item = Game1.player.Items[i];
                if (item is not null)
                {
                    Game1.player.Items[i] = this.TryAddItem(item);
                }
            }

            Game1.playSound("Ship");
            Input.Suppress(this._modConfigService.ModConfig.StashItems);
        }

        private Item TryAddItem(Item item)
        {
            var itemMatcher = new ItemMatcher(this._modConfigService.ModConfig.SearchTagSymbol);
            uint stack = (uint)item.Stack;
            foreach (var chest in this.EnabledChests)
            {
                bool allowList = FilterItemsFeature.Instance.IsEnabledForItem(chest);

                if (chest.modData.TryGetValue($"{XSPlus.ModPrefix}/FilterItems", out string filterItems))
                {
                    itemMatcher.SetSearch(filterItems);

                    // Skip chest if per-chest filter does not match
                    if (!itemMatcher.Matches(item))
                    {
                        continue;
                    }
                }
                else if (!allowList)
                {
                    // Skip chest if no built-in filter and no per-chest filter
                    continue;
                }

                // Attempt to add item into chest
                var tmp = chest.addItem(item);
                if (tmp is null || tmp.Stack <= 0)
                {
                    return null;
                }

                if (tmp.Stack != stack)
                {
                    item.Stack = tmp.Stack;
                }
            }

            return item;
        }
    }
}