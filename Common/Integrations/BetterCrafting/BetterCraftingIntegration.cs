namespace StardewMods.Common.Integrations.BetterCrafting;

/// <inheritdoc />
internal sealed class BetterCraftingIntegration : ModIntegration<IBetterCrafting>
{
    private const string ModUniqueId = "leclair.bettercrafting";

    /// <summary>
    ///     Initializes a new instance of the <see cref="BetterCraftingIntegration" /> class.
    /// </summary>
    /// <param name="modRegistry">SMAPI's mod registry.</param>
    public BetterCraftingIntegration(IModRegistry modRegistry)
        : base(modRegistry, BetterCraftingIntegration.ModUniqueId, "1.2.0")
    {
        // Nothing
    }
}