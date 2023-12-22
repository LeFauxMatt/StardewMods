namespace StardewMods.ToolbarIcons.Framework.Integrations.Mods;

/// <inheritdoc />
internal sealed class DynamicGameAssets : ICustomIntegration
{
    private const string Argument = "dga_store";
    private const string Method = "OnStoreCommand";
    private const string ModId = "spacechase0.DynamicGameAssets";

    private readonly ComplexIntegration complex;

    /// <summary>Initializes a new instance of the <see cref="DynamicGameAssets" /> class.</summary>
    /// <param name="complex">Dependency for adding a complex mod integration.</param>
    public DynamicGameAssets(ComplexIntegration complex) => this.complex = complex;

    /// <inheritdoc />
    public void AddIntegration() => this.complex.AddMethodWithParams(DynamicGameAssets.ModId, 3, I18n.Button_DynamicGameAssets(), DynamicGameAssets.Method, DynamicGameAssets.Argument, Array.Empty<string>());
}
