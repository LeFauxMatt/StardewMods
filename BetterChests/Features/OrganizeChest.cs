namespace StardewMods.BetterChests.Features;

using System.Linq;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewMods.BetterChests.Helpers;
using StardewMods.BetterChests.Storages;
using StardewValley;
using StardewValley.Menus;

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
        var items = storage.OrganizeChestOrderByDescending
            ? storage.Items.OfType<Item>()
                     .OrderByDescending(storage.OrderBy)
                     .ThenByDescending(storage.ThenBy)
                     .ToList()
            : storage.Items.OfType<Item>()
                     .OrderBy(storage.OrderBy)
                     .ThenBy(storage.ThenBy)
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
        this.Helper.Events.Input.ButtonPressed += OrganizeChest.OnButtonPressed;
    }

    /// <inheritdoc />
    public void Deactivate()
    {
        this.Helper.Events.Input.ButtonPressed -= OrganizeChest.OnButtonPressed;
    }

    private static void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
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