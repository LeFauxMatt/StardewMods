namespace Common.Integrations.FuryCoreConfigurator;

using StardewModdingAPI;

/// <inheritdoc />
internal class FuryCoreConfiguratorIntegration : ModIntegration<IFuryCoreConfiguratorApi>
{
    private const string ModUniqueId = "furyx639.FuryCoreConfigurator";

    /// <summary>
    ///     Initializes a new instance of the <see cref="FuryCoreConfiguratorIntegration" /> class.
    /// </summary>
    /// <param name="modRegistry">SMAPI's mod registry.</param>
    public FuryCoreConfiguratorIntegration(IModRegistry modRegistry)
        : base(modRegistry, FuryCoreConfiguratorIntegration.ModUniqueId)
    {
    }
}