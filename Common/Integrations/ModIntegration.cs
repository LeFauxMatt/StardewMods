namespace Common.Integrations;

using StardewModdingAPI;

/// <summary>Provides an integration point for using external mods' APIs.</summary>
/// <typeparam name="T">Interface for the external mod's API.</typeparam>
internal abstract class ModIntegration<T>
    where T : class
{
    private bool _isLoaded;
    private T _modAPI = null!;

    /// <summary>Initializes a new instance of the <see cref="ModIntegration{T}" /> class.</summary>
    /// <param name="modRegistry">SMAPI's mod registry.</param>
    /// <param name="modUniqueId">The unique id of the external mod.</param>
    internal ModIntegration(IModRegistry modRegistry, string modUniqueId)
    {
        this.ModRegistry = modRegistry;
        this.UniqueId = modUniqueId;
    }

    /// <summary>
    /// Gets the Unique Id for this mod.
    /// </summary>
    protected internal string UniqueId { get; }

    /// <summary>Gets the Mod's API through SMAPI's standard interface.</summary>
    protected internal T API
    {
        get
        {
            if (!this.IsInitialized)
            {
                this._modAPI = this.ModRegistry.GetApi<T>(this.UniqueId);
                this.IsInitialized = true;
            }

            return this._modAPI;
        }
    }

    /// <summary>Gets the loaded status of the mod.</summary>
    protected internal bool IsLoaded
    {
        get => this._isLoaded = this._isLoaded || this.ModRegistry.IsLoaded(this.UniqueId);
    }

    private IModRegistry ModRegistry { get; }

    private bool IsInitialized { get; set; }
}