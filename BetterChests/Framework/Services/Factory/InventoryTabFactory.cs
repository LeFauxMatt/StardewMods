namespace StardewMods.BetterChests.Framework.Services.Factory;

using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Utilities;
using StardewMods.BetterChests.Framework.Models;
using StardewMods.Common.Services.Integrations.FuryCore;

/// <summary>Represents a factory class that creates and manages inventory tabs.</summary>
internal sealed class InventoryTabFactory : BaseService
{
    private readonly IGameContentHelper gameContentHelper;
    private readonly ItemMatcherFactory itemMatcherFactory;
    private readonly Lazy<Dictionary<string, InventoryTabData>> tabData;
    private readonly PerScreen<Dictionary<string, InventoryTab>> tabs = new(() => []);
    private readonly ITranslationHelper translationHelper;

    /// <summary>Initializes a new instance of the <see cref="InventoryTabFactory" /> class.</summary>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="gameContentHelper">Dependency used for loading game assets.</param>
    /// <param name="itemMatcherFactory">Dependency used for getting an ItemMatcher.</param>
    /// <param name="translationHelper">Dependency used for accessing translations.</param>
    public InventoryTabFactory(
        ILog log,
        IGameContentHelper gameContentHelper,
        ItemMatcherFactory itemMatcherFactory,
        ITranslationHelper translationHelper)
        : base(log)
    {
        this.gameContentHelper = gameContentHelper;
        this.itemMatcherFactory = itemMatcherFactory;
        this.translationHelper = translationHelper;
        this.tabData = new Lazy<Dictionary<string, InventoryTabData>>(this.GetTabData);
    }

    /// <summary>Tries to get an inventory tab with the specified name.</summary>
    /// <param name="name">The name of the tab.</param>
    /// <param name="tab">When this method returns, contains the tab with the specified name, if found; otherwise, null.</param>
    /// <returns><c>true</c> if a tab with the specified name is found; otherwise, <c>false</c>.</returns>
    public bool TryGetOne(string name, [NotNullWhen(true)] out InventoryTab? tab)
    {
        if (this.tabs.Value.TryGetValue(name, out tab))
        {
            return true;
        }

        if (!this.tabData.Value.TryGetValue(name, out var data))
        {
            tab = null;
            return false;
        }

        var itemMatcher = this.itemMatcherFactory.GetDefault();
        itemMatcher.SearchText = string.Join(' ', data.Rules);
        tab = new InventoryTab(
            name,
            this.translationHelper.Get($"tab.{name}.Name").Default(name),
            this.gameContentHelper.Load<Texture2D>(data.Path),
            data.Index,
            itemMatcher);

        this.tabs.Value[name] = tab;
        return true;
    }

    private Dictionary<string, InventoryTabData> GetTabData() =>
        this.gameContentHelper.Load<Dictionary<string, InventoryTabData>>(AssetHandler.TabDataPath);
}