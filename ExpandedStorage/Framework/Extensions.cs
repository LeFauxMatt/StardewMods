namespace StardewMods.ExpandedStorage.Framework;

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.Common.Enums;
using StardewMods.Common.Integrations.BetterChests;
using StardewMods.Common.Integrations.ExpandedStorage;
using StardewMods.ExpandedStorage.Models;
using StardewValley.Objects;

/// <summary>
///     Extension methods for Expanded Storage.
/// </summary>
internal static class Extensions
{
#nullable disable
    private static IDictionary<string, CachedStorage> storageCache;
#nullable enable

    /// <summary>
    ///     Copies one instance BetterChestsData to another.
    /// </summary>
    /// <param name="storageData">The BetterChestsData instance to copy from.</param>
    /// <param name="other">The BetterChestsData instance to copy to.</param>
    public static void CopyTo(this IStorageData storageData, IStorageData other)
    {
        if (other.AutoOrganize is FeatureOption.Default)
        {
            other.AutoOrganize = storageData.AutoOrganize;
        }

        if (other.CarryChest is FeatureOption.Default)
        {
            other.CarryChest = storageData.CarryChest;
        }

        if (other.CarryChestSlow is FeatureOption.Default)
        {
            other.CarryChestSlow = storageData.CarryChestSlow;
        }

        if (other.ChestInfo is FeatureOption.Default)
        {
            other.ChestInfo = storageData.ChestInfo;
        }

        if (other.ChestMenuTabs is FeatureOption.Default)
        {
            other.ChestMenuTabs = storageData.ChestMenuTabs;
            other.ChestMenuTabSet = storageData.ChestMenuTabSet;
        }

        if (other.CollectItems is FeatureOption.Default)
        {
            other.CollectItems = storageData.CollectItems;
        }

        if (other.Configurator is FeatureOption.Default)
        {
            other.Configurator = storageData.Configurator;
        }

        if (other.ConfigureMenu is InGameMenu.Default)
        {
            other.ConfigureMenu = storageData.ConfigureMenu;
        }

        if (other.CraftFromChest is FeatureOptionRange.Default)
        {
            other.CraftFromChest = storageData.CraftFromChest;
            other.CraftFromChestDisableLocations = storageData.CraftFromChestDisableLocations;
        }

        if (other.CraftFromChestDistance == 0)
        {
            other.CraftFromChestDistance = storageData.CraftFromChestDistance;
        }

        if (other.CustomColorPicker is FeatureOption.Default)
        {
            other.CustomColorPicker = storageData.CustomColorPicker;
        }

        if (other.FilterItems is FeatureOption.Default)
        {
            other.FilterItems = storageData.FilterItems;
            other.FilterItemsList = storageData.FilterItemsList;
        }

        if (other.HideItems is FeatureOption.Default)
        {
            other.HideItems = storageData.HideItems;
        }

        if (other.LabelChest is FeatureOption.Default)
        {
            other.LabelChest = storageData.LabelChest;
        }

        if (other.OpenHeldChest is FeatureOption.Default)
        {
            other.OpenHeldChest = storageData.OpenHeldChest;
        }

        if (other.OrganizeChest is FeatureOption.Default)
        {
            other.OrganizeChest = storageData.OrganizeChest;
        }

        if (other.OrganizeChestGroupBy is GroupBy.Default)
        {
            other.OrganizeChestGroupBy = storageData.OrganizeChestGroupBy;
        }

        if (other.OrganizeChestSortBy is SortBy.Default)
        {
            other.OrganizeChestSortBy = storageData.OrganizeChestSortBy;
        }

        if (other.ResizeChest is FeatureOption.Default)
        {
            other.ResizeChest = storageData.ResizeChest;
        }

        if (other.ResizeChestCapacity == 0)
        {
            other.ResizeChestCapacity = storageData.ResizeChestCapacity;
        }

        if (other.SearchItems is FeatureOption.Default)
        {
            other.SearchItems = storageData.SearchItems;
        }

        if (other.StashToChest is FeatureOptionRange.Default)
        {
            other.StashToChest = storageData.StashToChest;
            other.StashToChestDisableLocations = storageData.StashToChestDisableLocations;
            other.StashToChestStacks = storageData.StashToChestStacks;
        }

        if (other.StashToChestDistance == 0)
        {
            other.StashToChestDistance = storageData.StashToChestDistance;
        }

        if (other.StashToChestPriority == 0)
        {
            other.StashToChestPriority = storageData.StashToChestPriority;
        }

        if (other.TransferItems is FeatureOption.Default)
        {
            other.TransferItems = storageData.TransferItems;
        }

        if (other.UnloadChest is FeatureOption.Default)
        {
            other.UnloadChest = storageData.UnloadChest;
        }

        if (other.UnloadChestCombine is FeatureOption.Default)
        {
            other.UnloadChestCombine = storageData.UnloadChestCombine;
        }
    }

    /// <summary>
    ///     Draws an Expanded Storage chest.
    /// </summary>
    /// <param name="storage">The storage to draw.</param>
    /// <param name="obj">The source object.</param>
    /// <param name="currentLidFrame">The current animation frame.</param>
    /// <param name="b">The sprite batch to draw to.</param>
    /// <param name="pos">The position to draw the storage at.</param>
    /// <param name="color">The color to draw the chest.</param>
    /// <param name="origin">The origin of the texture.</param>
    /// <param name="alpha">The alpha level to draw.</param>
    /// <param name="scaleSize">The scale size.</param>
    /// <param name="layerDepth">The layer depth.</param>
    public static void Draw(
        this ICustomStorage storage,
        SObject obj,
        int currentLidFrame,
        SpriteBatch b,
        Vector2 pos,
        Color? color = null,
        Vector2? origin = null,
        float alpha = 1f,
        float scaleSize = 1f,
        float layerDepth = 0.0001f)
    {
        var cachedStorage = Extensions.storageCache.Get(storage);
        var startingLidFrame = (obj as Chest)?.startingLidFrame.Value ?? 0;
        var lastLidFrame = (obj as Chest)?.getLastLidFrame() ?? 1;
        var colored = storage.PlayerColor && (obj as Chest)?.playerChoiceColor.Value.Equals(Color.Black) == false;
        var tint = (obj as Chest)?.Tint ?? Color.White;
        var frame = new Rectangle(
            Math.Min((startingLidFrame + lastLidFrame) - 1, Math.Max(0, currentLidFrame - startingLidFrame))
            * storage.Width,
            colored ? storage.Height : 0,
            storage.Width,
            storage.Height);

        // Draw Base Layer
        b.Draw(
            cachedStorage.Texture,
            pos + (obj.shakeTimer > 0 ? new(Game1.random.Next(-1, 2), 0) : Vector2.Zero),
            frame,
            (color ?? tint) * alpha,
            0f,
            origin ?? Vector2.Zero,
            Game1.pixelZoom * scaleSize,
            SpriteEffects.None,
            layerDepth);
        if (frame.Y == 0)
        {
            return;
        }

        frame.Y = storage.Height * 2;

        // Draw Top Layer
        b.Draw(
            cachedStorage.Texture,
            pos + (obj.shakeTimer > 0 ? new(Game1.random.Next(-1, 2), 0) : Vector2.Zero),
            frame,
            tint * alpha,
            0f,
            origin ?? Vector2.Zero,
            Game1.pixelZoom * scaleSize,
            SpriteEffects.None,
            layerDepth + 1E-05f);
    }

    /// <summary>
    /// Gets or adds a storage from cache.
    /// </summary>
    /// <param name="cachedStorages">The storage cache to get from.</param>
    /// <param name="storage">The storage to get from cache.</param>
    /// <returns>Returns the cached storage.</returns>
    public static CachedStorage Get(this IDictionary<string, CachedStorage> cachedStorages, ICustomStorage storage)
    {
        if (cachedStorages.TryGetValue(storage.Image, out var cachedStorage))
        {
            return cachedStorage;
        }

        cachedStorage = new(storage);
        cachedStorages.Add(storage.Image, new(storage));
        return cachedStorage;
    }

    /// <summary>
    ///     Gets the frame count of a custom storage's lid opening animation .
    /// </summary>
    /// <param name="storage">The custom storage.</param>
    /// <returns>Returns the frame count.</returns>
    public static int GetFrames(this ICustomStorage storage) => Extensions.storageCache.Get(storage).Frames;

    /// <summary>
    ///     Gets the scale multiplier of a custom storage.
    /// </summary>
    /// <param name="storage">The custom storage.</param>
    /// <returns>Returns the scale multiplier.</returns>
    public static float GetScaleMultiplier(this ICustomStorage storage) => Extensions.storageCache.Get(storage).ScaleMultiplier;

    /// <summary>
    ///     Gets the tile depth of a custom storage.
    /// </summary>
    /// <param name="storage">The custom storage.</param>
    /// <returns>Returns the tile depth.</returns>
    public static int GetTileDepth(this ICustomStorage storage) => Extensions.storageCache.Get(storage).TileDepth;

    /// <summary>
    ///     Gets the tile height of a custom storage.
    /// </summary>
    /// <param name="storage">The custom storage.</param>
    /// <returns>Returns the tile height.</returns>
    public static int GetTileHeight(this ICustomStorage storage) => Extensions.storageCache.Get(storage).TileHeight;

    /// <summary>
    ///     Gets the tile width of a custom storage.
    /// </summary>
    /// <param name="storage">The custom storage.</param>
    /// <returns>Returns the tile width.</returns>
    public static int GetTileWidth(this ICustomStorage storage) => Extensions.storageCache.Get(storage).TileWidth;

    /// <summary>
    ///     Initialized <see cref="Extensions" />.
    /// </summary>
    /// <param name="cachedStorages">Cached storage textures and attributes.</param>
    public static void Init(IDictionary<string, CachedStorage> cachedStorages) => Extensions.storageCache = cachedStorages;
}
