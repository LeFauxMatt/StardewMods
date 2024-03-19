namespace StardewMods.GarbageDay.Framework.Interfaces;

/// <summary>Mod config data for Garbage Day.</summary>
internal interface IModConfig
{
    /// <summary>Gets the day of the week that garbage is collected.</summary>
    public DayOfWeek GarbageDay { get; }

    /// <summary>Get a value indicating whether all garbage cans will be handled by default.</summary>
    public bool OnByDefault { get; }
}