namespace StardewMods.SpritePatcher.Framework.Models;

using StardewMods.SpritePatcher.Framework.Interfaces;

/// <summary>Represents the constructor arguments for creating a PatchModel instance.</summary>
public sealed class PatchModelCtorArgs
{
    /// <summary>Initializes a new instance of the <see cref="PatchModelCtorArgs" /> class.</summary>
    /// <param name="id">The unique identifier for the mod.</param>
    /// <param name="contentModel">The content data model for the patch.</param>
    /// <param name="contentPack">The content pack associated with the mod.</param>
    /// <param name="monitor">Dependency used for monitoring and logging.</param>
    /// <param name="netEventManager">Dependency used for managing net field events.</param>
    /// <param name="spriteSheetManager">Dependency used for managing sprite sheets.</param>
    public PatchModelCtorArgs(
        string id,
        IContentModel contentModel,
        IContentPack contentPack,
        IMonitor monitor,
        INetEventManager netEventManager,
        ISpriteSheetManager spriteSheetManager)
    {
        this.Id = id;
        this.ContentModel = contentModel;
        this.ContentPack = contentPack;
        this.Monitor = monitor;
        this.NetEventManager = netEventManager;
        this.SpriteSheetManager = spriteSheetManager;
    }

    /// <summary>Gets the unique identifier for this mod.</summary>
    public string Id { get; }

    /// <summary>Gets the content data model for the patch.</summary>
    public IContentModel ContentModel { get; }

    /// <summary>Gets the content pack associated with this mod.</summary>
    public IContentPack ContentPack { get; }

    /// <summary>Gets the dependency used for monitoring and logging.</summary>
    public IMonitor Monitor { get; }

    /// <summary>Gets the dependency used for managing net field events.</summary>
    public INetEventManager NetEventManager { get; }

    /// <summary>Gets the dependency used for managing sprite sheets.</summary>
    public ISpriteSheetManager SpriteSheetManager { get; }
}