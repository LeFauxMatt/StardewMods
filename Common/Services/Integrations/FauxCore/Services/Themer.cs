namespace StardewMods.Common.Services.Integrations.FauxCore;

/// <inheritdoc />
internal sealed class Themer(FauxCoreIntegration fauxCore) : IThemeHelper
{
    private readonly IThemeHelper themeHelper = fauxCore.Api!.CreateThemeService();

    /// <inheritdoc />
    public void AddAssets(string[] assetNames) => this.themeHelper.AddAssets(assetNames);
}