namespace StardewMods.FuryCore.Models.GameObjects.Storages;

using System;
using System.Collections.Generic;
using System.Linq;
using StardewMods.FuryCore.Interfaces.GameObjects;
using StardewValley;
using StardewValley.Menus;

/// <inheritdoc cref="StardewMods.FuryCore.Interfaces.GameObjects.IStorageContainer" />
public abstract class StorageContainer : GameObject, IStorageContainer
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="StorageContainer" /> class.
    /// </summary>
    /// <param name="context">The source object.</param>
    /// <param name="getCapacity">A get method for the actual capacity of the storage.</param>
    /// <param name="getItems">A get method for the item in storage.</param>
    /// <param name="getModData">A get method for the mod data of the object.</param>
    protected StorageContainer(object context, Func<int> getCapacity, Func<IList<Item>> getItems, Func<ModDataDictionary> getModData)
        : base(context)
    {
        this.GetCapacity = getCapacity;
        this.GetItems = getItems;
        this.GetModData = getModData;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="StorageContainer" /> class.
    /// </summary>
    /// <param name="context">The source object.</param>
    /// <param name="getModData">A get method for the mod data of the object.</param>
    protected StorageContainer(object context, Func<ModDataDictionary> getModData)
        : base(context)
    {
        this.GetModData = getModData;
    }

    /// <inheritdoc />
    public virtual int Capacity
    {
        get => this.GetCapacity.Invoke();
    }

    /// <inheritdoc />
    public virtual IList<Item> Items
    {
        get => this.GetItems.Invoke();
    }

    /// <inheritdoc />
    public override ModDataDictionary ModData
    {
        get => this.GetModData.Invoke();
    }

    private Func<int> GetCapacity { get; }

    private Func<IList<Item>> GetItems { get; }

    private Func<ModDataDictionary> GetModData { get; }

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