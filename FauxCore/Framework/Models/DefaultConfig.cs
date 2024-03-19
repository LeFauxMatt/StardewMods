namespace StardewMods.FauxCore.Framework.Models;

using StardewMods.Common.Enums;
using StardewMods.FauxCore.Framework.Interfaces;

/// <summary>Mod config data for FauxCore.</summary>
internal sealed class DefaultConfig : IModConfig
{
    /// <inheritdoc />
    public SimpleLogLevel LogLevel { get; set; } = SimpleLogLevel.Less;
}