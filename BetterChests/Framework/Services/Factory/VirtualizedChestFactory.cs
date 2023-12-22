namespace StardewMods.BetterChests.Framework.Services.Factory;

using System.Globalization;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.BetterChests.Framework.Models;
using StardewMods.BetterChests.Framework.Models.Containers;
using StardewMods.Common.Extensions;
using StardewMods.Common.Interfaces;
using StardewValley.Menus;
using StardewValley.Objects;

/// <summary>Manages the global inventories and chest/item creation and retrieval operations.</summary>
internal sealed class VirtualizedChestFactory : BaseService
{
    private const string VirtualizedChestDataKey = "furyx639.BetterChests_VirtualizedChests";

#nullable disable
    private static VirtualizedChestFactory instance;
#nullable enable
    private readonly IDataHelper data;

    private readonly Dictionary<string, VirtualizedChest> vChests = new();

    /// <summary>Initializes a new instance of the <see cref="VirtualizedChestFactory" /> class.</summary>
    /// <param name="logging">Dependency used for logging debug information to the console.</param>
    /// <param name="data">Dependency used for storing and retrieving data.</param>
    /// <param name="events">Dependency used for managing access to events.</param>
    /// <param name="harmony">Dependency used to patch external code.</param>
    public VirtualizedChestFactory(ILogging logging, IDataHelper data, IModEvents events, Harmony harmony)
        : base(logging)
    {
        // Init
        VirtualizedChestFactory.instance = this;
        this.data = data;

        // Events
        events.GameLoop.SaveLoaded += this.OnSaveLoaded;
        events.GameLoop.Saving += this.OnSaving;

        // Patches
        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(InventoryMenu), nameof(InventoryMenu.rightClick)),
            new HarmonyMethod(typeof(VirtualizedChestFactory), nameof(VirtualizedChestFactory.InventoryMenu_rightClick_prefix)),
            new HarmonyMethod(typeof(VirtualizedChestFactory), nameof(VirtualizedChestFactory.InventoryMenu_rightClick_postfix)));

        harmony.Patch(AccessTools.DeclaredMethod(typeof(Item), nameof(Item.canBeDropped)), postfix: new HarmonyMethod(typeof(VirtualizedChestFactory), nameof(VirtualizedChestFactory.Item_canBeDropped_postfix)));

        harmony.Patch(AccessTools.DeclaredMethod(typeof(Item), nameof(Item.canBeTrashed)), postfix: new HarmonyMethod(typeof(VirtualizedChestFactory), nameof(VirtualizedChestFactory.Item_canBeTrashed_postfix)));

        harmony.Patch(AccessTools.DeclaredMethod(typeof(Item), nameof(Item.canStackWith)), postfix: new HarmonyMethod(typeof(VirtualizedChestFactory), nameof(VirtualizedChestFactory.Item_canStackWith_postfix)));

        harmony.Patch(AccessTools.DeclaredMethod(typeof(SObject), nameof(SObject.drawInMenu)), postfix: new HarmonyMethod(typeof(VirtualizedChestFactory), nameof(VirtualizedChestFactory.Object_drawInMenu_postfix)));

        harmony.Patch(AccessTools.DeclaredMethod(typeof(SObject), nameof(SObject.drawWhenHeld)), new HarmonyMethod(typeof(VirtualizedChestFactory), nameof(VirtualizedChestFactory.Object_drawWhenHeld_prefix)));

        harmony.Patch(
            AccessTools.DeclaredMethod(typeof(SObject), nameof(SObject.maximumStackSize)),
            postfix: new HarmonyMethod(typeof(VirtualizedChestFactory), nameof(VirtualizedChestFactory.Object_maximumStackSize_postfix)));
    }

    /// <summary>Tries to get a virtualized chest from the specified chest.</summary>
    /// <param name="container">The storage to create the new item based on.</param>
    /// <param name="vChest">The created virtualized chest, if successful; otherwise, null.</param>
    /// <returns>Returns true if the virtualized chest creation was successful; otherwise, false.</returns>
    public bool TryGetOne(IContainer container, [NotNullWhen(true)] out VirtualizedChest? vChest)
    {
        if (container is not ChestContainer chestStorage || chestStorage.Chest.GlobalInventoryId != null)
        {
            vChest = null;
            return false;
        }

        vChest = new VirtualizedChest(chestStorage.Chest);
        this.vChests[vChest.GlobalInventoryId] = vChest;
        return true;
    }

    /// <summary>Tries to get the virtualized chest with the specified ID from the dictionary.</summary>
    /// <param name="id">The ID of the virtualized chest.</param>
    /// <param name="vChest">
    /// When this method returns, the virtualized chest with the specified ID, if it exists; otherwise,
    /// null.
    /// </param>
    /// <returns>True if the virtualized chest with the specified ID exists in the dictionary; otherwise, false.</returns>
    public bool TryGetOne(string id, [NotNullWhen(true)] out VirtualizedChest? vChest)
    {
        if (this.vChests.TryGetValue(id, out vChest))
        {
            return true;
        }

        vChest = null;
        return false;
    }

    /// <summary>Remove the virtualized chest.</summary>
    /// <param name="vChest">The virtualized chest to remove.</param>
    public void Remove(VirtualizedChest vChest) => this.vChests.Remove(vChest.GlobalInventoryId);

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void InventoryMenu_rightClick_postfix(InventoryMenu __instance, Item? toAddTo, ref Item? __result, ref (Item?, int)? __state)
    {
        if (__state is null || toAddTo is not null || __result is null)
        {
            return;
        }

        var (item, slotNumber) = __state.Value;
        if (item?.QualifiedItemId != __result.QualifiedItemId)
        {
            return;
        }

        switch (__instance.actualInventory.ElementAtOrDefault(slotNumber))
        {
            case null:
                __result = item;
                return;
            case Chest
            {
                SpecialChestType: not Chest.SpecialChestTypes.JunimoChest,
            } chest:
                __result = new Chest(true, chest.ItemId)
                {
                    Name = chest.Name,
                    SpecialChestType = chest.SpecialChestType,
                    fridge = { Value = chest.fridge.Value },
                    lidFrameCount = { Value = chest.lidFrameCount.Value },
                    playerChoiceColor = { Value = chest.playerChoiceColor.Value },
                };

                __result.CopyFrom(chest);
                return;
        }
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void InventoryMenu_rightClick_prefix(InventoryMenu __instance, int x, int y, ref (Item?, int)? __state)
    {
        var slot = __instance.inventory.FirstOrDefault(slot => slot.containsPoint(x, y));
        if (slot is null)
        {
            return;
        }

        var slotNumber = int.Parse(slot.name, CultureInfo.InvariantCulture);
        var item = __instance.actualInventory.ElementAtOrDefault(slotNumber);
        if (item is not null)
        {
            __state = (item, slotNumber);
        }
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void Item_canBeDropped_postfix(Item __instance, ref bool __result)
    {
        if (__result && VirtualizedChest.TryGetId(__instance, out _))
        {
            __result = false;
        }
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void Item_canBeTrashed_postfix(Item __instance, ref bool __result)
    {
        if (__result && VirtualizedChest.TryGetId(__instance, out _))
        {
            __result = false;
        }
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void Item_canStackWith_postfix(Item __instance, ref bool __result, ISalable other)
    {
        if (__result && (VirtualizedChest.TryGetId(__instance, out _) || VirtualizedChest.TryGetId(other, out _)))
        {
            __result = false;
        }
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void Object_drawInMenu_postfix(SObject __instance, SpriteBatch spriteBatch, Vector2 location, float scaleSize, Color color)
    {
        if (!VirtualizedChestFactory.TryGetProxy(__instance, out var chest))
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
        if (!VirtualizedChestFactory.TryGetProxy(__instance, out var chest))
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
        if (VirtualizedChest.TryGetId(__instance, out _))
        {
            __result = -1;
        }
    }

    private static bool TryGetProxy(ISalable item, [NotNullWhen(true)] out Chest? chest)
    {
        if (VirtualizedChest.TryGetId(item, out var globalInventoryId) && VirtualizedChestFactory.instance.vChests.TryGetValue(globalInventoryId, out var vChest))
        {
            chest = vChest.Chest;
            return true;
        }

        chest = null;
        return false;
    }

    private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
    {
        // Clear global inventories
        this.vChests.Clear();

        var vChestData = this.data.ReadSaveData<List<VirtualizedChestData>>(VirtualizedChestFactory.VirtualizedChestDataKey);

        if (vChestData is null)
        {
            return;
        }

        // Populate virtualized chests
        foreach (var vChestItem in vChestData)
        {
            if (vChestItem.TryCreate(out var vChest))
            {
                this.vChests[vChest.GlobalInventoryId] = vChest;
            }
        }
    }

    private void OnSaving(object? sender, SavingEventArgs e)
    {
        var vChestData = new List<VirtualizedChestData>();
        foreach (var (_, vChest) in this.vChests)
        {
            vChestData.Add(vChest.GetData());
        }

        this.data.WriteSaveData(VirtualizedChestFactory.VirtualizedChestDataKey, vChestData);
    }
}
