namespace XSPlus.Features
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading.Tasks;
    using Common.Helpers;
    using CommonHarmony.Services;
    using Services;
    using StardewModdingAPI;
    using StardewModdingAPI.Events;
    using StardewModdingAPI.Utilities;
    using StardewValley;
    using StardewValley.Objects;

    /// <inheritdoc />
    internal class StashToChestFeature : FeatureWithParam<string>
    {
        private readonly PerScreen<IList<Chest>> _cachedGameChests = new();
        private readonly PerScreen<IList<Chest>> _cachedPlayerChests = new();
        private readonly FilterItemsFeature _filterItems;
        private readonly ModConfigService _modConfigService;

        private StashToChestFeature(ModConfigService modConfigService, FilterItemsFeature filterItems)
            : base("StashToChest", modConfigService)
        {
            this._modConfigService = modConfigService;
            this._filterItems = filterItems;
        }

        /// <summary>
        ///     Gets or sets the instance of <see cref="StashToChestFeature" />.
        /// </summary>
        private static StashToChestFeature Instance { get; set; }

        /// <summary>
        ///     Returns and creates if needed an instance of the <see cref="StashToChestFeature" /> class.
        /// </summary>
        /// <param name="serviceManager">Service manager to request shared services.</param>
        /// <returns>Returns an instance of the <see cref="StashToChestFeature" /> class.</returns>
        public static async Task<StashToChestFeature> Create(ServiceManager serviceManager)
        {
            return StashToChestFeature.Instance ??= new(
                await serviceManager.Get<ModConfigService>(),
                await serviceManager.Get<FilterItemsFeature>());
        }

        /// <inheritdoc />
        public override void Activate()
        {
            // Events
            Events.Input.ButtonsChanged += this.OnButtonsChanged;
            Events.Player.InventoryChanged += this.OnInventoryChanged;
            Events.Player.Warped += this.OnWarped;
        }

        /// <inheritdoc />
        public override void Deactivate()
        {
            // Events
            Events.Input.ButtonsChanged -= this.OnButtonsChanged;
            Events.Player.InventoryChanged -= this.OnInventoryChanged;
            Events.Player.Warped -= this.OnWarped;
        }

        /// <inheritdoc />
        [SuppressMessage("ReSharper", "HeapView.BoxingAllocation", Justification = "Required for enumerating this collection.")]
        internal override bool IsEnabledForItem(Item item)
        {
            if (!base.IsEnabledForItem(item) || item is not Chest chest || !chest.playerChest.Value || !this.TryGetValueForItem(item, out var range) || !this._filterItems.HasFilterItems(chest))
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

            param = this._modConfigService.ModConfig.StashingRange;
            return !string.IsNullOrWhiteSpace(param);
        }

        internal void ResetCachedChests(bool playerChest = false, bool gameChest = false)
        {
            if (playerChest)
            {
                this._cachedPlayerChests.Value = null;
            }

            if (gameChest)
            {
                this._cachedGameChests.Value = null;
            }
        }

        private void OnInventoryChanged(object sender, InventoryChangedEventArgs e)
        {
            if (!e.IsLocalPlayer)
            {
                return;
            }

            this.ResetCachedChests(true);
        }

        private void OnWarped(object sender, WarpedEventArgs e)
        {
            this.ResetCachedChests(gameChest: true);
        }

        /// <summary>Stash inventory items into all supported chests.</summary>
        private void OnButtonsChanged(object sender, ButtonsChangedEventArgs e)
        {
            if (!Context.IsPlayerFree || !this._modConfigService.ModConfig.StashItems.JustPressed())
            {
                return;
            }

            this._cachedPlayerChests.Value ??= Game1.player.Items.OfType<Chest>().Where(this.IsEnabledForItem).ToList();
            this._cachedGameChests.Value ??= XSPlus.AccessibleChests.Where(this.IsEnabledForItem).ToList();

            if (!this._cachedGameChests.Value.Any() && !this._cachedPlayerChests.Value.Any())
            {
                Log.Trace("No eligible chests found to stash items into");
                return;
            }

            Log.Trace("Trying to stash items into chest");
            for (var i = Game1.player.Items.Count - 1; i >= 0; i--)
            {
                var item = Game1.player.Items[i];
                if (item is not null)
                {
                    Game1.player.Items[i] = this.TryAddItem(item);
                    if (Game1.player.Items[i] is not null)
                    {
                        if (Game1.player.Items[i].Stack != item.Stack)
                        {
                            Log.Trace($"Ran out of available space for {item.Name}");
                        }
                        else
                        {
                            Log.Trace($"No eligible storage found for {item.Name}");
                        }
                    }
                }
            }

            Game1.playSound("Ship");
            Input.Suppress(this._modConfigService.ModConfig.StashItems);
        }

        private Item TryAddItem(Item item)
        {
            var stack = (uint)item.Stack;

            foreach (var chest in this._cachedPlayerChests.Value)
            {
                if (!this._filterItems.Matches(chest, item))
                {
                    continue;
                }

                // Attempt to add item into chest
                Log.Trace($"Adding {item.Name} to chest in player inventory: {chest.DisplayName}");
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

            foreach (var chest in this._cachedGameChests.Value)
            {
                if (!this._filterItems.Matches(chest, item))
                {
                    continue;
                }

                // Attempt to add item into chest
                Log.Trace($"Adding {item.Name} to chest at location: {chest.DisplayName}");
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