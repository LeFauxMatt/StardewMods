namespace StardewMods.BetterChests.Framework.Services.Factory;

using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.BetterChests.Framework.Services.Transient;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.FauxCore;

/// <summary>Represents a factory class for creating instances of the ItemMatcher class.</summary>
internal sealed class ItemMatcherFactory : BaseService
{
    private readonly Func<IModConfig> getConfig;
    private readonly ITranslationHelper translationHelper;

    /// <summary>Initializes a new instance of the <see cref="ItemMatcherFactory" /> class.</summary>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="getConfig">Dependency used for accessing config data.</param>
    /// <param name="translationHelper">Dependency used for accessing translations.</param>
    public ItemMatcherFactory(
        ILog log,
        IManifest manifest,
        Func<IModConfig> getConfig,
        ITranslationHelper translationHelper)
        : base(log, manifest)
    {
        this.getConfig = getConfig;
        this.translationHelper = translationHelper;
    }

    /// <summary>Retrieves a single ItemMatcher.</summary>
    /// <returns>The ItemMatcher object.</returns>
    public ItemMatcher GetDefault() => new('!', '#', this.translationHelper);

    /// <summary>Retrieves a single ItemMatcher for use in search.</summary>
    /// <returns>The ItemMatcher object.</returns>
    public ItemMatcher GetOneForSearch()
    {
        var modConfig = this.getConfig();
        return new ItemMatcher(modConfig.SearchNegationSymbol, modConfig.SearchTagSymbol, this.translationHelper)
        {
            AllowPartial = true,
            OnlyTags = false,
        };
    }
}