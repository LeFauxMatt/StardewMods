#nullable disable

namespace Common.Integrations.GenericModConfigMenu;

using System;
using System.Collections.Generic;
using StardewModdingAPI;

/// <inheritdoc />
internal class GenericModConfigMenuIntegration : ModIntegration<IGenericModConfigMenuApi>
{
    private const string ModUniqueId = "spacechase0.GenericModConfigMenu";

    /// <summary>
    ///     Initializes a new instance of the <see cref="GenericModConfigMenuIntegration" /> class.
    /// </summary>
    /// <param name="modRegistry">SMAPI's mod registry.</param>
    public GenericModConfigMenuIntegration(IModRegistry modRegistry)
        : base(modRegistry, GenericModConfigMenuIntegration.ModUniqueId)
    {
    }

    private HashSet<string> Registered { get; } = new();

    /// <summary>
    ///     Checks if the mod is already registered with GMCM.
    /// </summary>
    /// <param name="mod">The mod to check.</param>
    /// <returns>True if the mod is registered.</returns>
    public bool IsRegistered(IManifest mod)
    {
        return this.Registered.Contains(mod.UniqueID);
    }

    /// <summary>
    ///     <inheritdoc cref="IGenericModConfigMenuApi.Register" />
    /// </summary>
    /// <param name="mod">The mod's manifest.</param>
    /// <param name="reset">Reset the mod's config to its default values.</param>
    /// <param name="save">Save the mod's current config to the <c>config.json</c> file.</param>
    /// <param name="titleScreenOnly">Whether the options can only be edited from the title screen.</param>
    public void Register(IManifest mod, Action reset, Action save, bool titleScreenOnly = false)
    {
        this.Unregister(mod);
        this.API.Register(mod, reset, save, titleScreenOnly);
        this.Registered.Add(mod.UniqueID);
    }

    /// <summary>
    ///     <inheritdoc cref="IGenericModConfigMenuApi.Unregister" />
    /// </summary>
    /// <param name="mod">The mod's manifest.</param>
    public void Unregister(IManifest mod)
    {
        if (this.Registered.Contains(mod.UniqueID))
        {
            this.API.Unregister(mod);
            this.Registered.Remove(mod.UniqueID);
        }
    }
}