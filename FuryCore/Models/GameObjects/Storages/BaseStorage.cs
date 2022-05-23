#nullable disable

namespace StardewMods.FuryCore.Models.GameObjects.Storages;

using System.Collections.Generic;
using System.Linq;
using StardewMods.FuryCore.Interfaces.GameObjects;
using StardewValley;
using StardewValley.Menus;

/// <inheritdoc cref="StardewMods.FuryCore.Interfaces.GameObjects.IStorageContainer" />
public abstract class BaseStorage : GameObject, IStorageContainer
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="BaseStorage" /> class.
    /// </summary>
    /// <param name="context">The source object.</param>
    protected BaseStorage(object context)
        : base(context)
    {
    }

    /// <inheritdoc />
    public abstract int Capacity { get; }

    /// <inheritdoc />
    public abstract IList<Item> Items { get; }

    /// <inheritdoc />
    public abstract override ModDataDictionary ModData { get; }

    /// <inheritdoc />
    public virtual Item AddItem(Item item)
    {
        item.resetState();
        this.ClearNulls();
        foreach (var existingItem in this.Items.Where(existingItem => existingItem.canStackWith(item)))
        {
            item.Stack = existingItem.addToStack(item);
            if (item.Stack <= 0)
            {
                return null;
            }
        }

        if (this.Items.Count < this.Capacity)
        {
            this.Items.Add(item);
            return null;
        }

        return item;
    }

    /// <inheritdoc />
    public virtual void ClearNulls()
    {
        for (var index = this.Items.Count - 1; index >= 0; index--)
        {
            if (this.Items[index] is null)
            {
                this.Items.RemoveAt(index);
            }
        }
    }

    /// <inheritdoc />
    public virtual void GrabInventoryItem(Item item, Farmer who)
    {
        if (item.Stack == 0)
        {
            item.Stack = 1;
        }

        var tmp = this.AddItem(item);
        if (tmp == null)
        {
            who.removeItemFromInventory(item);
        }
        else
        {
            tmp = who.addItemToInventory(tmp);
        }

        this.ClearNulls();
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
    public virtual void GrabStorageItem(Item item, Farmer who)
    {
        if (who.couldInventoryAcceptThisItem(item))
        {
            this.Items.Remove(item);
            this.ClearNulls();
            this.ShowMenu();
        }
    }

    /// <inheritdoc />
    public virtual void ShowMenu()
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
            true,
            1,
            null,
            -1,
            this.Context);
    }
}