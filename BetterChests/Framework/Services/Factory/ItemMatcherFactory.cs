namespace StardewMods.BetterChests.Framework.Services.Factory;

using StardewMods.BetterChests.Framework.Services.Transient;
using StardewMods.Common.Interfaces;

/// <summary>Represents a factory class for creating instances of the ItemMatcher class.</summary>
internal sealed class ItemMatcherFactory : BaseService
{
    private readonly Func<ItemMatcher> itemMatcherGetter;

    /// <summary>Initializes a new instance of the <see cref="ItemMatcherFactory" /> class.</summary>
    /// <param name="logging">Dependency used for logging debug information to the console.</param>
    /// <param name="itemMatcherGetter">Function which returns a new item matcher.</param>
    public ItemMatcherFactory(ILogging logging, Func<ItemMatcher> itemMatcherGetter)
        : base(logging) =>
        this.itemMatcherGetter = itemMatcherGetter;

    /// <summary>Retrieves a single ItemMatcher.</summary>
    /// <returns>The ItemMatcher object.</returns>
    public ItemMatcher GetDefault() => this.itemMatcherGetter();

    /// <summary>Retrieves a single ItemMatcher for use in search.</summary>
    /// <returns>The ItemMatcher object.</returns>
    public ItemMatcher GetSearch()
    {
        var itemMatcher = this.itemMatcherGetter();
        itemMatcher.AllowPartial = true;
        itemMatcher.OnlyTags = false;
        return itemMatcher;
    }
}
