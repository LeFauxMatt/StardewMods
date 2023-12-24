namespace StardewMods.FuryCore.Framework;

using StardewMods.Common.Services.Integrations.FuryCore;
using StardewMods.FuryCore.Framework.Services;

/// <inheritdoc />
public sealed class FuryCoreApi : IFuryCoreApi
{
    private readonly IConfigWithLogLevel config;
    private readonly IModInfo mod;
    private readonly IThemeHelper themeHelper;

    /// <summary>Initializes a new instance of the <see cref="FuryCoreApi" /> class.</summary>
    /// <param name="mod">Dependency used for accessing mod info.</param>
    /// <param name="config">Dependency used for accessing config data.</param>
    /// <param name="themeHelper">Dependency used for swapping palettes.</param>
    public FuryCoreApi(IModInfo mod, IConfigWithLogLevel config, IThemeHelper themeHelper)
    {
        this.mod = mod;
        this.config = config;
        this.themeHelper = themeHelper;
    }

    /// <inheritdoc />
    public ILog GetLogger(IMonitor monitor) => new Log(this.config, monitor);

    /// <inheritdoc />
    public IThemeHelper GetThemeHelper() => this.themeHelper;
}