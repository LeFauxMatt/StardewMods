namespace StardewMods.EasyAccess.Framework.Models;

using StardewMods.EasyAccess.Framework.Interfaces;

/// <inheritdoc />
internal sealed class DefaultConfig : IModConfig
{
    /// <inheritdoc />
    public int CollectOutputDistance { get; set; } = 15;

    /// <inheritdoc />
    public Controls ControlScheme { get; set; } = new();

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