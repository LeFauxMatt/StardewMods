namespace StardewMods.ToolbarIcons.Framework.Services.Integrations.Modded;

using StardewMods.ToolbarIcons.Framework.Interfaces;

/// <inheritdoc />
internal sealed class GenericModConfigMenu : IMethodIntegration
{
    /// <inheritdoc />
    public int Index => 4;

    /// <inheritdoc />
    public string HoverText => I18n.Button_GenericModConfigMenu();

    /// <inheritdoc />
    public string ModId => "spacechase0.GenericModConfigMenu";

    /// <inheritdoc />
    public string MethodName => "OpenListMenu";

    /// <inheritdoc />
    public object?[] Arguments => [0];
}