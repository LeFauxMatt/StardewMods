namespace StardewMods.BetterChests.Framework.Services.Factory;

using StardewMods.BetterChests.Framework.Services.Transient;
using StardewMods.Common.Services.Integrations.FuryCore;

/// <summary>Represents a factory class for creating instances of the ItemMatcher class.</summary>
internal sealed class ItemMatcherFactory : BaseService
{
    private readonly Func<ItemMatcher> itemMatcherGetter;

    /// <summary>Initializes a new instance of the <see cref="ItemMatcherFactory" /> class.</summary>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="itemMatcherGetter">Function which returns a new item matcher.</param>
    public ItemMatcherFactory(ILog log, Func<ItemMatcher> itemMatcherGetter)
        : base(log) =>
        this.itemMatcherGetter = itemMatcherGetter;

    /// <summary>Retrieves a single ItemMatcher.</summary>
    /// <returns>The ItemMatcher object.</returns>
    public ItemMatcher GetDefault() => this.itemMatcherGetter();

    /// <summary>Retrieves a single ItemMatcher for use in search.</summary>
    /// <returns>The ItemMatcher object.</returns>
    public ItemMatcher GetOneForSearch()
    {
        var itemMatcher = this.itemMatcherGetter();
        itemMatcher.AllowPartial = true;
        itemMatcher.OnlyTags = false;
        return itemMatcher;
    }
}