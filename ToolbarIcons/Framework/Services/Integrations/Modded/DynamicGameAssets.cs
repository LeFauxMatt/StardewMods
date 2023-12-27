namespace StardewMods.ToolbarIcons.Framework.Services.Integrations.Modded;

using StardewMods.ToolbarIcons.Framework.Interfaces;

/// <inheritdoc />
internal sealed class DynamicGameAssets : ICustomIntegration
{
    private const string Argument = "dga_store";
    private const string Method = "OnStoreCommand";
    private const string ModId = "spacechase0.DynamicGameAssets";

    private readonly ComplexIntegration complexIntegration;

    /// <summary>Initializes a new instance of the <see cref="DynamicGameAssets" /> class.</summary>
    /// <param name="complexIntegration">Dependency for adding a complex mod integration.</param>
    public DynamicGameAssets(ComplexIntegration complexIntegration) => this.complexIntegration = complexIntegration;

    /// <inheritdoc />
    public void AddIntegration() =>
        this.complexIntegration.AddMethodWithParams(
            DynamicGameAssets.ModId,
            3,
            I18n.Button_DynamicGameAssets(),
            DynamicGameAssets.Method,
            DynamicGameAssets.Argument,
            Array.Empty<string>());
}