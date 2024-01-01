namespace StardewMods.GarbageDay.Framework.Services;

using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.FuryCore;

/// <summary>Represents a class for managing asset paths.</summary>
internal sealed class Definitions : BaseService
{
    /// <summary>The game path where the big craftable data is stored.</summary>
    public const string BigCraftablePath = "Data/BigCraftables";

    /// <summary>The game path where the garbage can data is stored.</summary>
    public const string GarbageCanPath = "Data/GarbageCans";

    /// <summary>Initializes a new instance of the <see cref="Definitions" /> class.</summary>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    public Definitions(ILog log, IManifest manifest)
        : base(log, manifest)
    {
        this.IconTexturePath = this.ModId + "/Icons";
        this.ItemId = this.ModId + "/GarbageCan";
        this.QualifiedItemId = "(BC)" + this.ItemId;
        this.TexturePath = this.ModId + "/Texture";
    }

    /// <summary>Gets the game path to the icon texture.</summary>
    public string IconTexturePath { get; }

    /// <summary>Gets the item id for the Garbage Can object.</summary>
    public string ItemId { get; }

    /// <summary>Gets the qualified item id for the Garbage Can object.</summary>
    public string QualifiedItemId { get; }

    /// <summary>Gets the game path to the garbage can texture.</summary>
    public string TexturePath { get; }
}