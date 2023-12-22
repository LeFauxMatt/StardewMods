namespace StardewMods.ExpandedStorage;

using StardewMods.Common.Enums;
using StardewMods.Common.Interfaces;

/// <summary>Mod config data for Expanded Storage.</summary>
internal sealed class ModConfig : IConfigWithLogLevel
{
    /// <inheritdoc />
    public SimpleLogLevel LogLevel { get; set; } = SimpleLogLevel.Less;
}
