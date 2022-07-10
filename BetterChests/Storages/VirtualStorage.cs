namespace StardewMods.BetterChests.Storages;

using System.Collections.Generic;
using System.Linq;
using StardewMods.Common.Enums;
using StardewMods.Common.Integrations.BetterChests;
using StardewValley;
using StardewValley.Menus;

/// <inheritdoc />
internal class VirtualStorage : BaseStorage
{
    private readonly List<Item> _cachedItems = new();

    /// <summary>
    ///     Initializes a new instance of the <see cref="VirtualStorage" /> class.
    /// </summary>
    /// <param name="defaultChest">Config options for <see cref="ModConfig.DefaultChest" />.</param>
    public VirtualStorage(IStorageData defaultChest)
        : base(Game1.player, Game1.player, defaultChest, Game1.player.getTileLocation())
    {
        this.FromStorages = Features.CraftFromChest.Eligible.ToList();
        this.ToStorages = Features.StashToChest.Eligible.OrderByDescending(storage => storage.StashToChestPriority).ToList();
        this.RefreshItems();
    }

    public List<IStorageObject> FromStorages { get; }

    /// <inheritdoc />
    public override IList<Item?> Items
    {
        get => this._cachedItems!;
    }

    /// <inheritdoc />
    public override ModDataDictionary ModData
    {
        get => Game1.player.modData;
    }

    public List<IStorageObject> ToStorages { get; }

    /// <inheritdoc />
    public override void GrabInventoryItem(Item item, Farmer who)
    {
        Item? tmp = null;
        foreach (var storage in this.ToStorages)
        {
            tmp = storage.StashItem(item, storage.StashToChestStacks != FeatureOption.Disabled);
            if (tmp is null)
            {
                who.removeItemFromInventory(item);
                break;
            }
        }

        this.RefreshItems();
        var oldId = Game1.activeClickableMenu.currentlySnappedComponent != null ? Game1.activeClickableMenu.currentlySnappedComponent.myID : -1;
        this.ShowMenu();
        ((ItemGrabMenu)Game1.activeClickableMenu).heldItem = tmp;
        if (oldId != -1)
        {
            Game1.activeClickableMenu.currentlySnappedComponent = Game1.activeClickableMenu.getComponentWithID(oldId);
            Game1.activeClickableMenu.snapCursorToCurrentSnappedComponent();
        }
    }

    /// <inheritdoc />
    public override void GrabStorageItem(Item item, Farmer who)
    {
        var stack = item.Stack;
        if (who.couldInventoryAcceptThisItem(item))
        {
            foreach (var storage in this.FromStorages)
            {
                var existingItem = storage.Items.OfType<Item>().FirstOrDefault(existingItem => existingItem.canStackWith(item));
                if (existingItem is not null)
                {
                    if (existingItem.Stack > stack)
                    {
                        stack = 0;
                        existingItem.Stack -= item.Stack;
                    }
                    else
                    {
                        stack -= existingItem.Stack;
                        storage.Items.Remove(existingItem);
                    }
                }

                if (stack <= 0)
                {
                    break;
                }
            }

            this.RefreshItems();
            this.ShowMenu();
        }
    }

    public void RefreshItems()
    {
        this._cachedItems.Clear();
        foreach (var item in this.FromStorages.SelectMany(storage => storage.Items))
        {
            if (item is null)
            {
                continue;
            }

            if (item.Stack == 0)
            {
                item.Stack = 1;
            }

            var existingItem = this._cachedItems.FirstOrDefault(existingItem => existingItem.canStackWith(item));
            if (existingItem is not null)
            {
                item.Stack = existingItem.addToStack(item);
                if (item.Stack <= 0)
                {
                    continue;
                }
            }

            this._cachedItems.Add(item);
        }
    }

    /// <inheritdoc />
    public override void ShowMenu()
    {
        Game1.activeClickableMenu = new ItemGrabMenu(
            this.Items,
            false,
            true,
            InventoryMenu.highlightAllItems,
            this.GrabInventoryItem,
            null,
            this.GrabStorageItem,
            false,
            true,
            true,
            true,
            false,
            1,
            null,
            -1,
            this);
    }
}