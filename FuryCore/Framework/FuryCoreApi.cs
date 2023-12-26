namespace StardewMods.FuryCore.Framework;

using StardewMods.Common.Services.Integrations.FuryCore;
using StardewMods.FuryCore.Framework.Services;

/// <inheritdoc />
public sealed class FuryCoreApi : IFuryCoreApi
{
    private readonly IConfigWithLogLevel config;
    private readonly IModInfo mod;
    private readonly ITheming theming;

    /// <summary>Initializes a new instance of the <see cref="FuryCoreApi" /> class.</summary>
    /// <param name="mod">Dependency used for accessing mod info.</param>
    /// <param name="config">Dependency used for accessing config data.</param>
    /// <param name="theming">Dependency used for swapping palettes.</param>
    public FuryCoreApi(IModInfo mod, IConfigWithLogLevel config, ITheming theming)
    {
        this.mod = mod;
        this.config = config;
        this.theming = theming;
    }

    /// <inheritdoc />
    public ILog CreateLogService(IMonitor monitor) => new Log(this.config, monitor);

    /// <inheritdoc />
    public ITheming CreateThemingService() => this.theming;
}