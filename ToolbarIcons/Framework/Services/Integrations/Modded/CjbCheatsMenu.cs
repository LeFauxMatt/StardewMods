namespace StardewMods.ToolbarIcons.Framework.Services.Integrations.Modded;

using StardewMods.ToolbarIcons.Framework.Interfaces;

/// <inheritdoc />
internal sealed class CjbCheatsMenu : IMethodIntegration
{
    /// <inheritdoc/>
    public string ModId => "CJBok.CheatsMenu";

    /// <inheritdoc/>
    public int Index => 2;

    /// <inheritdoc/>
    public string HoverText => I18n.Button_CheatsMenu();

    /// <inheritdoc/>
    public string MethodName => "OpenCheatsMenu";

    /// <inheritdoc/>
    public object?[] Arguments => [0, true];
}