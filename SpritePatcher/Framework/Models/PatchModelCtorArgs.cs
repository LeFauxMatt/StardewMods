namespace StardewMods.SpritePatcher.Framework.Models;

using StardewMods.SpritePatcher.Framework.Interfaces;

/// <summary>Represents the constructor arguments for creating a PatchModel instance.</summary>
public sealed class PatchModelCtorArgs(
    IMonitor monitor,
    INetFieldManager netFieldManager,
    string id,
    IContentPack contentPack,
    IContentModel contentModel)
{
    /// <summary>Gets the dependency used for monitoring and logging.</summary>
    public IMonitor Monitor { get; } = monitor;

    /// <summary>Gets the dependency used for managing net field events.</summary>
    public INetFieldManager NetFieldManager { get; } = netFieldManager;

    /// <summary>Gets the unique identifier for this mod.</summary>
    public string Id { get; } = id;

    /// <summary>Gets the content pack associated with this mod.</summary>
    public IContentPack ContentPack { get; } = contentPack;

    /// <summary>Gets the content data model for the patch.</summary>
    public IContentModel ContentModel { get; } = contentModel;
}