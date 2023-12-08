namespace StardewMods.ToolbarIcons.Framework.Integrations.Mods;

/// <inheritdoc />
internal sealed class CjbCheatsMenu : ICustomIntegration
{
    private const string Method = "OpenCheatsMenu";
    private const string ModId = "CJBok.CheatsMenu";

    private readonly ComplexIntegration complex;

    /// <summary>Initializes a new instance of the <see cref="CjbCheatsMenu" /> class.</summary>
    /// <param name="complex">Dependency for adding a complex mod integration.</param>
    public CjbCheatsMenu(ComplexIntegration complex) => this.complex = complex;

    /// <inheritdoc />
    public void AddIntegration() =>
        this.complex.AddMethodWithParams(
            CjbCheatsMenu.ModId,
            2,
            I18n.Button_CheatsMenu(),
            CjbCheatsMenu.Method,
            0,
            true);
}
