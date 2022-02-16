namespace StardewMods.FuryCore.Models.CustomEvents;

using System;
using System.Collections.Generic;
using StardewMods.FuryCore.Interfaces.GameObjects;

/// <inheritdoc />
public class GameObjectsRemovedEventArgs : EventArgs
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="GameObjectsRemovedEventArgs" /> class.
    /// </summary>
    /// <param name="removed">The <see cref="IGameObject" /> removed.</param>
    internal GameObjectsRemovedEventArgs(IEnumerable<IGameObject> removed)
    {
        this.Removed = removed;
    }

    /// <summary>
    ///     Gets <see cref="IGameObject" /> removed.
    /// </summary>
    public IEnumerable<IGameObject> Removed { get; }
}