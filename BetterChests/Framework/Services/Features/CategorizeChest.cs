namespace StardewMods.BetterChests.Framework.Services.Features;

using System.Runtime.CompilerServices;
using StardewMods.BetterChests.Framework.Models.Events;
using StardewMods.BetterChests.Framework.Services.Factory;
using StardewMods.BetterChests.Framework.Services.Transient;
using StardewMods.Common.Services.Integrations.BetterChests.Enums;
using StardewMods.Common.Services.Integrations.BetterChests.Interfaces;
using StardewMods.Common.Services.Integrations.FuryCore;

/// <summary>Restricts what items can be added into a chest.</summary>
internal sealed class CategorizeChest : BaseFeature<CategorizeChest>
{
    private readonly ConditionalWeakTable<IStorageContainer, ItemMatcher> cachedItemMatchers = new();
    private readonly ContainerHandler containerHandler;
    private readonly ItemGrabMenuManager itemGrabMenuManager;
    private readonly ItemMatcherFactory itemMatcherFactory;

    /// <summary>Initializes a new instance of the <see cref="CategorizeChest" /> class.</summary>
    /// <param name="configManager">Dependency used for accessing config data.</param>
    /// <param name="containerHandler">Dependency used for handling operations between containers.</param>
    /// <param name="itemGrabMenuManager">Dependency used for managing the item grab menu.</param>
    /// <param name="itemMatcherFactory">Dependency used for getting an ItemMatcher.</param>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    public CategorizeChest(
        ConfigManager configManager,
        ContainerHandler containerHandler,
        ItemGrabMenuManager itemGrabMenuManager,
        ItemMatcherFactory itemMatcherFactory,
        ILog log,
        IManifest manifest)
        : base(log, manifest, configManager)
    {
        this.containerHandler = containerHandler;
        this.itemGrabMenuManager = itemGrabMenuManager;
        this.itemMatcherFactory = itemMatcherFactory;
    }

    /// <inheritdoc />
    public override bool ShouldBeActive => this.Config.DefaultOptions.CategorizeChest != FeatureOption.Disabled;

    /// <inheritdoc />
    protected override void Activate()
    {
        // Events
        this.itemGrabMenuManager.ItemGrabMenuChanged += this.OnItemGrabMenuChanged;
        this.containerHandler.ItemTransferring += this.OnItemTransferring;
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        // Events
        this.itemGrabMenuManager.ItemGrabMenuChanged -= this.OnItemGrabMenuChanged;
        this.containerHandler.ItemTransferring -= this.OnItemTransferring;
    }

    private void OnItemGrabMenuChanged(object? sender, ItemGrabMenuChangedEventArgs e)
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

    private void OnItemTransferring(object? sender, ItemTransferringEventArgs e)
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
        if (!itemMatcher.IsEmpty && !itemMatcher.MatchesFilter(e.Item))
        {
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