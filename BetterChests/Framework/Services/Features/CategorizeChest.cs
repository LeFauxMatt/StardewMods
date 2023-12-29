namespace StardewMods.BetterChests.Framework.Services.Features;

using System.Reflection;
using System.Runtime.CompilerServices;
using HarmonyLib;
using StardewMods.BetterChests.Framework.Enums;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.BetterChests.Framework.Models.Events;
using StardewMods.BetterChests.Framework.Services.Factory;
using StardewMods.BetterChests.Framework.Services.Transient;
using StardewMods.Common.Services.Integrations.Automate;
using StardewMods.Common.Services.Integrations.FuryCore;
using StardewValley.Objects;

/// <summary>Restricts what items can be added into a chest.</summary>
internal sealed class CategorizeChest : BaseFeature<CategorizeChest>
{
#nullable disable
    private static CategorizeChest instance;
#nullable enable

    private readonly AutomateIntegration automateIntegration;
    private readonly ConditionalWeakTable<IContainer, ItemMatcher> cachedItemMatchers = new();
    private readonly ContainerFactory containerFactory;
    private readonly ContainerOperations containerOperations;
    private readonly Harmony harmony;
    private readonly ItemGrabMenuManager itemGrabMenuManager;
    private readonly ItemMatcherFactory itemMatcherFactory;
    private readonly IModRegistry modRegistry;
    private readonly IReflectionHelper reflectionHelper;

    private MethodBase? storeMethod;

    /// <summary>Initializes a new instance of the <see cref="CategorizeChest" /> class.</summary>
    /// <param name="automateIntegration">Dependency for integration with Automate.</param>
    /// <param name="configManager">Dependency used for accessing config data.</param>
    /// <param name="containerFactory">Dependency for handling storages.</param>
    /// <param name="containerOperations">Dependency used for handling operations between containers.</param>
    /// <param name="harmony">Dependency used to patch external code.</param>
    /// <param name="itemGrabMenuManager">Dependency used for managing the item grab menu.</param>
    /// <param name="itemMatcherFactory">Dependency used for getting an ItemMatcher.</param>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="modRegistry">Dependency used for fetching metadata about loaded mods.</param>
    /// <param name="reflectionHelper">Dependency used for accessing inaccessible code.</param>
    public CategorizeChest(
        AutomateIntegration automateIntegration,
        ConfigManager configManager,
        ContainerFactory containerFactory,
        ContainerOperations containerOperations,
        Harmony harmony,
        ItemGrabMenuManager itemGrabMenuManager,
        ItemMatcherFactory itemMatcherFactory,
        ILog log,
        IManifest manifest,
        IModRegistry modRegistry,
        IReflectionHelper reflectionHelper)
        : base(log, manifest, configManager)
    {
        CategorizeChest.instance = this;
        this.automateIntegration = automateIntegration;
        this.containerFactory = containerFactory;
        this.containerOperations = containerOperations;
        this.harmony = harmony;
        this.itemGrabMenuManager = itemGrabMenuManager;
        this.itemMatcherFactory = itemMatcherFactory;
        this.modRegistry = modRegistry;
        this.reflectionHelper = reflectionHelper;
    }

    /// <inheritdoc />
    public override bool ShouldBeActive => this.Config.DefaultOptions.CategorizeChest != Option.Disabled;

    /// <inheritdoc />
    protected override void Activate()
    {
        // Events
        this.itemGrabMenuManager.ItemGrabMenuChanged += this.OnItemGrabMenuChanged;
        this.containerOperations.ItemTransferring += this.OnItemTransferring;

        // Patches
        this.harmony.Patch(
            AccessTools.DeclaredMethod(typeof(Chest), nameof(Chest.addItem)),
            new HarmonyMethod(typeof(CategorizeChest), nameof(CategorizeChest.Chest_addItem_prefix)));

        // Integrations
        if (this.automateIntegration.IsLoaded)
        {
            return;
        }

        this.storeMethod = this
            .modRegistry.Get(this.automateIntegration.UniqueId)
            ?.GetType()
            .Assembly.GetType("Pathoschild.Stardew.Automate.Framework.Storage.ChestContainer")
            ?.GetMethod("Store", BindingFlags.Public | BindingFlags.Instance);

        if (this.storeMethod is not null)
        {
            this.harmony.Patch(
                this.storeMethod,
                new HarmonyMethod(typeof(CategorizeChest), nameof(CategorizeChest.Automate_Store_prefix)));
        }
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        // Events
        this.itemGrabMenuManager.ItemGrabMenuChanged -= this.OnItemGrabMenuChanged;
        this.containerOperations.ItemTransferring -= this.OnItemTransferring;

        // Patches
        this.harmony.Unpatch(
            AccessTools.DeclaredMethod(typeof(Chest), nameof(Chest.addItem)),
            AccessTools.Method(typeof(CategorizeChest), nameof(CategorizeChest.Chest_addItem_prefix)));

        // Integrations
        if (this.storeMethod is not null)
        {
            this.harmony.Unpatch(
                this.storeMethod,
                AccessTools.DeclaredMethod(typeof(CategorizeChest), nameof(CategorizeChest.Automate_Store_prefix)));
        }
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static bool Automate_Store_prefix(object stack, Chest ___Chest)
    {
        var item = CategorizeChest.instance.reflectionHelper.GetProperty<Item>(stack, "Sample").GetValue();
        if (!CategorizeChest.instance.containerFactory.TryGetOne(___Chest, out var container)
            || container.Options.CategorizeChest != Option.Enabled)
        {
            return true;
        }

        var itemMatcher = CategorizeChest.instance.GetOrCreateItemMatcher(container);
        return itemMatcher.MatchesFilter(item);
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    [HarmonyPriority(Priority.High)]
    private static bool Chest_addItem_prefix(Chest __instance, ref Item __result, Item item)
    {
        if (!CategorizeChest.instance.containerFactory.TryGetOne(__instance, out var container)
            || container.Options.CategorizeChest != Option.Enabled)
        {
            return true;
        }

        var itemMatcher = CategorizeChest.instance.GetOrCreateItemMatcher(container);
        if (itemMatcher.MatchesFilter(item))
        {
            return true;
        }

        __result = item;
        return false;
    }

    private void OnItemGrabMenuChanged(object? sender, ItemGrabMenuChangedEventArgs e)
    {
        if (this.itemGrabMenuManager.Top.Container?.Options.CategorizeChest == Option.Enabled)
        {
            var itemMatcher = this.GetOrCreateItemMatcher(this.itemGrabMenuManager.Top.Container);
            this.itemGrabMenuManager.Bottom.AddHighlightMethod(itemMatcher.MatchesFilter);
        }

        if (this.itemGrabMenuManager.Bottom.Container?.Options.CategorizeChest == Option.Enabled)
        {
            var itemMatcher = this.GetOrCreateItemMatcher(this.itemGrabMenuManager.Bottom.Container);
            this.itemGrabMenuManager.Top.AddHighlightMethod(itemMatcher.MatchesFilter);
        }
    }

    private void OnItemTransferring(object? sender, ItemTransferringEventArgs e)
    {
        if (e.To.Options.CategorizeChest != Option.Enabled)
        {
            return;
        }

        // Allow transfer if item matches categories
        var itemMatcher = this.GetOrCreateItemMatcher(e.To);
        if (!itemMatcher.IsEmpty && itemMatcher.MatchesFilter(e.Item))
        {
            return;
        }

        // Allow transfer if existing stacks are allowed and item is already in the chest
        if (this.Config.StashToChestStacks && e.To.Items.ContainsId(e.Item.ItemId))
        {
            return;
        }

        e.PreventTransfer();
    }

    private ItemMatcher GetOrCreateItemMatcher(IContainer container)
    {
        if (!this.cachedItemMatchers.TryGetValue(container, out var itemMatcher))
        {
            itemMatcher = this.itemMatcherFactory.GetDefault();
        }

        if (itemMatcher.IsEmpty && container.Options.CategorizeChestTags.Any())
        {
            itemMatcher.SearchText = string.Join(' ', container.Options.CategorizeChestTags);
        }

        this.cachedItemMatchers.AddOrUpdate(container, itemMatcher);
        return itemMatcher;
    }
}