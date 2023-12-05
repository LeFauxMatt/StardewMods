namespace StardewMods.Common.Integrations.OrdinaryCapsule;

/// <summary>API for Ordinary Capsule.</summary>
public interface IOrdinaryCapsuleApi
{
    /// <summary>Registers an item for use with Ordinary Capsule.</summary>
    /// <param name="item">The item(s) that can be duplicated..</param>
    public void RegisterItem(ICapsuleItem item);
}
