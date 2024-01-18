namespace StardewMods.Common.Services.Integrations.FuryCore;

/// <inheritdoc />
internal sealed class FuryThemer(FuryCoreIntegration furyCore) : IThemeHelper
{
    private readonly IThemeHelper themeHelper = furyCore.Api!.CreateThemeService();

    /// <inheritdoc />
    public void AddAssets(string[] assetNames) => this.themeHelper.AddAssets(assetNames);
}