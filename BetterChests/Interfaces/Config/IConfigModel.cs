﻿namespace StardewMods.BetterChests.Interfaces.Config;

/// <inheritdoc />
internal interface IConfigModel : IConfigData
{
    /// <summary>
    ///     Resets all config options back to their default value.
    /// </summary>
    public void Reset();

    /// <summary>
    ///     Saves current config options to the config.json file.
    /// </summary>
    public void Save();
}