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
using StardewMods.FuryCore.Models.CustomEvents;
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
        OrganizeChest.Instance.OrganizeItems();
    }

    private void OnClickableMenuChanged(object sender, ClickableMenuChangedEventArgs e)
    {
        this.CurrentStorage = e.Menu is ItemGrabMenu { context: { } context } && this.ManagedObjects.TryGetManagedStorage(context, out var managedStorage) && managedStorage.OrganizeChest == FeatureOption.Enabled
            ? managedStorage
            : null;
    }

    private string OrderBy(Item item)
    {
        return this.CurrentStorage.OrganizeChestGroupBy switch
        {
            GroupBy.Category => item.GetContextTags().FirstOrDefault(tag => tag.StartsWith("category_")),
            GroupBy.Color => item.GetContextTags().FirstOrDefault(tag => tag.StartsWith("color_")),
            GroupBy.Name => item.DisplayName,
            GroupBy.Default or _ => string.Empty,
        };
    }

    private void OrganizeItems()
    {
        if (this.CurrentStorage is null)
        {
            return;
        }

        var items = this.CurrentStorage.OrganizeChestOrderByDescending
            ? this.CurrentStorage.Items
                  .OrderByDescending(this.OrderBy)
                  .ThenByDescending(this.ThenBy)
                  .ToList()
            : this.CurrentStorage.Items
                  .OrderBy(this.OrderBy)
                  .ThenBy(this.ThenBy)
                  .ToList();
        this.CurrentStorage.OrganizeChestOrderByDescending = !this.CurrentStorage.OrganizeChestOrderByDescending;
        this.CurrentStorage.Items.Clear();
        foreach (var item in items)
        {
            this.CurrentStorage.Items.Add(item);
        }
    }

    private int ThenBy(Item item)
    {
        return this.CurrentStorage.OrganizeChestSortBy switch
        {
            SortBy.Quality when item is SObject obj => obj.Quality,
            SortBy.Quantity => item.Stack,
            SortBy.Type => item.Category,
            SortBy.Default or _ => 0,
        };
    }
}