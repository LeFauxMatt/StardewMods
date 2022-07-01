#nullable disable

namespace StardewMods.FuryCore.Models.CustomEvents;

using System;
using System.Collections.Generic;
using StardewMods.FuryCore.Interfaces.CustomEvents;
using StardewMods.FuryCore.Interfaces.GameObjects;

/// <inheritdoc cref="StardewMods.FuryCore.Interfaces.CustomEvents.IGameObjectsRemovedEventArgs" />
internal class GameObjectsRemovedEventArgs : EventArgs, IGameObjectsRemovedEventArgs
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="GameObjectsRemovedEventArgs" /> class.
    /// </summary>
    /// <param name="removed">The <see cref="IGameObject" /> removed.</param>
    internal GameObjectsRemovedEventArgs(IEnumerable<IGameObject> removed)
    {
        this.Removed = removed;
    }

    /// <inheritdoc />
    public IEnumerable<IGameObject> Removed { get; }
}