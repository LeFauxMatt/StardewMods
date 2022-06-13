#nullable disable

namespace StardewMods.EasyAccess.Models.Config;

using StardewMods.EasyAccess.Interfaces.Config;

/// <inheritdoc />
internal class ConfigData : IConfigData
{
    /// <inheritdoc />
    public int CollectOutputDistance { get; set; } = 15;

    /// <inheritdoc />
    public ControlScheme ControlScheme { get; set; } = new();

    /// <inheritdoc />
    public int DispenseInputDistance { get; set; } = 15;

    /// <inheritdoc />
    public bool DoDigSpots { get; set; } = true;

    /// <inheritdoc />
    public bool DoForage { get; set; } = true;

    /// <inheritdoc />
    public bool DoMachines { get; set; } = true;

    /// <inheritdoc />
    public bool DoTerrain { get; set; } = true;
}