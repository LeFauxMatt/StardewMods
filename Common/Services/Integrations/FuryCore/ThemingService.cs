namespace StardewMods.Common.Services.Integrations.FuryCore;

/// <inheritdoc />
internal sealed class ThemingService(FuryCoreIntegration furyCoreIntegration) : ITheming
{
    private readonly ITheming theming = furyCoreIntegration.Api!.CreateThemingService();

    /// <inheritdoc/>
    public void AddAssets(string[] assetNames) => this.theming.AddAssets(assetNames);
}