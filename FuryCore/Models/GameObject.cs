﻿namespace StardewMods.FuryCore.Models;

using StardewMods.FuryCore.Interfaces;
using StardewValley;

/// <inheritdoc />
internal abstract class GameObject : IGameObject
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="GameObject" /> class.
    /// </summary>
    /// <param name="context">The source object.</param>
    protected GameObject(object context)
    {
        this.Context = context;
    }

    /// <inheritdoc />
    public object Context { get; }

    /// <inheritdoc />
    public abstract ModDataDictionary ModData { get; }
}