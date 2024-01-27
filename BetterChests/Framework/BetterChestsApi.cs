namespace StardewMods.BetterChests.Framework;

using Microsoft.Xna.Framework;
using StardewMods.BetterChests.Framework.Services;
using StardewMods.BetterChests.Framework.Services.Factory;
using StardewMods.Common.Services.Integrations.BetterChests.Interfaces;
using StardewMods.GarbageDay.Services.Integrations.BetterChests;

/// <inheritdoc />
public sealed class BetterChestsApi : IBetterChestsApi
{
    private readonly ContainerFactory containerFactory;
    private readonly ContainerHandler containerHandler;
    private readonly IModInfo modInfo;

    /// <summary>Initializes a new instance of the <see cref="BetterChestsApi" /> class.</summary>
    /// <param name="containerHandler">Dependency used for handling operations between containers.</param>
    /// <param name="containerFactory">Dependency used for accessing containers.</param>
    /// <param name="modInfo">Mod info from the calling mod.</param>
    internal BetterChestsApi(ContainerHandler containerHandler, ContainerFactory containerFactory, IModInfo modInfo)
    {
        this.containerHandler = containerHandler;
        this.containerFactory = containerFactory;
        this.modInfo = modInfo;
    }

    /// <inheritdoc />
    public event EventHandler<EventArgs>? ItemTransferred;

    /// <inheritdoc />
    public IEnumerable<IStorageContainer> GetAllContainers(Func<IStorageContainer, bool>? predicate = default) =>
        this.containerFactory.GetAll(predicate);

    /// <inheritdoc />
    public IEnumerable<IStorageContainer> GetAllContainersFromLocation(
        GameLocation location,
        Func<IStorageContainer, bool>? predicate = default) =>
        this.containerFactory.GetAllFromLocation(location, predicate);

    /// <inheritdoc />
    public IEnumerable<IStorageContainer> GetAllContainersFromPlayer(
        Farmer farmer,
        Func<IStorageContainer, bool>? predicate = default) =>
        this.containerFactory.GetAllFromPlayer(farmer, predicate);

    /// <inheritdoc />
    public IEnumerable<(string ItemId, int Amount, bool Prevented)> Transfer(
        IStorageContainer containerFrom,
        IStorageContainer containerTo)
    {
        if (!this.containerHandler.Transfer(containerFrom, containerTo, out var amounts))
        {
            yield break;
        }

        foreach (var (itemId, amount) in amounts)
        {
            yield return (itemId, amount, false);
        }
    }

    /// <inheritdoc />
    public bool TryGetContainerFromLocation(
        GameLocation location,
        Vector2 pos,
        [NotNullWhen(true)] out IStorageContainer? container) =>
        this.containerFactory.TryGetOneFromLocation(location, pos, out container);

    /// <inheritdoc />
    public bool TryGetContainerFromMenu([NotNullWhen(true)] out IStorageContainer? container) =>
        this.containerFactory.TryGetOneFromMenu(out container);

    /// <inheritdoc />
    public bool TryGetContainerFromPlayer(
        Farmer farmer,
        [NotNullWhen(true)] out IStorageContainer? container,
        int index = -1) =>
        this.containerFactory.TryGetOneFromPlayer(farmer, out container, index);

    /// <inheritdoc />
    public bool TryGetContainerFromBackpack(Farmer farmer, [NotNullWhen(true)] out IStorageContainer? container) =>
        this.containerFactory.TryGetOne(farmer, out container);

    /// <inheritdoc />
    public bool TryGetContainerFromItem(Item item, [NotNullWhen(true)] out IStorageContainer? container) =>
        this.containerFactory.TryGetOne(item, out container);
}