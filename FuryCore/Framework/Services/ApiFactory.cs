namespace StardewMods.FuryCore.Framework.Services;

using StardewMods.Common.Interfaces;
using StardewMods.Common.Services.Integrations.FuryCore;

/// <summary>This is a factory class that is used to create instances of the FuryCoreApi.</summary>
internal sealed class ApiFactory
{
    private readonly IConfigWithLogLevel config;
    private readonly IThemeHelper themeHelper;

    /// <summary>Initializes a new instance of the <see cref="ApiFactory"/> class.</summary>
    /// <param name="config">Dependency used for accessing config data.</param>
    /// <param name="themeHelper">Dependency used for swapping palettes.</param>
    public ApiFactory(IConfigWithLogLevel config, IThemeHelper themeHelper)
    {
        this.config = config;
        this.themeHelper = themeHelper;
    }

    /// <summary>Creates an instance of the FuryCoreApi by providing the mod information.</summary>
    /// <param name="mod">The information related to the mod.</param>
    /// <returns>An instance of the FuryCoreApi.</returns>
    public IFuryCoreApi CreateApi(IModInfo mod) => new FuryCoreApi(mod, this.config, this.themeHelper);
}
