namespace StardewMods.FauxCore.Framework;

using StardewMods.Common.Services.Integrations.FauxCore;
using StardewMods.FauxCore.Framework.Interfaces;
using StardewMods.FauxCore.Framework.Services;

/// <inheritdoc />
public sealed class FauxCoreApi : IFauxCoreApi
{
    private readonly Func<IModConfig> getConfig;
    private readonly IModInfo modInfo;
    private readonly IThemeHelper themeHelper;

    /// <summary>Initializes a new instance of the <see cref="FauxCoreApi" /> class.</summary>
    /// <param name="modInfo">Dependency used for accessing mod info.</param>
    /// <param name="getConfig">Dependency used for accessing config data.</param>
    /// <param name="themeHelper">Dependency used for swapping palettes.</param>
    public FauxCoreApi(IModInfo modInfo, Func<IModConfig> getConfig, IThemeHelper themeHelper)
    {
        this.modInfo = modInfo;
        this.getConfig = getConfig;
        this.themeHelper = themeHelper;
    }

    /// <inheritdoc />
    public ILog CreateLogService(IMonitor monitor) => new Log(this.getConfig, monitor);

    /// <inheritdoc />
    public IPatchManager CreatePatchService(ILog log) => new PatchManager(log, this.modInfo.Manifest);

    /// <inheritdoc />
    public IThemeHelper CreateThemeService() => this.themeHelper;
}