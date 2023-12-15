namespace StardewMods.BetterChests.Framework.Models.Storages;

using StardewMods.BetterChests.Framework.Interfaces;
using StardewValley.Network;

/// <inheritdoc />
internal sealed class HeldStorage : ObjectStorage
{
    private readonly IStorage storage;

    /// <summary>Initializes a new instance of the <see cref="HeldStorage" /> class.</summary>
    /// <param name="obj">The Object associated with the held storage object.</param>
    /// <param name="storage">The held storage object.</param>
    public HeldStorage(SObject obj, IStorage storage) : base(obj) => this.storage = storage;

    /// <summary
    public override IEnumerable<Item> Items => this.storage.Items;

    /// <inheritdoc />
    public override NetMutex Mutex => this.storage.Mutex;
}
