namespace StardewMods.EasyAccess.Framework.Interfaces;

using StardewMods.EasyAccess.Framework.Models;

/// <summary>Mod config data for Easy Access.</summary>
internal interface IModConfig
{
    /// <summary>Gets a value indicating the distance in tiles that the producer can be collected from.</summary>
    public int CollectOutputDistance { get; }

    /// <summary>Gets the control scheme.</summary>
    public Controls ControlScheme { get; }

    /// <summary>Gets a value indicating the distance in tiles that the producer can be dispensed into.</summary>
    public int DispenseInputDistance { get; }

    /// <summary>Gets a value indicating whether CollectItems will grab from dig spots.</summary>
    public bool DoDigSpots { get; }

    /// <summary>Gets a value indicating whether CollectItems will drop forage as debris.</summary>
    public bool DoForage { get; }

    /// <summary>Gets a value indicating whether CollectItems will collect from machines.</summary>
    public bool DoMachines { get; }

    /// <summary>Gets a value indicating whether CollectItems will interact with Terrain features such as bushes and trees.</summary>
    public bool DoTerrain { get; }
}