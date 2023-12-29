namespace StardewMods.Common.Services.Integrations.FuryCore;

using StardewMods.Common.Enums;

/// <summary>Mod config data with log level.</summary>
public interface IConfigWithLogLevel
{
    /// <summary>Gets the amount of debugging information that will be logged to the console.</summary>
    public SimpleLogLevel LogLevel { get; }
}