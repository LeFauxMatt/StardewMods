namespace StardewMods.Common.Models.Events;

/// <summary>Represents the event arguments for a configuration changes.</summary>
/// <typeparam name="TConfig">The config type.</typeparam>
internal sealed class ConfigChangedEventArgs<TConfig>(TConfig config) : EventArgs
{
    /// <summary>Gets the current config options.</summary>
    public TConfig Config { get; } = config;
}