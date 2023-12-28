namespace StardewMods.ToolbarIcons.Framework.Services.Integrations.Modded;

using StardewMods.ToolbarIcons.Framework.Interfaces;

/// <inheritdoc />
internal sealed class StardewAquarium : IMethodIntegration
{
    /// <inheritdoc />
    public int Index => 1;

    /// <inheritdoc />
    public string HoverText => I18n.Button_StardewAquarium();

    /// <inheritdoc />
    public string ModId => "Cherry.StardewAquarium";

    /// <inheritdoc />
    public string MethodName => "OpenAquariumCollectionMenu";

    /// <inheritdoc />
    public object?[] Arguments => ["aquariumprogress", Array.Empty<string>()];
}