namespace StardewMods.BetterChests.Framework.Services;

using StardewMods.BetterChests.Framework.Enums;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.BetterChests.Framework.Models;
using StardewMods.BetterChests.Framework.Models.StorageOptions;
using StardewMods.Common.Services.Integrations.FuryCore;

/// <inheritdoc cref="StardewMods.BetterChests.Framework.Interfaces.IModConfig" />
internal sealed class ConfigManager : BaseService, IModConfig
{
    private readonly FeatureManager featureManager;
    private readonly IModHelper modHelper;

    private IModConfig modConfig;

    /// <summary>Initializes a new instance of the <see cref="ConfigManager" /> class.</summary>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="modHelper">Dependency for events, input, and content.</param>
    /// <param name="featureManager">Dependency used for managing features.</param>
    public ConfigManager(ILog log, IModHelper modHelper, FeatureManager featureManager)
        : base(log)
    {
        this.modHelper = modHelper;
        this.featureManager = featureManager;
        this.modConfig = this.modHelper.ReadConfig<DefaultConfig>();
    }

    /// <inheritdoc />
    public DefaultStorageOptions DefaultOptions => this.modConfig.DefaultOptions;

    /// <inheritdoc />
    public int CarryChestLimit => this.modConfig.CarryChestLimit;

    /// <inheritdoc />
    public int CarryChestSlowLimit => this.modConfig.CarryChestSlowLimit;

    /// <inheritdoc />
    public Method CategorizeChestMethod => this.modConfig.CategorizeChestMethod;

    /// <inheritdoc />
    public Controls Controls => this.modConfig.Controls;

    /// <inheritdoc />
    public HashSet<string> CraftFromChestDisableLocations => this.modConfig.CraftFromChestDisableLocations;

    /// <inheritdoc />
    public int CraftFromChestDistance => this.modConfig.CraftFromChestDistance;

    /// <inheritdoc />
    public RangeOption CraftFromWorkbench => this.modConfig.CraftFromWorkbench;

    /// <inheritdoc />
    public int CraftFromWorkbenchDistance => this.modConfig.CraftFromWorkbenchDistance;

    /// <inheritdoc />
    public bool Experimental => this.modConfig.Experimental;

    /// <inheritdoc />
    public Method InventoryTabMethod => this.modConfig.InventoryTabMethod;

    /// <inheritdoc />
    public bool LabelChest => this.modConfig.LabelChest;

    /// <inheritdoc />
    public Option LockItem => this.modConfig.LockItem;

    /// <inheritdoc />
    public bool LockItemHold => this.modConfig.LockItemHold;

    /// <inheritdoc />
    public Method SearchItemsMethod => this.modConfig.SearchItemsMethod;

    /// <inheritdoc />
    public char SearchTagSymbol => this.modConfig.SearchTagSymbol;

    /// <inheritdoc />
    public char SearchNegationSymbol => this.modConfig.SearchNegationSymbol;

    /// <inheritdoc />
    public HashSet<string> StashToChestDisableLocations => this.modConfig.StashToChestDisableLocations;

    /// <inheritdoc />
    public int StashToChestDistance => this.modConfig.StashToChestDistance;

    /// <inheritdoc />
    public bool StashToChestStacks => this.modConfig.StashToChestStacks;

    /// <summary>Returns a new instance of IModConfig by reading the DefaultConfig from the mod helper.</summary>
    /// <returns>The new instance of IModConfig.</returns>
    public DefaultConfig GetNew() => this.modHelper.ReadConfig<DefaultConfig>();

    /// <summary>Reloads the configuration by reading the updated configuration file.</summary>
    public void Reload() => this.modConfig = this.modHelper.ReadConfig<DefaultConfig>();

    /// <summary>Saves the provided config.</summary>
    /// <param name="config">The config object to be saved.</param>
    public void Save(DefaultConfig config)
    {
        this.modHelper.WriteConfig(config);
        this.modConfig = config;
        this.featureManager.Activate();
    }
}