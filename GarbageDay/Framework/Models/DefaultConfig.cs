namespace StardewMods.GarbageDay.Framework.Models;

using StardewMods.GarbageDay.Framework.Interfaces;

/// <inheritdoc />
internal sealed class DefaultConfig : IModConfig
{
    /// <inheritdoc />
    public DayOfWeek GarbageDay { get; set; } = DayOfWeek.Monday;

    /// <inheritdoc />
    public bool OnByDefault { get; set; } = true;
}