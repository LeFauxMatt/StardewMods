namespace StardewMods.FauxCore.Framework.Interfaces;

using StardewMods.Common.Enums;

/// <summary>Mod config data with log level.</summary>
public interface IModConfig
{
    /// <summary>Gets the amount of debugging information that will be logged to the console.</summary>
    public SimpleLogLevel LogLevel { get; }
}