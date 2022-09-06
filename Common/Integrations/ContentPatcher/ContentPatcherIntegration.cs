namespace StardewMods.Common.Integrations.ContentPatcher;

/// <inheritdoc />
internal class ContentPatcherIntegration : ModIntegration<IContentPatcherApi>
{
    private const string ModUniqueId = "Pathoschild.ContentPatcher";
    private const string ModVersion = "1.28.0";

    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentPatcherIntegration" /> class.
    /// </summary>
    /// <param name="modRegistry">SMAPI's mod registry.</param>
    public ContentPatcherIntegration(IModRegistry modRegistry)
        : base(modRegistry, ContentPatcherIntegration.ModUniqueId, ContentPatcherIntegration.ModVersion)
    {
        // Nothing
    }
}