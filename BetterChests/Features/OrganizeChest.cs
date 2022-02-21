namespace StardewMods.BetterChests.Features;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewMods.BetterChests.Enums;
using StardewMods.BetterChests.Interfaces.Config;
using StardewMods.BetterChests.Interfaces.ManagedObjects;
using StardewMods.FuryCore.Enums;
using StardewMods.FuryCore.Interfaces;
using StardewMods.FuryCore.Interfaces.CustomEvents;
using StardewValley;
using StardewValley.Menus;
using SObject = StardewValley.Object;

/// <inheritdoc />
internal class OrganizeChest : Feature
{
    private readonly PerScreen<IManagedStorage> _currentStorage = new();
    private readonly Lazy<IHarmonyHelper> _harmony;

    /// <summary>
    ///     Initializes a new instance of the <see cref="OrganizeChest" /> class.
    /// </summary>
    /// <param name="config">Data for player configured mod options.</param>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <param name="services">Provides access to internal and external services.</param>
    public OrganizeChest(IConfigModel config, IModHelper helper, IModServices services)
        : base(config, helper, services)
    {
        OrganizeChest.Instance = this;
        this._harmony = services.Lazy<IHarmonyHelper>(
            harmony =>
            {
                harmony.AddPatch(
                    this.Id,
                    AccessTools.Method(typeof(ItemGrabMenu), nameof(ItemGrabMenu.organizeItemsInList)),
                    typeof(OrganizeChest),
                    nameof(OrganizeChest.ItemGrabMenu_organizeItemsInList_postfix),
                    PatchType.Postfix);
            });
    }

    private static OrganizeChest Instance { get; set; }

    private IManagedStorage CurrentStorage
    {
        get => this._currentStorage.Value;
        set => this._currentStorage.Value = value;
    }

    private IHarmonyHelper Harmony
    {
        get => this._harmony.Value;
    }

    /// <summary>
    ///     Organizes items in a storage.
    /// </summary>
    /// <param name="storage">The storage to organize.</param>
    /// <param name="forceAscending">Forces ascending order.</param>
    public void OrganizeItems(IManagedStorage storage, bool forceAscending = false)
    {
        string OrderBy(Item item)
        {
            return storage.OrganizeChestGroupBy switch
            {
                GroupBy.Category => item.GetContextTags().FirstOrDefault(tag => tag.StartsWith("category_")),
                GroupBy.Color => item.GetContextTags().FirstOrDefault(tag => tag.StartsWith("color_")),
                GroupBy.Name => item.DisplayName,
                GroupBy.Default or _ => string.Empty,
            };
        }

        int ThenBy(Item item)
        {
            return storage.OrganizeChestSortBy switch
            {
                SortBy.Quality when item is SObject obj => obj.Quality,
                SortBy.Quantity => item.Stack,
                SortBy.Type => item.Category,
                SortBy.Default or _ => 0,
            };
        }

        var items = storage.OrganizeChestOrderByDescending && !forceAscending
            ? storage.Items
                     .OrderByDescending(OrderBy)
                     .ThenByDescending(ThenBy)
                     .ToList()
            : storage.Items
                     .OrderBy(OrderBy)
                     .ThenBy(ThenBy)
                     .ToList();
        if (!forceAscending)
        {
            storage.OrganizeChestOrderByDescending = !storage.OrganizeChestOrderByDescending;
        }

        storage.Items.Clear();
        foreach (var item in items)
        {
            storage.Items.Add(item);
        }
    }

    /// <inheritdoc />
    protected override void Activate()
    {
        this.CustomEvents.ClickableMenuChanged += this.OnClickableMenuChanged;
        this.Harmony.ApplyPatches(this.Id);
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        this.CustomEvents.ClickableMenuChanged -= this.OnClickableMenuChanged;
        this.Harmony.UnapplyPatches(this.Id);
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Naming is determined by Harmony.")]
    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Type is determined by Harmony.")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Naming is determined by Harmony.")]
    private static void ItemGrabMenu_organizeItemsInList_postfix()
    {
        if (OrganizeChest.Instance.CurrentStorage is not null)
        {
            OrganizeChest.Instance.OrganizeItems(OrganizeChest.Instance.CurrentStorage);
        }
    }

    private void OnClickableMenuChanged(object sender, IClickableMenuChangedEventArgs e)
    {
        this.CurrentStorage = e.Menu is ItemGrabMenu { context: { } context } && this.ManagedObjects.TryGetManagedStorage(context, out var managedStorage) && managedStorage.OrganizeChest == FeatureOption.Enabled
            ? managedStorage
            : null;
    }
}