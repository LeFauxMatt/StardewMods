namespace StardewMods.BetterChests.Framework.Services.Factory;

using StardewMods.BetterChests.Framework.Services.Transient;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.FuryCore;

/// <summary>Represents a factory class for creating instances of the ItemMatcher class.</summary>
internal sealed class ItemMatcherFactory : BaseService
{
    private readonly Func<ItemMatcher> getItemMatcher;

    /// <summary>Initializes a new instance of the <see cref="ItemMatcherFactory" /> class.</summary>
    /// <param name="getItemMatcher">Function which returns a new item matcher.</param>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    public ItemMatcherFactory(Func<ItemMatcher> getItemMatcher, ILog log, IManifest manifest)
        : base(log, manifest) =>
        this.getItemMatcher = getItemMatcher;

    /// <summary>Retrieves a single ItemMatcher.</summary>
    /// <returns>The ItemMatcher object.</returns>
    public ItemMatcher GetDefault() => this.getItemMatcher();

    /// <summary>Retrieves a single ItemMatcher for use in search.</summary>
    /// <returns>The ItemMatcher object.</returns>
    public ItemMatcher GetOneForSearch()
    {
        var itemMatcher = this.getItemMatcher();
        itemMatcher.AllowPartial = true;
        itemMatcher.OnlyTags = false;
        return itemMatcher;
    }
}