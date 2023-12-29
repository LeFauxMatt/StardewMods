namespace StardewMods.FuryCore.Framework.Models;

using StardewMods.Common.Enums;
using StardewMods.FuryCore.Framework.Interfaces;

/// <summary>Mod config data for FuryCore.</summary>
internal sealed class DefaultConfig : IModConfig
{
    /// <inheritdoc />
    public SimpleLogLevel LogLevel { get; set; } = SimpleLogLevel.Less;
}