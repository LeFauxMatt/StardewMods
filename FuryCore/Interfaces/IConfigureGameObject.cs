namespace StardewMods.FuryCore.Interfaces;

using System;
using StardewMods.FuryCore.Interfaces.CustomEvents;
using StardewMods.FuryCore.Interfaces.GameObjects;

/// <summary>
///     Opens a ModConfigMenu for a particular game object.
/// </summary>
public interface IConfigureGameObject
{
    /// <summary>
    ///     Raised when an attempt is made to configure an object.
    /// </summary>
    public event EventHandler<IConfiguringGameObjectEventArgs> ConfiguringGameObject;

    /// <summary>
    ///     Raised when the config menu is reset.
    /// </summary>
    public event EventHandler<IResettingConfigEventArgs> ResettingConfig;

    /// <summary>
    ///     Raised when the config menu is saved.
    /// </summary>
    public event EventHandler<ISavingConfigEventArgs> SavingConfig;

    /// <summary>
    ///     Gets the current object being configured.
    /// </summary>
    public IGameObject CurrentObject { get; }
}