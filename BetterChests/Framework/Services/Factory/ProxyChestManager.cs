namespace StardewMods.BetterChests.Framework.Services.Factory;

using System.Globalization;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.BetterChests.Framework.Models;
using StardewMods.Common.Interfaces;
using StardewValley.Objects;

/// <summary>Manages the global inventories and chest/item creation and retrieval operations.</summary>
internal sealed class ProxyChestManager : BaseService
{
    private const string AlphaNumeric = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
    private const string ColorKey = "PlayerChoiceColor";
    private const string GlobalInventoryIdKey = "GlobalInventoryId";

#nullable disable
    private static ProxyChestManager instance;
#nullable enable

    private readonly Dictionary<string, Chest> proxyChests = new();

    /// <summary>Initializes a new instance of the <see cref="ProxyChestManager" /> class.</summary>
    /// <param name="logging">Dependency used for logging debug information to the console.</param>
    /// <param name="harmony">Dependency used to patch external code.</param>
    public ProxyChestManager(ILogging logging, Harmony harmony)
        : base(logging)
    {
        // Init
        ProxyChestManager.instance = this;

        // Patches
        harmony.Patch(AccessTools.DeclaredMethod(typeof(Item), nameof(Item.canBeDropped)), postfix: new HarmonyMethod(typeof(ProxyChestManager), nameof(ProxyChestManager.Item_canBeDropped_postfix)));

        harmony.Patch(AccessTools.DeclaredMethod(typeof(Item), nameof(Item.canBeTrashed)), postfix: new HarmonyMethod(typeof(ProxyChestManager), nameof(ProxyChestManager.Item_canBeTrashed_postfix)));

        harmony.Patch(AccessTools.DeclaredMethod(typeof(Item), nameof(Item.canStackWith)), postfix: new HarmonyMethod(typeof(ProxyChestManager), nameof(ProxyChestManager.Item_canStackWith_postfix)));

        harmony.Patch(AccessTools.DeclaredMethod(typeof(SObject), nameof(SObject.drawInMenu)), postfix: new HarmonyMethod(typeof(ProxyChestManager), nameof(ProxyChestManager.Object_drawInMenu_postfix)));

        harmony.Patch(AccessTools.DeclaredMethod(typeof(SObject), nameof(SObject.drawWhenHeld)), new HarmonyMethod(typeof(ProxyChestManager), nameof(ProxyChestManager.Object_drawWhenHeld_prefix)));

        harmony.Patch(AccessTools.DeclaredMethod(typeof(SObject), nameof(SObject.maximumStackSize)), postfix: new HarmonyMethod(typeof(ProxyChestManager), nameof(ProxyChestManager.Object_maximumStackSize_postfix)));
    }

    /// <summary>
    /// Tries to create a chest request object and returns a boolean indicating whether the creation was successful or
    /// not.
    /// </summary>
    /// <param name="chest">The chest object from which to create the request.</param>
    /// <param name="request">
    /// When this method returns, contains the created ProxyChestRequest object if the creation was
    /// successful; otherwise, null.
    /// </param>
    /// <returns>true if the creation of the request was successful; otherwise, false.</returns>
    public bool TryCreateRequest(Chest chest, [NotNullWhen(true)] out ProxyChestRequest? request)
    {
        if (chest.GlobalInventoryId != null && (!Game1.player.team.globalInventories.ContainsKey(chest.GlobalInventoryId) || !chest.GlobalInventoryId.StartsWith(this.Prefix, StringComparison.OrdinalIgnoreCase)))
        {
            request = null;
            return false;
        }

        var id = chest.GlobalInventoryId ?? this.GenerateGlobalInventoryId();
        var globalInventory = Game1.player.team.GetOrCreateGlobalInventory(id);
        var item = ItemRegistry.Create(chest.QualifiedItemId);

        item.Name = chest.Name;
        item.modData[this.Prefix + ProxyChestManager.GlobalInventoryIdKey] = id;

        if (chest.playerChoiceColor.Value != Color.Black)
        {
            var c = chest.playerChoiceColor.Value;
            var color = (c.R << 0) | (c.G << 8) | (c.B << 16);
            item.modData[this.Prefix + ProxyChestManager.ColorKey] = color.ToString(CultureInfo.InvariantCulture);
        }

        foreach (var (key, value) in chest.modData.Pairs)
        {
            item.modData[key] = value;
        }

        this.proxyChests[id] = chest;
        request = new ProxyChestRequest(item, Confirm, Cancel);
        return true;

        // Move Items to global inventory
        void Confirm()
        {
            chest.GlobalInventoryId = id;
            globalInventory.OverwriteWith(chest.Items);
            chest.Items.Clear();
        }

        // Clear global inventory
        void Cancel()
        {
            Game1.player.team.globalInventories.Remove(id);
            Game1.player.team.globalInventoryMutexes.Remove(id);
            this.proxyChests.Remove(id);
        }
    }

    /// <summary>Determines if the given item represents a proxy chest.</summary>
    /// <param name="salable">The item to check.</param>
    /// <returns>True if the item is a proxy; otherwise, false.</returns>
    public bool IsProxy(ISalable salable) =>
        salable is Item item && item.modData.TryGetValue(this.Prefix + ProxyChestManager.GlobalInventoryIdKey, out var id) && Game1.player.team.globalInventories.ContainsKey(id) && this.proxyChests.ContainsKey(id);

    /// <summary>Tries to get the proxy chest from the specified source object.</summary>
    /// <param name="item">The item representing a proxy chest.</param>
    /// <param name="chest">When this method returns, the chest, if it exists; otherwise, null.</param>
    /// <returns>True if the proxy chest exists; otherwise, false.</returns>
    public bool TryGetProxy(Item item, [NotNullWhen(true)] out Chest? chest)
    {
        if (!item.modData.TryGetValue(this.Prefix + ProxyChestManager.GlobalInventoryIdKey, out var id) || !Game1.player.team.globalInventories.ContainsKey(id))
        {
            chest = null;
            return false;
        }

        if (this.proxyChests.TryGetValue(id, out chest))
        {
            return true;
        }

        var color = Color.Black;
        if (item.modData.TryGetValue(this.Prefix + ProxyChestManager.ColorKey, out var colorString) && int.TryParse(colorString, out var colorValue))
        {
            var r = (byte)(colorValue & 0xFF);
            var g = (byte)((colorValue >> 8) & 0xFF);
            var b = (byte)((colorValue >> 16) & 0xFF);
            color = new Color(r, g, b);
        }

        chest = new Chest(true, Vector2.Zero, item.ItemId)
        {
            Name = item.Name,
            GlobalInventoryId = id,
            playerChoiceColor = { Value = color },
        };

        foreach (var (key, value) in item.modData.Pairs)
        {
            chest.modData[key] = value;
        }

        this.proxyChests[id] = chest;
        return true;
    }

    /// <summary>Tries to restore the proxy chest with the specified <paramref name="chest" />.</summary>
    /// <param name="chest">The proxy chest to restore.</param>
    /// <returns>True if the proxy chest was successfully restored, false otherwise.</returns>
    public bool TryRestoreProxy(Chest chest)
    {
        if (chest.GlobalInventoryId == null || !Game1.player.team.globalInventories.ContainsKey(chest.GlobalInventoryId) || !chest.GlobalInventoryId.StartsWith(this.Prefix, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        // Move Items to Chest
        var id = chest.GlobalInventoryId;
        var globalInventory = Game1.player.team.GetOrCreateGlobalInventory(id);
        chest.GlobalInventoryId = null;
        chest.Items.OverwriteWith(globalInventory);

        // Clear Global Inventory
        Game1.player.team.globalInventories.Remove(id);
        Game1.player.team.globalInventoryMutexes.Remove(id);
        this.proxyChests.Remove(id);
        return true;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void Item_canBeDropped_postfix(Item __instance, ref bool __result)
    {
        if (__result && ProxyChestManager.instance.IsProxy(__instance))
        {
            __result = false;
        }
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void Item_canBeTrashed_postfix(Item __instance, ref bool __result)
    {
        if (__result && ProxyChestManager.instance.IsProxy(__instance))
        {
            __result = false;
        }
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void Item_canStackWith_postfix(Item __instance, ref bool __result, ISalable other)
    {
        if (__result && (ProxyChestManager.instance.IsProxy(__instance) || ProxyChestManager.instance.IsProxy(other)))
        {
            __result = false;
        }
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void Object_drawInMenu_postfix(SObject __instance, SpriteBatch spriteBatch, Vector2 location, float scaleSize, Color color)
    {
        if (!ProxyChestManager.instance.TryGetProxy(__instance, out var chest))
        {
            return;
        }

        // Draw Items count
        var items = chest.GetItemsForPlayer(Game1.player.UniqueMultiplayerID).Count;
        if (items <= 0)
        {
            return;
        }

        var position = location + new Vector2(Game1.tileSize - Utility.getWidthOfTinyDigitString(items, 3f * scaleSize) - (3f * scaleSize), 2f * scaleSize);

        Utility.drawTinyDigits(items, spriteBatch, position, 3f * scaleSize, 1f, color);
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static bool Object_drawWhenHeld_prefix(SObject __instance, SpriteBatch spriteBatch, Vector2 objectPosition)
    {
        if (!ProxyChestManager.instance.TryGetProxy(__instance, out var chest))
        {
            return true;
        }

        var (x, y) = objectPosition;
        chest.draw(spriteBatch, (int)x, (int)y + Game1.tileSize, 1f, true);
        return false;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void Object_maximumStackSize_postfix(SObject __instance, ref int __result)
    {
        if (__result > 0 && ProxyChestManager.instance.IsProxy(__instance))
        {
            __result = -1;
        }
    }

    private static string RandomString()
    {
        var stringChars = new char[16];
        var random = new Random();

        for (var i = 0; i < stringChars.Length; i++)
        {
            stringChars[i] = ProxyChestManager.AlphaNumeric[random.Next(ProxyChestManager.AlphaNumeric.Length)];
        }

        return new string(stringChars);
    }

    private string GenerateGlobalInventoryId()
    {
        var globalInventoryId = this.Prefix + ProxyChestManager.RandomString();
        while (Game1.player.team.globalInventories.ContainsKey(globalInventoryId) || Game1.player.team.globalInventoryMutexes.ContainsKey(globalInventoryId))
        {
            globalInventoryId = this.Prefix + ProxyChestManager.RandomString();
        }

        return globalInventoryId;
    }
}
