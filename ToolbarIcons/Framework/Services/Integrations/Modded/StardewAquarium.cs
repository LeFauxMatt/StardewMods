namespace StardewMods.ToolbarIcons.Framework.Services.Integrations.Modded;

using StardewMods.ToolbarIcons.Framework.Interfaces;

/// <inheritdoc />
internal sealed class StardewAquarium : ICustomIntegration
{
    private const string Argument = "aquariumprogress";
    private const string Method = "OpenAquariumCollectionMenu";
    private const string ModId = "Cherry.StardewAquarium";

    private readonly ComplexIntegration complexIntegration;

    /// <summary>Initializes a new instance of the <see cref="StardewAquarium" /> class.</summary>
    /// <param name="complexIntegration">Dependency for adding a complex mod integration.</param>
    public StardewAquarium(ComplexIntegration complexIntegration) => this.complexIntegration = complexIntegration;

    /// <inheritdoc />
    public void AddIntegration() =>
        this.complexIntegration.AddMethodWithParams(
            StardewAquarium.ModId,
            1,
            I18n.Button_StardewAquarium(),
            StardewAquarium.Method,
            StardewAquarium.Argument,
            Array.Empty<string>());
}