namespace StardewMods.ToolbarIcons.Framework.Integrations.Mods;

/// <inheritdoc />
internal sealed class GenericCustomConfigMenu : ICustomIntegration
{
    private const string Method = "OpenListMenu";
    private const string ModId = "spacechase0.GenericModConfigMenu";

    private readonly ComplexIntegration complex;

    /// <summary>Initializes a new instance of the <see cref="GenericCustomConfigMenu" /> class.</summary>
    /// <param name="complex">Dependency for adding a complex mod integration.</param>
    public GenericCustomConfigMenu(ComplexIntegration complex) => this.complex = complex;

    /// <inheritdoc />
    public void AddIntegration() => this.complex.AddMethodWithParams(GenericCustomConfigMenu.ModId, 4, I18n.Button_GenericModConfigMenu(), GenericCustomConfigMenu.Method, 0);
}
