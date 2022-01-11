namespace Common.Integrations;

using StardewModdingAPI;

/// <summary>Provides an integration point for using external mods' APIs.</summary>
/// <typeparam name="T">Interface for the external mod's API.</typeparam>
internal abstract class ModIntegration<T>
    where T : class
{
    private readonly IModRegistry _modRegistry;
    private readonly string _modUniqueId;
    private bool _isInitialized;
    private bool _isLoaded;
    private T _modAPI = null!;

    /// <summary>Initializes a new instance of the <see cref="ModIntegration{T}" /> class.</summary>
    /// <param name="modRegistry">SMAPI's mod registry.</param>
    /// <param name="modUniqueId">The unique id of the external mod.</param>
    internal ModIntegration(IModRegistry modRegistry, string modUniqueId)
    {
        this._modRegistry = modRegistry;
        this._modUniqueId = modUniqueId;
    }

    /// <summary>Gets the Mod's API through SMAPI's standard interface.</summary>
    protected internal T API
    {
        get
        {
            if (!this._isInitialized)
            {
                this._modAPI = this._modRegistry.GetApi<T>(this._modUniqueId);
                this._isInitialized = true;
            }

            return this._modAPI;
        }
    }

    /// <summary>Gets the loaded status of the mod.</summary>
    protected internal bool IsLoaded
    {
        get => this._isLoaded = this._isLoaded || this._modRegistry.IsLoaded(this._modUniqueId);
    }
}