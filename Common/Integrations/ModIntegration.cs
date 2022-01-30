namespace Common.Integrations;

using System;
using StardewModdingAPI;

/// <summary>Provides an integration point for using external mods' APIs.</summary>
/// <typeparam name="T">Interface for the external mod's API.</typeparam>
internal abstract class ModIntegration<T>
    where T : class
{
    private readonly Lazy<T> _modAPI;

    /// <summary>Initializes a new instance of the <see cref="ModIntegration{T}" /> class.</summary>
    /// <param name="modRegistry">SMAPI's mod registry.</param>
    /// <param name="modUniqueId">The unique id of the external mod.</param>
    internal ModIntegration(IModRegistry modRegistry, string modUniqueId)
    {
        this.ModRegistry = modRegistry;
        this.UniqueId = modUniqueId;
        this._modAPI = new(() => this.ModRegistry.GetApi<T>(this.UniqueId));
    }

    /// <summary>
    /// Gets the Unique Id for this mod.
    /// </summary>
    protected internal string UniqueId { get; }

    /// <summary>Gets the Mod's API through SMAPI's standard interface.</summary>
    protected internal T API
    {
        get => this.IsLoaded ? this._modAPI.Value : default;
    }

    /// <summary>Gets a value indicating whether the mod is loaded.</summary>
    protected internal bool IsLoaded
    {
        get => this.ModRegistry.IsLoaded(this.UniqueId);
    }

    private IModRegistry ModRegistry { get; }
}