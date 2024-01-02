namespace StardewMods.Common.Services.Integrations.ExpandedStorage;

internal sealed class ExpandedStorageIntegration : ModIntegration<IExpandedStorageApi>
{
    private const string ModUniqueId = "furyx639.ExpandedStorage";

    /// <summary>Initializes a new instance of the <see cref="ExpandedStorageIntegration" /> class.</summary>
    /// <param name="modRegistry">SMAPI's mod registry.</param>
    public ExpandedStorageIntegration(IModRegistry modRegistry)
        : base(modRegistry, ExpandedStorageIntegration.ModUniqueId)
    {
        // Nothing
    }
}