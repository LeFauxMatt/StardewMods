namespace StardewMods.BetterChests.Framework.Features;

using System.Globalization;
using System.Reflection;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewMods.BetterChests.Framework.StorageObjects;
using StardewMods.Common.Enums;
using StardewMods.Common.Extensions;
using StardewMods.Common.Helpers;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;

/// <summary>Allows a placed chest full of items to be picked up by the farmer.</summary>
internal sealed class CarryChest : Feature
{
    private const string Id = "furyx639.BetterChests/CarryChest";
    private const string BuffId = "furyx639.BetterChests/Overburdened";

    private static readonly MethodBase InventoryMenuRightClick = AccessTools.Method(
        typeof(InventoryMenu),
        nameof(InventoryMenu.rightClick));

    private static readonly MethodBase ItemCanBeDropped = AccessTools.Method(typeof(Item), nameof(Item.canBeDropped));

    private static readonly MethodBase ItemCanBeTrashed = AccessTools.Method(typeof(Item), nameof(Item.canBeTrashed));

    private static readonly MethodBase ItemCanStackWith = AccessTools.Method(typeof(Item), nameof(Item.canStackWith));

    private static readonly MethodBase ObjectDrawInMenu = AccessTools.Method(
        typeof(SObject),
        nameof(SObject.drawInMenu),
        new[]
        {
            typeof(SpriteBatch),
            typeof(Vector2),
            typeof(float),
            typeof(float),
            typeof(float),
            typeof(StackDrawType),
            typeof(Color),
            typeof(bool),
        });

    private static readonly MethodBase ObjectDrawWhenHeld = AccessTools.Method(
        typeof(SObject),
        nameof(SObject.drawWhenHeld));

    private static readonly MethodBase ObjectPlacementAction = AccessTools.Method(
        typeof(SObject),
        nameof(SObject.placementAction));

    private static readonly MethodBase UtilityIterateChestsAndStorage = AccessTools.Method(
        typeof(Utility),
        nameof(Utility.iterateChestsAndStorage));

#nullable disable
    private static CarryChest instance;
#nullable enable

    private readonly ModConfig config;
    private readonly Harmony harmony;
    private readonly IModHelper helper;

    private CarryChest(IModHelper helper, ModConfig config)
    {
        this.helper = helper;
        this.config = config;
        this.harmony = new(CarryChest.Id);
    }

    private static ModConfig Config => CarryChest.instance.config;

    /// <summary>Checks if the player should be overburdened while carrying a chest.</summary>
    /// <param name="excludeCurrent">Whether to exclude the current item.</param>
    public static void CheckForOverburdened(bool excludeCurrent = false)
    {
        if (CarryChest.Config.CarryChestSlowAmount == 0)
        {
            Game1.player.buffs.Remove(CarryChest.BuffId);
            return;
        }

        foreach (var storage in Storages.Inventory)
        {
            if (storage is not
                {
                    Data: Storage storageObject,
                }
                || (excludeCurrent && storageObject.Context == Game1.player.CurrentItem)
                || !storageObject.Inventory.HasAny())
            {
                continue;
            }

            Game1.player.buffs.Apply(CarryChest.GetOverburdened(CarryChest.Config.CarryChestSlowAmount));
            return;
        }

        Game1.player.buffs.Remove(CarryChest.BuffId);
    }

    /// <summary>Initializes <see cref="CarryChest" />.</summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <param name="config">Mod config data.</param>
    /// <returns>Returns an instance of the <see cref="CarryChest" /> class.</returns>
    public static Feature Init(IModHelper helper, ModConfig config) => CarryChest.instance ??= new(helper, config);

    /// <inheritdoc />
    protected override void Activate()
    {
        // Events
        this.helper.Events.Input.ButtonPressed += this.OnButtonPressed;
        this.helper.Events.GameLoop.DayStarted += CarryChest.OnDayStarted;
        this.helper.Events.Player.InventoryChanged += CarryChest.OnInventoryChanged;

        // Patches
        this.harmony.Patch(
            CarryChest.InventoryMenuRightClick,
            new(typeof(CarryChest), nameof(CarryChest.InventoryMenu_rightClick_prefix)));

        this.harmony.Patch(
            CarryChest.InventoryMenuRightClick,
            postfix: new(typeof(CarryChest), nameof(CarryChest.InventoryMenu_rightClick_postfix)));

        this.harmony.Patch(
            CarryChest.ItemCanBeDropped,
            postfix: new(typeof(CarryChest), nameof(CarryChest.Item_canBeDropped_postfix)));

        this.harmony.Patch(
            CarryChest.ItemCanBeTrashed,
            postfix: new(typeof(CarryChest), nameof(CarryChest.Item_canBeTrashed_postfix)));

        this.harmony.Patch(
            CarryChest.ItemCanStackWith,
            postfix: new(typeof(CarryChest), nameof(CarryChest.Item_canStackWith_postfix)));

        this.harmony.Patch(
            CarryChest.ObjectDrawInMenu,
            postfix: new(typeof(CarryChest), nameof(CarryChest.Object_drawInMenu_postfix)));

        this.harmony.Patch(
            CarryChest.ObjectDrawWhenHeld,
            new(typeof(CarryChest), nameof(CarryChest.Object_drawWhenHeld_prefix)));

        this.harmony.Patch(
            CarryChest.ObjectPlacementAction,
            postfix: new(typeof(CarryChest), nameof(CarryChest.Object_placementAction_postfix)));

        this.harmony.Patch(
            CarryChest.UtilityIterateChestsAndStorage,
            postfix: new(typeof(CarryChest), nameof(CarryChest.Utility_iterateChestsAndStorage_postfix)));
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        // Events
        this.helper.Events.Input.ButtonPressed -= this.OnButtonPressed;
        this.helper.Events.GameLoop.DayStarted -= CarryChest.OnDayStarted;
        this.helper.Events.Player.InventoryChanged -= CarryChest.OnInventoryChanged;

        // Patches
        this.harmony.Unpatch(
            CarryChest.ObjectDrawInMenu,
            AccessTools.Method(typeof(CarryChest), nameof(CarryChest.Object_drawInMenu_postfix)));

        this.harmony.Unpatch(
            CarryChest.InventoryMenuRightClick,
            AccessTools.Method(typeof(CarryChest), nameof(CarryChest.InventoryMenu_rightClick_prefix)));

        this.harmony.Unpatch(
            CarryChest.InventoryMenuRightClick,
            AccessTools.Method(typeof(CarryChest), nameof(CarryChest.InventoryMenu_rightClick_postfix)));

        this.harmony.Unpatch(
            CarryChest.ItemCanBeDropped,
            AccessTools.Method(typeof(CarryChest), nameof(CarryChest.Item_canBeDropped_postfix)));

        this.harmony.Unpatch(
            CarryChest.ItemCanBeTrashed,
            AccessTools.Method(typeof(CarryChest), nameof(CarryChest.Item_canBeTrashed_postfix)));

        this.harmony.Unpatch(
            CarryChest.ItemCanStackWith,
            AccessTools.Method(typeof(CarryChest), nameof(CarryChest.Item_canStackWith_postfix)));

        this.harmony.Unpatch(
            CarryChest.ObjectDrawWhenHeld,
            AccessTools.Method(typeof(CarryChest), nameof(CarryChest.Object_drawWhenHeld_prefix)));

        this.harmony.Unpatch(
            CarryChest.ObjectPlacementAction,
            AccessTools.Method(typeof(CarryChest), nameof(CarryChest.Object_placementAction_postfix)));

        this.harmony.Unpatch(
            CarryChest.UtilityIterateChestsAndStorage,
            AccessTools.Method(typeof(CarryChest), nameof(CarryChest.Utility_iterateChestsAndStorage_postfix)));
    }

    private static Buff GetOverburdened(int speed) =>
        new(
            CarryChest.BuffId,
            displayName: I18n.Effect_CarryChestSlow_Description(speed.ToString(CultureInfo.InvariantCulture)),
            duration: int.MaxValue / 700,
            iconTexture: Game1.buffsIcons,
            iconSheetIndex: 13,
            effects: new() { Speed = { -speed } });

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void InventoryMenu_rightClick_postfix(
        InventoryMenu __instance,
        Item? toAddTo,
        ref Item? __result,
        ref (Item?, int)? __state)
    {
        if (__state is null || toAddTo is not null)
        {
            return;
        }

        var (item, slotNumber) = __state.Value;
        if (item is null || __result is null)
        {
            return;
        }

        if (item.ItemId != __result.ItemId)
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
    private static void InventoryMenu_rightClick_prefix(
        InventoryMenu __instance,
        int x,
        int y,
        ref (Item?, int)? __state)
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
            __state = new(item, slotNumber);
        }
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void Item_canBeDropped_postfix(Item __instance, ref bool __result)
    {
        if (!__result || __instance is not Chest chest)
        {
            return;
        }

        if (chest is not
            {
                SpecialChestType: Chest.SpecialChestTypes.JunimoChest,
            }
            && chest.GetItemsForPlayer(Game1.player.UniqueMultiplayerID).Any())
        {
            __result = false;
        }
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void Item_canBeTrashed_postfix(Item __instance, ref bool __result)
    {
        if (!__result || __instance is not Chest chest)
        {
            return;
        }

        if (chest is not
            {
                SpecialChestType: Chest.SpecialChestTypes.JunimoChest,
            }
            && chest.GetItemsForPlayer(Game1.player.UniqueMultiplayerID).Any())
        {
            __result = false;
        }
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void Item_canStackWith_postfix(Item __instance, ref bool __result, ISalable other)
    {
        if ((__instance is Chest chest && chest.Items.Any(item => item is not null))
            || (other is Chest otherChest && otherChest.Items.Any(item => item is not null)))
        {
            __result = false;
        }
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void Object_drawInMenu_postfix(
        SObject __instance,
        SpriteBatch spriteBatch,
        Vector2 location,
        float scaleSize,
        Color color)
    {
        if (__instance is not Chest chest)
        {
            return;
        }

        // Draw Items count
        var items = chest.GetItemsForPlayer(Game1.player.UniqueMultiplayerID).Count;
        if (items <= 0)
        {
            return;
        }

        var position = location
            + new Vector2(
                Game1.tileSize - Utility.getWidthOfTinyDigitString(items, 3f * scaleSize) - (3f * scaleSize),
                2f * scaleSize);

        Utility.drawTinyDigits(items, spriteBatch, position, 3f * scaleSize, 1f, color);
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static bool Object_drawWhenHeld_prefix(SObject __instance, SpriteBatch spriteBatch, Vector2 objectPosition)
    {
        if (__instance is not Chest chest)
        {
            return true;
        }

        var (x, y) = objectPosition;
        chest.draw(spriteBatch, (int)x, (int)y + Game1.tileSize, 1f, true);
        return false;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void Object_placementAction_postfix(
        SObject __instance,
        GameLocation location,
        int x,
        int y,
        ref bool __result)
    {
        if (!__result
            || __instance is not Chest held
            || !location.Objects.TryGetValue(new(x / Game1.tileSize, y / Game1.tileSize), out var obj)
            || obj is not Chest placed)
        {
            return;
        }

        // Only copy items from regular chest types
        if (held is not
            {
                SpecialChestType: Chest.SpecialChestTypes.JunimoChest,
            }
            && !placed.GetItemsForPlayer(Game1.player.UniqueMultiplayerID).Any())
        {
            placed.GetItemsForPlayer(Game1.player.UniqueMultiplayerID)
                .OverwriteWith(held.GetItemsForPlayer(Game1.player.UniqueMultiplayerID));
        }

        // Copy properties
        placed.CopyFrom(held);
        placed.Name = held.Name;
        placed.SpecialChestType = held.SpecialChestType;
        placed.fridge.Value = held.fridge.Value;
        placed.lidFrameCount.Value = held.lidFrameCount.Value;
        placed.playerChoiceColor.Value = held.playerChoiceColor.Value;

        CarryChest.CheckForOverburdened(true);
    }

    private static void OnDayStarted(object? sender, DayStartedEventArgs e) => CarryChest.CheckForOverburdened();

    private static void OnInventoryChanged(object? sender, InventoryChangedEventArgs e) =>
        CarryChest.CheckForOverburdened();

    private static void RecursiveIterate(Farmer player, Chest chest, Action<Item> action, ISet<Chest> exclude)
    {
        var items = chest.GetItemsForPlayer(player.UniqueMultiplayerID);
        if (exclude.Contains(chest) || chest.SpecialChestType is Chest.SpecialChestTypes.JunimoChest)
        {
            return;
        }

        exclude.Add(chest);
        foreach (var item in items)
        {
            if (item is Chest otherChest)
            {
                CarryChest.RecursiveIterate(player, otherChest, action, exclude);
            }

            if (item is not null)
            {
                action(item);
            }
        }
    }

    private static void Utility_iterateChestsAndStorage_postfix(Action<Item> action)
    {
        Log.Verbose("Recursively iterating chests in farmer inventory.");
        foreach (var farmer in Game1.getAllFarmers())
        {
            foreach (var chest in farmer.Items.OfType<Chest>())
            {
                CarryChest.RecursiveIterate(farmer, chest, action, new HashSet<Chest>());
            }
        }
    }

    private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        if (!Context.IsPlayerFree
            || Game1.player.CurrentItem is Tool
            || !e.Button.IsUseToolButton()
            || this.helper.Input.IsSuppressed(e.Button)
            || (Game1.player.currentLocation is MineShaft mineShaft
                && mineShaft.Name.StartsWith("UndergroundMine", StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        var pos = CommonHelpers.GetCursorTile(1, false);
        if (!Utility.tileWithinRadiusOfPlayer((int)pos.X, (int)pos.Y, 1, Game1.player)
            || !Game1.currentLocation.Objects.TryGetValue(pos, out var obj)
            || !Storages.TryGetOne(obj, out var storage)
            || storage.CarryChest is not FeatureOption.Enabled)
        {
            return;
        }

        // Already carrying the limit
        var limit = this.config.CarryChestLimit;
        if (limit > 0)
        {
            foreach (var item in Game1.player.Items.OfType<Chest>())
            {
                if (item.GetItemsForPlayer(Game1.player.UniqueMultiplayerID).Any())
                {
                    --limit;
                }

                if (limit > 0)
                {
                    continue;
                }

                Game1.showRedMessage(I18n.Alert_CarryChestLimit_HitLimit());
                this.helper.Input.Suppress(e.Button);
                return;
            }
        }

        // Cannot add to inventory
        if (!Game1.player.addItemToInventoryBool(obj, true))
        {
            return;
        }

        Game1.playSound("pickUpItem");
        Game1.currentLocation.Objects.Remove(pos);
        this.helper.Input.Suppress(e.Button);
        CarryChest.CheckForOverburdened();
    }
}
