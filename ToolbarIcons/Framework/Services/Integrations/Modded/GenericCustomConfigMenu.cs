namespace StardewMods.ToolbarIcons.Framework.Services.Integrations.Modded;

using StardewMods.ToolbarIcons.Framework.Interfaces;

/// <inheritdoc />
internal sealed class GenericCustomConfigMenu : ICustomIntegration
{
    private const string Method = "OpenListMenu";
    private const string ModId = "spacechase0.GenericModConfigMenu";

    private readonly ComplexIntegration complexIntegration;

    /// <summary>Initializes a new instance of the <see cref="GenericCustomConfigMenu" /> class.</summary>
    /// <param name="complexIntegration">Dependency for adding a complex mod integration.</param>
    public GenericCustomConfigMenu(ComplexIntegration complexIntegration) =>
        this.complexIntegration = complexIntegration;

    /// <inheritdoc />
    public void AddIntegration() =>
        this.complexIntegration.AddMethodWithParams(
            GenericCustomConfigMenu.ModId,
            4,
            I18n.Button_GenericModConfigMenu(),
            GenericCustomConfigMenu.Method,
            0);
}