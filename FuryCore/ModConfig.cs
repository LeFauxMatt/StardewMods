namespace StardewMods.FuryCore;

using StardewMods.FuryCore.Framework.Enums;
using StardewMods.FuryCore.Framework.Interfaces;

/// <summary>Mod config data for FuryCore.</summary>
internal sealed class ModConfig : IConfigWithLogLevel
{
    /// <inheritdoc />
    public LogLevels LogLevel { get; set; } = LogLevels.Less;
}
