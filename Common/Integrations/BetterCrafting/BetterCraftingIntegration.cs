#nullable disable

namespace Common.Integrations.BetterCrafting;

using StardewModdingAPI;

/// <inheritdoc />
internal class BetterCraftingIntegration : ModIntegration<IBetterCraftingApi>
{
    private const string ModUniqueId = "leclair.bettercrafting";

    /// <summary>
    ///     Initializes a new instance of the <see cref="BetterCraftingIntegration" /> class.
    /// </summary>
    /// <param name="modRegistry">SMAPI's mod registry.</param>
    public BetterCraftingIntegration(IModRegistry modRegistry)
        : base(modRegistry, BetterCraftingIntegration.ModUniqueId)
    {
    }
}