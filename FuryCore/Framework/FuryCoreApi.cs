namespace StardewMods.FuryCore.Framework;

using StardewMods.FuryCore.Framework.Interfaces;
using StardewMods.FuryCore.Framework.Services;
using StardewMods.FuryCore.Framework.Services.Integrations.FuryCore;

/// <inheritdoc />
public sealed class FuryCoreApi : IFuryCoreApi
{
    private readonly IModInfo mod;
    private readonly IConfigWithLogLevel config;
    private readonly IThemeHelper themeHelper;

    /// <summary>Initializes a new instance of the <see cref="FuryCoreApi"/> class.</summary>
    /// <param name="mod">Dependency used for accessing mod info.</param>
    /// <param name="config">Dependency used for accessing config data.</param>
    /// <param name="themeHelper">Dependency used for swapping palettes.</param>
    public FuryCoreApi(IModInfo mod, IConfigWithLogLevel config, IThemeHelper themeHelper)
    {
        this.mod = mod;
        this.config = config;
        this.themeHelper = themeHelper;
    }

    /// <inheritdoc/>
    public ILogging GetLogger(IMonitor monitor) => new Logging(this.config, monitor);

    /// <inheritdoc/>
    public IThemeHelper GetThemeHelper() => this.themeHelper;
}
