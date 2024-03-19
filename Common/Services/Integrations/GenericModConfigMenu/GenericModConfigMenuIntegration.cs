namespace StardewMods.Common.Services.Integrations.GenericModConfigMenu;

/// <inheritdoc />
internal sealed class GenericModConfigMenuIntegration : ModIntegration<IGenericModConfigMenuApi>
{
    private const string ModUniqueId = "spacechase0.GenericModConfigMenu";

    private readonly IManifest manifest;

    /// <summary>Initializes a new instance of the <see cref="GenericModConfigMenuIntegration" /> class.</summary>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="modRegistry">Dependency used for fetching metadata about loaded mods.</param>
    public GenericModConfigMenuIntegration(IManifest manifest, IModRegistry modRegistry)
        : base(modRegistry, GenericModConfigMenuIntegration.ModUniqueId) =>
        this.manifest = manifest;

    /// <summary>Gets a value indicating whether the mod is already registered with GMCM.</summary>
    public bool IsRegistered { get; private set; }

    /// <summary>Register a config menu with GMCM.</summary>
    /// <param name="reset">Reset the mod's config to its default values.</param>
    /// <param name="save">Save the mod's current config to the <c>config.json</c> file.</param>
    /// <param name="titleScreenOnly">Whether the options can only be edited from the title screen.</param>
    public void Register(Action reset, Action save, bool titleScreenOnly = false)
    {
        this.Unregister();
        this.Api?.Register(this.manifest, reset, save, titleScreenOnly);
        this.IsRegistered = true;
    }

    /// <summary>Unregister a config menu with GMCM.</summary>
    public void Unregister()
    {
        if (!this.IsRegistered)
        {
            return;
        }

        this.Api?.Unregister(this.manifest);
        this.IsRegistered = false;
    }

    /// <summary>Add an option at the current position in the form using custom rendering logic.</summary>
    /// <param name="complexOption">The option to add.</param>
    public void AddComplexOption(IComplexOption complexOption) =>
        this.Api?.AddComplexOption(
            this.manifest,
            () => complexOption.Name,
            complexOption.Draw,
            () => complexOption.Tooltip,
            complexOption.BeforeMenuOpened,
            complexOption.BeforeSave,
            complexOption.AfterSave,
            complexOption.BeforeReset,
            complexOption.AfterReset,
            complexOption.BeforeMenuClosed,
            () => complexOption.Height);
}