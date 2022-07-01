#nullable disable

namespace StardewMods.FuryCore.Interfaces.CustomEvents;

using System;
using StardewModdingAPI;
using StardewMods.FuryCore.Events;
using StardewMods.FuryCore.Interfaces.GameObjects;

/// <summary>
///     <see cref="EventArgs" /> for the <see cref="ConfiguringGameObject" /> event.
/// </summary>
public interface IConfiguringGameObjectEventArgs
{
    /// <summary>
    ///     Gets the GameObject being configured.
    /// </summary>
    public IGameObject GameObject { get; }

    /// <summary>
    ///     Gets the mod manifest to add config options to.
    /// </summary>
    public IManifest ModManifest { get; }
}