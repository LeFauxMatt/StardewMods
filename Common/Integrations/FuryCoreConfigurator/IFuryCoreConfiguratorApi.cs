namespace Common.Integrations.FuryCoreConfigurator;

using System;
using StardewModdingAPI;

/// <summary>
///     API for Fury Core Configurator.
/// </summary>
public interface IFuryCoreConfiguratorApi
{
    /// <summary>
    ///     Event triggered when configure is used on an object.
    /// </summary>
    public event EventHandler<object> ConfigureObject;

    /// <summary>
    ///     Event triggered when config menu is reset.
    /// </summary>
    public event EventHandler<object> ResetConfig;

    /// <summary>
    ///     Event triggered when config menu is saved.
    /// </summary>
    public event EventHandler<object> SaveConfig;

    /// <summary>
    ///     Gets the manifest for FuryCoreConfigurator to add config options for the current object.
    /// </summary>
    public IManifest ModManifest { get; }
}