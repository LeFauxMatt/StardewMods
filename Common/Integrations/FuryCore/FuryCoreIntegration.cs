#nullable disable

namespace Common.Integrations.FuryCore;

using StardewModdingAPI;

/// <inheritdoc />
internal class FuryCoreIntegration : ModIntegration<IFuryCoreApi>
{
    private const string ModUniqueId = "furyx639.FuryCore";

    /// <summary>
    ///     Initializes a new instance of the <see cref="FuryCoreIntegration" /> class.
    /// </summary>
    /// <param name="modRegistry">SMAPI's mod registry.</param>
    public FuryCoreIntegration(IModRegistry modRegistry)
        : base(modRegistry, FuryCoreIntegration.ModUniqueId)
    {
    }
}