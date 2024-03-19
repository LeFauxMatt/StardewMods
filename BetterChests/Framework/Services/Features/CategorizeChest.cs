namespace StardewMods.BetterChests.Framework.Services.Features;

using System.Runtime.CompilerServices;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.BetterChests.Framework.Models.Events;
using StardewMods.BetterChests.Framework.Services.Factory;
using StardewMods.BetterChests.Framework.Services.Transient;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Services.Integrations.BetterChests.Enums;
using StardewMods.Common.Services.Integrations.BetterChests.Interfaces;
using StardewMods.Common.Services.Integrations.FauxCore;

/// <summary>Restricts what items can be added into a chest.</summary>
internal sealed class CategorizeChest : BaseFeature<CategorizeChest>
{
    private readonly ConditionalWeakTable<IStorageContainer, ItemMatcher> cachedItemMatchers = new();
    private readonly ItemGrabMenuManager itemGrabMenuManager;
    private readonly ItemMatcherFactory itemMatcherFactory;

    /// <summary>Initializes a new instance of the <see cref="CategorizeChest" /> class.</summary>
    /// <param name="eventManager">Dependency used for managing events.</param>
    /// <param name="itemGrabMenuManager">Dependency used for managing the item grab menu.</param>
    /// <param name="itemMatcherFactory">Dependency used for getting an ItemMatcher.</param>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="modConfig">Dependency used for accessing config data.</param>
    public CategorizeChest(
        IEventManager eventManager,
        ItemGrabMenuManager itemGrabMenuManager,
        ItemMatcherFactory itemMatcherFactory,
        ILog log,
        IManifest manifest,
        IModConfig modConfig)
        : base(eventManager, log, manifest, modConfig)
    {
        this.itemGrabMenuManager = itemGrabMenuManager;
        this.itemMatcherFactory = itemMatcherFactory;
    }

    /// <inheritdoc />
    public override bool ShouldBeActive => this.Config.DefaultOptions.CategorizeChest != FeatureOption.Disabled;

    /// <inheritdoc />
    protected override void Activate()
    {
        // Events
        this.Events.Subscribe<ItemGrabMenuChangedEventArgs>(this.OnItemGrabMenuChanged);
        this.Events.Subscribe<ItemTransferringEventArgs>(this.OnItemTransferring);
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        // Events
        this.Events.Unsubscribe<ItemGrabMenuChangedEventArgs>(this.OnItemGrabMenuChanged);
        this.Events.Unsubscribe<ItemTransferringEventArgs>(this.OnItemTransferring);
    }

    private void OnItemGrabMenuChanged(ItemGrabMenuChangedEventArgs e)
    {
        if (this.itemGrabMenuManager.Top.Container?.Options.CategorizeChest == FeatureOption.Enabled)
        {
            var itemMatcher = this.GetOrCreateItemMatcher(this.itemGrabMenuManager.Top.Container);
            this.itemGrabMenuManager.Bottom.AddHighlightMethod(itemMatcher.MatchesFilter);
        }

        if (this.itemGrabMenuManager.Bottom.Container?.Options.CategorizeChest == FeatureOption.Enabled)
        {
            var itemMatcher = this.GetOrCreateItemMatcher(this.itemGrabMenuManager.Bottom.Container);
            this.itemGrabMenuManager.Top.AddHighlightMethod(itemMatcher.MatchesFilter);
        }
    }

    private void OnItemTransferring(ItemTransferringEventArgs e)
    {
        if (e.Into.Options.CategorizeChest != FeatureOption.Enabled)
        {
            return;
        }

        // Allow transfer if existing stacks are allowed and item is already in the chest
        if (this.Config.StashToChestStacks && e.Into.Items.ContainsId(e.Item.ItemId))
        {
            return;
        }

        // Allow transfer if item matches categories
        var itemMatcher = this.GetOrCreateItemMatcher(e.Into);
        if (!itemMatcher.IsEmpty)
        {
            if (itemMatcher.MatchesFilter(e.Item))
            {
                return;
            }

            e.PreventTransfer();
            return;
        }

        if (!e.IsForced)
        {
            e.PreventTransfer();
        }
    }

    private ItemMatcher GetOrCreateItemMatcher(IStorageContainer container)
    {
        if (!this.cachedItemMatchers.TryGetValue(container, out var itemMatcher))
        {
            itemMatcher = this.itemMatcherFactory.GetDefault();
        }

        if (itemMatcher.IsEmpty && container.Options.CategorizeChestTags.Any())
        {
            itemMatcher.SearchText = string.Join(' ', container.Options.CategorizeChestTags);
        }

        this.cachedItemMatchers.AddOrUpdate(container, itemMatcher);
        return itemMatcher;
    }
}