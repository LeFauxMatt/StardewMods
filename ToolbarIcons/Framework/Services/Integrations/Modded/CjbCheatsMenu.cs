namespace StardewMods.ToolbarIcons.Framework.Services.Integrations.Modded;

using StardewMods.ToolbarIcons.Framework.Interfaces;

/// <inheritdoc />
internal sealed class CjbCheatsMenu : ICustomIntegration
{
    private const string Method = "OpenCheatsMenu";
    private const string ModId = "CJBok.CheatsMenu";

    private readonly ComplexIntegration complexIntegration;

    /// <summary>Initializes a new instance of the <see cref="CjbCheatsMenu" /> class.</summary>
    /// <param name="complexIntegration">Dependency for adding a complex mod integration.</param>
    public CjbCheatsMenu(ComplexIntegration complexIntegration) => this.complexIntegration = complexIntegration;

    /// <inheritdoc />
    public void AddIntegration() =>
        this.complexIntegration.AddMethodWithParams(
            CjbCheatsMenu.ModId,
            2,
            I18n.Button_CheatsMenu(),
            CjbCheatsMenu.Method,
            0,
            true);
}