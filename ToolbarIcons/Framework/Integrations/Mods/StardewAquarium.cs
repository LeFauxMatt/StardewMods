namespace StardewMods.ToolbarIcons.Framework.Integrations.Mods;

/// <inheritdoc />
internal sealed class StardewAquarium : ICustomIntegration
{
    private const string Argument = "aquariumprogress";
    private const string Method = "OpenAquariumCollectionMenu";
    private const string ModId = "Cherry.StardewAquarium";

    private readonly ComplexIntegration complex;

    /// <summary>Initializes a new instance of the <see cref="StardewAquarium" /> class.</summary>
    /// <param name="complex">Dependency for adding a complex mod integration.</param>
    public StardewAquarium(ComplexIntegration complex) => this.complex = complex;

    /// <inheritdoc />
    public void AddIntegration() => this.complex.AddMethodWithParams(StardewAquarium.ModId, 1, I18n.Button_StardewAquarium(), StardewAquarium.Method, StardewAquarium.Argument, Array.Empty<string>());
}
