namespace StardewMods.BetterChests.Framework.StorageObjects;

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewMods.Common.Enums;
using StardewValley.Buildings;
using StardewValley.Menus;
using StardewValley.Objects;

/// <inheritdoc />
internal sealed class ShippingBinStorage : Storage
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ShippingBinStorage" /> class.
    /// </summary>
    /// <param name="location">The location of the shipping bin.</param>
    /// <param name="position">The position of the source object.</param>
    public ShippingBinStorage(GameLocation location, Vector2 position)
        : base(location, location, position)
    {
        // Do nothing
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ShippingBinStorage" /> class.
    /// </summary>
    /// <param name="shippingBin">The shipping bin.</param>
    /// <param name="source">The context where the source object is contained.</param>
    /// <param name="position">The position of the source object.</param>
    public ShippingBinStorage(ShippingBin shippingBin, object? source, Vector2 position)
        : base(shippingBin, source, position)
    {
        // Do nothing
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ShippingBinStorage" /> class.
    /// </summary>
    /// <param name="chest">The mini-shipping bin.</param>
    /// <param name="source">The context where the source object is contained.</param>
    /// <param name="position">The position of the source object.</param>
    public ShippingBinStorage(Chest chest, object? source, Vector2 position)
        : base(chest, source, position)
    {
        // Do nothing
    }

    /// <inheritdoc />
    public override int ActualCapacity =>
        this.Context is GameLocation or ShippingBin ? int.MaxValue : base.ActualCapacity;

    /// <inheritdoc />
    public override IList<Item?> Items =>
        this.Context switch
        {
            Chest chest => chest.GetItemsForPlayer(Game1.player.UniqueMultiplayerID),
            _ => Game1.getFarm().getShippingBin(Game1.player),
        };

    /// <inheritdoc />
    public override ModDataDictionary ModData =>
        this.Context switch
        {
            Building building => building.modData,
            GameLocation location => location.modData,
            Chest chest => chest.modData,
            _ => throw new ArgumentOutOfRangeException(),
        };

    /// <inheritdoc />
    public override FeatureOption UnloadChestCombine =>
        this.Context switch
        {
            GameLocation or ShippingBin => FeatureOption.Disabled,
            _ => base.UnloadChestCombine,
        };

    /// <summary>
    ///     Creates an <see cref="ItemGrabMenu" /> for this storage container.
    /// </summary>
    public override void ShowMenu()
    {
        var menu = new ItemGrabMenu(
            this.Items,
            false,
            true,
            Utility.highlightShippableObjects,
            this.GrabInventoryItem,
            null,
            this.GrabStorageItem,
            false,
            true,
            true,
            true,
            true,
            0,
            null,
            -1,
            this.Context);

        if (Game1.options.SnappyMenus
         && Game1.activeClickableMenu is ItemGrabMenu { currentlySnappedComponent: { } currentlySnappedComponent })
        {
            menu.setCurrentlySnappedComponentTo(currentlySnappedComponent.myID);
            menu.snapCursorToCurrentSnappedComponent();
        }

        Game1.activeClickableMenu = menu;
    }
}