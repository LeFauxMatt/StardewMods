namespace StardewMods.BetterChests.Framework.Models;

using StardewMods.Common.Integrations.BetterChests;

/// <inheritdoc />
internal sealed class StorageTypeRequestedEventArgs : IStorageTypeRequestedEventArgs
{
    private readonly List<KeyValuePair<IStorageData, int>> prioritizedTypes = new();
    private readonly IList<IStorageData> storageTypes;

    /// <summary>Initializes a new instance of the <see cref="StorageTypeRequestedEventArgs" /> class.</summary>
    /// <param name="context">The context object for the storage.</param>
    /// <param name="storageTypes">The types loaded for the storage.</param>
    public StorageTypeRequestedEventArgs(object context, IList<IStorageData> storageTypes)
    {
        this.Context = context;
        this.storageTypes = storageTypes;
    }

    /// <inheritdoc />
    public object Context { get; }

    /// <inheritdoc />
    public void Load(IStorageData data, int priority)
    {
        this.prioritizedTypes.Add(new(data, priority));
        this.prioritizedTypes.Sort((t1, t2) => -t1.Value.CompareTo(t2.Value));
        this.storageTypes.Clear();
        foreach (var (storageType, _) in this.prioritizedTypes)
        {
            this.storageTypes.Add(storageType);
        }
    }
}
