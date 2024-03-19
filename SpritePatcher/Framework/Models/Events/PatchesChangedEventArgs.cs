namespace StardewMods.SpritePatcher.Framework.Models.Events;

/// <summary>Represents the event arguments for the PatchesChanged event.</summary>
internal sealed class PatchesChangedEventArgs(IList<string> targets) : EventArgs
{
    /// <summary>Gets the patch targets which were changed.</summary>
    public IList<string> ChangedTargets { get; } = targets;
}