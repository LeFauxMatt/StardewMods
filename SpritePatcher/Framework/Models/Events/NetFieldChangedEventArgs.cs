namespace StardewMods.SpritePatcher.Framework.Models.Events;

/// <summary>Event arguments for when a net field changes.</summary>
internal sealed class NetFieldChangedEventArgs(string target) : EventArgs
{
    /// <summary>Gets the target</summary>
    public string Target { get; } = target;
}