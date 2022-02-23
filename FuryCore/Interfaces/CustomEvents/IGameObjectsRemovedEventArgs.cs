namespace StardewMods.FuryCore.Interfaces.CustomEvents;

using System;
using System.Collections.Generic;
using StardewMods.FuryCore.Events;
using StardewMods.FuryCore.Interfaces.GameObjects;

/// <summary>
///     <see cref="EventArgs" /> for the <see cref="GameObjectsRemoved" /> event.
/// </summary>
public interface IGameObjectsRemovedEventArgs
{
    /// <summary>
    ///     Gets <see cref="IGameObject" /> removed.
    /// </summary>
    public IEnumerable<IGameObject> Removed { get; }
}