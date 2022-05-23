#nullable disable

namespace StardewMods.FuryCore.Interfaces.CustomEvents;

using System;
using StardewMods.FuryCore.Events;
using StardewMods.FuryCore.Interfaces.GameObjects;

/// <summary>
///     <see cref="EventArgs" /> for the <see cref="ResettingConfig" /> event.
/// </summary>
public interface IResettingConfigEventArgs
{
    /// <summary>
    ///     Gets the GameObject being reset.
    /// </summary>
    public IGameObject GameObject { get; }
}