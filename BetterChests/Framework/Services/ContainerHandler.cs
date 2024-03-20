namespace StardewMods.BetterChests.Framework.Services;

using System.Reflection;
using HarmonyLib;
using StardewMods.BetterChests.Framework.Models.Events;
using StardewMods.BetterChests.Framework.Services.Factory;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.Automate;
using StardewMods.Common.Services.Integrations.BetterChests.Interfaces;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewValley.Objects;

/// <summary>Responsible for handling containers.</summary>
internal sealed class ContainerHandler : BaseService
{
    private static ContainerHandler instance = null!;

    private readonly ContainerFactory containerFactory;
    private readonly IEventPublisher eventPublisher;
    private readonly IReflectionHelper reflectionHelper;

    /// <summary>Initializes a new instance of the <see cref="ContainerHandler" /> class.</summary>
    /// <param name="automateIntegration">Dependency for integration with Automate.</param>
    /// <param name="containerFactory">Dependency used for accessing containers.</param>
    /// <param name="eventPublisher">Dependency used for publishing events.</param>
    /// <param name="harmony">Dependency used to patch external code.</param>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="modRegistry">Dependency used for fetching metadata about loaded mods.</param>
    /// <param name="reflectionHelper">Dependency used for reflecting into external code.</param>
    public ContainerHandler(
        AutomateIntegration automateIntegration,
        ContainerFactory containerFactory,
        IEventPublisher eventPublisher,
        Harmony harmony,
        ILog log,
        IManifest manifest,
        IModRegistry modRegistry,
        IReflectionHelper reflectionHelper)
        : base(log, manifest)
    {
        ContainerHandler.instance = this;
        this.containerFactory = containerFactory;
        this.eventPublisher = eventPublisher;
        this.reflectionHelper = reflectionHelper;

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(Chest), nameof(Chest.addItem)),
            new HarmonyMethod(typeof(ContainerHandler), nameof(ContainerHandler.Chest_addItem_prefix)));

        if (!automateIntegration.IsLoaded)
        {
            return;
        }

        var storeMethod = modRegistry
            .Get(automateIntegration.UniqueId)
            ?.GetType()
            .Assembly.GetType("Pathoschild.Stardew.Automate.Framework.Storage.ChestContainer")
            ?.GetMethod("Store", BindingFlags.Public | BindingFlags.Instance);

        if (storeMethod is not null)
        {
            harmony.Patch(
                storeMethod,
                new HarmonyMethod(typeof(ContainerHandler), nameof(ContainerHandler.Automate_Store_prefix)));
        }
    }

    /// <summary>Transfers items from one container to another.</summary>
    /// <param name="from">The container to transfer items from.</param>
    /// <param name="to">The container to transfer items to.</param>
    /// <param name="amounts">Output parameter that contains the transferred item amounts.</param>
    /// <param name="force">Indicates whether to attempt to force the transfer.</param>
    /// <returns>True if the transfer was successful and at least one item was transferred, otherwise False.</returns>
    public bool Transfer(
        IStorageContainer from,
        IStorageContainer to,
        [NotNullWhen(true)] out Dictionary<string, int>? amounts,
        bool force = false)
    {
        var items = new Dictionary<string, int>();
        from.ForEachItem(
            item =>
            {
                // Stop iterating if destination container is already at capacity
                if (to.Items.CountItemStacks() >= to.Capacity)
                {
                    return false;
                }

                var itemTransferringEventArgs = new ItemTransferringEventArgs(to, item, force);
                this.eventPublisher.Publish(itemTransferringEventArgs);
                if (itemTransferringEventArgs.IsPrevented)
                {
                    return true;
                }

                var stack = item.Stack;
                items.TryAdd(item.Name, 0);
                if (!to.TryAdd(item, out var remaining) || !from.TryRemove(item))
                {
                    return true;
                }

                var amount = stack - (remaining?.Stack ?? 0);
                items[item.Name] += amount;
                return true;
            });

        if (items.Any())
        {
            amounts = items;
            return true;
        }

        amounts = null;
        return false;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static bool Automate_Store_prefix(object stack, Chest ___Chest)
    {
        var item = ContainerHandler.instance.reflectionHelper.GetProperty<Item>(stack, "Sample").GetValue();
        return !ContainerHandler.instance.containerFactory.TryGetOne(___Chest, out var container)
            || ContainerHandler.instance.CanAddItem(container, item);
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    [HarmonyPriority(Priority.High)]
    private static bool Chest_addItem_prefix(Chest __instance, ref Item __result, Item item)
    {
        if (!ContainerHandler.instance.containerFactory.TryGetOne(__instance, out var container)
            || ContainerHandler.instance.CanAddItem(container, item, true))
        {
            return true;
        }

        __result = item;
        return false;
    }

    /// <summary>Checks if an item is allowed to be added to a container.</summary>
    /// <param name="to">The container to add the item to.</param>
    /// <param name="item">The item to add.</param>
    /// <param name="force">Indicates whether it should be a forced attempt.</param>
    /// <returns>True if the item can be added, otherwise False.</returns>
    private bool CanAddItem(IStorageContainer to, Item item, bool force = false)
    {
        if (to.Items.CountItemStacks() >= to.Capacity)
        {
            return false;
        }

        var itemTransferringEventArgs = new ItemTransferringEventArgs(to, item, force);
        this.eventPublisher.Publish(itemTransferringEventArgs);
        return !itemTransferringEventArgs.IsPrevented;
    }
}