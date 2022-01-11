namespace FuryCore.Events;

using FuryCore.Models;
using FuryCore.Services;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;

/// <inheritdoc />
internal class ItemsHighlighted : SortedEventHandler<ItemsHighlightedEventArgs>
{
    private readonly PerScreen<InventoryMenu.highlightThisItem> _highlightMethod = new();
    private readonly PerScreen<Chest> _chest = new();
    private readonly PerScreen<ItemsHighlightedEventArgs> _args = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ItemsHighlighted"/> class.
    /// </summary>
    /// <param name="services"></param>
    public ItemsHighlighted(ServiceCollection services)
    {
        services.Lazy<CustomEvents>(events =>
        {
            events.ItemGrabMenuChanged += this.OnItemGrabMenuChanged;
        });
    }

    private ItemsHighlightedEventArgs Args
    {
        get => this._args.Value;
        set => this._args.Value = value;
    }

    private Chest Chest
    {
        get => this._chest.Value;
        set => this._chest.Value = value;
    }

    private InventoryMenu.highlightThisItem OldHighlightMethod
    {
        get => this._highlightMethod.Value;
        set => this._highlightMethod.Value = value;
    }

    private void OnItemGrabMenuChanged(object sender, ItemGrabMenuChangedEventArgs e)
    {
        this.Chest = e.Chest;

        if (e.ItemGrabMenu is null or { inventory.highlightMethod.Target: ItemsHighlighted }
            || this.Chest is null or not
            {
                playerChest.Value: true,
                SpecialChestType: Chest.SpecialChestTypes.None or Chest.SpecialChestTypes.JunimoChest or Chest.SpecialChestTypes.MiniShippingBin,
            })
        {
            this.Args = null;
            return;
        }

        this.OldHighlightMethod = e.ItemGrabMenu.inventory.highlightMethod;
        e.ItemGrabMenu.inventory.highlightMethod = this.NewHighlightMethod;
        this.Args = new(this.Chest);
        this.InvokeAll(this.Args);
    }

    private bool NewHighlightMethod(Item item)
    {
        return this.OldHighlightMethod?.Invoke(item) != false && this.Args?.HighlightMethod(item) != false;
    }
}