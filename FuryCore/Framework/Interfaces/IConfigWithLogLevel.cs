namespace StardewMods.FuryCore.Framework.Interfaces;

using StardewMods.FuryCore.Framework.Enums;

/// <summary>Mod config data with log level.</summary>
public interface IConfigWithLogLevel
{
    /// <summary>Gets or sets the amount of debugging information that will be logged to the console.</summary>
    public LogLevels LogLevel { get; set; }
}
