#nullable disable

namespace StardewMods.FuryCore.Interfaces.CustomEvents;

using System;
using StardewMods.FuryCore.Events;
using StardewMods.FuryCore.Interfaces.GameObjects;

/// <summary>
///     <see cref="EventArgs" /> for the <see cref="SavingConfig" /> event.
/// </summary>
public interface ISavingConfigEventArgs
{
    /// <summary>
    ///     Gets the GameObject being saved.
    /// </summary>
    public IGameObject GameObject { get; }
}