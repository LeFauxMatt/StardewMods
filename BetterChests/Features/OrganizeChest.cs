namespace StardewMods.BetterChests.Features;

using System.Linq;
using Common.Enums;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewMods.BetterChests.Enums;
using StardewMods.BetterChests.Helpers;
using StardewMods.BetterChests.Interfaces;
using StardewMods.BetterChests.Storages;
using StardewValley;
using StardewValley.Menus;
using SObject = StardewValley.Object;

/// <summary>
///     Sort items in a chest using a customized criteria.
/// </summary>
internal class OrganizeChest : IFeature
{
    private OrganizeChest(IModHelper helper)
    {
        this.Helper = helper;
    }

    private static OrganizeChest? Instance { get; set; }

    private IModHelper Helper { get; }

    /// <summary>
    ///     Initializes <see cref="OrganizeChest" />.
    /// </summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <returns>Returns an instance of the <see cref="OrganizeChest" /> class.</returns>
    public static OrganizeChest Init(IModHelper helper)
    {
        return OrganizeChest.Instance ??= new(helper);
    }

    /// <summary>
    ///     Organizes items in a storage.
    /// </summary>
    /// <param name="storage">The storage to organize.</param>
    public static void OrganizeItems(BaseStorage storage)
    {
        string OrderBy(Item item)
        {
            return storage.OrganizeChestGroupBy switch
            {
                GroupBy.Category => item.GetContextTags().FirstOrDefault(tag => tag.StartsWith("category_")),
                GroupBy.Color => item.GetContextTags().FirstOrDefault(tag => tag.StartsWith("color_")),
                GroupBy.Name => item.DisplayName,
            } ?? string.Empty;
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

        var items = storage.OrganizeChestOrderByDescending
            ? storage.Items.OfType<Item>()
                     .OrderByDescending(OrderBy)
                     .ThenByDescending(ThenBy)
                     .ToList()
            : storage.Items.OfType<Item>()
                     .OrderBy(OrderBy)
                     .ThenBy(ThenBy)
                     .ToList();

        storage.OrganizeChestOrderByDescending = !storage.OrganizeChestOrderByDescending;

        storage.Items.Clear();
        foreach (var item in items)
        {
            storage.Items.Add(item);
        }
    }

    /// <inheritdoc />
    public void Activate()
    {
        this.Helper.Events.Input.ButtonPressed += this.OnButtonPressed;
    }

    /// <inheritdoc />
    public void Deactivate()
    {
        this.Helper.Events.Input.ButtonPressed -= this.OnButtonPressed;
    }

    private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        if (e.Button is not SButton.MouseLeft
            || Game1.activeClickableMenu is not ItemGrabMenu { context: Item context } itemGrabMenu
            || !StorageHelper.TryGetOne(context, out var storage))
        {
            return;
        }

        var (x, y) = Game1.getMousePosition(true);
        if (itemGrabMenu.organizeButton?.containsPoint(x, y) != true)
        {
            return;
        }

        OrganizeChest.OrganizeItems(storage);
    }
}