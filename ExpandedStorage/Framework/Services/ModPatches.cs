namespace StardewMods.ExpandedStorage.Framework.Services;

using System.Reflection.Emit;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;

/// <summary>Harmony Patches for Expanded Storage.</summary>
internal sealed class ModPatches
{
    private const string ChestOpenSound = "openChest";
    private const string LidOpenSound = "doorCreak";
    private const string LidCloseSound = "doorCreakReverse";

#nullable disable
    private static ModPatches instance;
#nullable enable

    private readonly ManagedStorages storages;

    /// <summary>Initializes a new instance of the <see cref="ModPatches" /> class.</summary>
    /// <param name="harmony">Dependency used to patch the base game.</param>
    /// <param name="storages">Dependency used to handle the objects which should be managed by Expanded Storages.</param>
    public ModPatches(Harmony harmony, ManagedStorages storages)
    {
        // Init
        ModPatches.instance = this;
        this.storages = storages;

        harmony.Patch(
            AccessTools.Method(typeof(Chest), nameof(Chest.checkForAction)),
            transpiler: new(typeof(ModPatches), nameof(ModPatches.Chest_checkForAction_transpiler)));

        harmony.Patch(
            AccessTools.Method(
                typeof(Chest),
                nameof(Chest.draw),
                new[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(float) }),
            new(typeof(ModPatches), nameof(ModPatches.Chest_draw_prefix)));

        harmony.Patch(
            AccessTools.Method(
                typeof(Chest),
                nameof(Chest.draw),
                new[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(float), typeof(bool) }),
            new(typeof(ModPatches), nameof(ModPatches.Chest_drawLocal_prefix)));

        harmony.Patch(
            AccessTools.Method(typeof(Chest), nameof(Chest.getLastLidFrame)),
            postfix: new(typeof(ModPatches), nameof(ModPatches.Chest_getLastLidFrame_postfix)));

        // World
        harmony.Patch(
            AccessTools.Method(typeof(Chest), nameof(Chest.checkForAction)),
            new(typeof(ModPatches), nameof(ModPatches.Chest_chestForAction_prefix)));

        harmony.Patch(
            AccessTools.Method(typeof(Chest), nameof(Chest.UpdateFarmerNearby)),
            transpiler: new(typeof(ModPatches), nameof(ModPatches.Chest_UpdateFarmerNearby_transpiler)));

        harmony.Patch(
            AccessTools.Method(typeof(Chest), nameof(Chest.updateWhenCurrentLocation)),
            postfix: new(typeof(ModPatches), nameof(ModPatches.Chest_updateWhenCurrentLocation_postfix)));

        harmony.Patch(
            AccessTools.Constructor(
                typeof(ItemGrabMenu),
                new[]
                {
                    typeof(IList<Item>),
                    typeof(bool),
                    typeof(bool),
                    typeof(InventoryMenu.highlightThisItem),
                    typeof(ItemGrabMenu.behaviorOnItemSelect),
                    typeof(string),
                    typeof(ItemGrabMenu.behaviorOnItemSelect),
                    typeof(bool),
                    typeof(bool),
                    typeof(bool),
                    typeof(bool),
                    typeof(bool),
                    typeof(int),
                    typeof(Item),
                    typeof(int),
                    typeof(object),
                    typeof(ItemExitBehavior),
                    typeof(bool),
                }),
            postfix: new(typeof(ModPatches), nameof(ModPatches.ItemGrabMenu_constructor_postfix)));

        harmony.Patch(
            AccessTools.Method(typeof(ItemGrabMenu), nameof(ItemGrabMenu.gameWindowSizeChanged)),
            postfix: new(typeof(ModPatches), nameof(ModPatches.ItemGrabMenu_gameWindowSizeChanged_postfix)));

        harmony.Patch(
            AccessTools.Method(typeof(ItemGrabMenu), nameof(ItemGrabMenu.setSourceItem)),
            postfix: new(typeof(ModPatches), nameof(ModPatches.ItemGrabMenu_setSourceItem_postfix)));

        harmony.Patch(
            AccessTools.Method(typeof(SObject), nameof(SObject.placementAction)),
            postfix: new(typeof(ModPatches), nameof(ModPatches.Object_placementAction_postfix)));
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static bool Chest_chestForAction_prefix(Chest __instance, ref bool __result, bool justCheckingForActivity)
    {
        if (justCheckingForActivity
            || !__instance.playerChest.Value
            || !ModPatches.instance.storages.Data.TryGetValue(__instance.ItemId, out var storage))
        {
            return true;
        }

        if (!Game1.didPlayerJustRightClick(true))
        {
            __result = false;
            return false;
        }

        __instance.GetMutex()
            .RequestLock(
                () =>
                {
                    if (storage.OpenNearby)
                    {
                        Game1.playSound(storage.OpenSound);
                        __instance.ShowMenu();
                    }
                    else
                    {
                        __instance.frameCounter.Value = 5;
                        Game1.playSound(storage.OpenSound);
                        Game1.player.Halt();
                        Game1.player.freezePause = 1000;
                    }
                });

        __result = true;
        return false;
    }

    private static IEnumerable<CodeInstruction> Chest_checkForAction_transpiler(
        IEnumerable<CodeInstruction> instructions)
    {
        foreach (var instruction in instructions)
        {
            if (instruction.LoadsConstant(ModPatches.ChestOpenSound))
            {
                yield return new(OpCodes.Ldarg_0);
                yield return instruction;
                yield return CodeInstruction.Call(typeof(ModPatches), nameof(ModPatches.GetSound));
            }
            else if (instruction.LoadsConstant(ModPatches.LidOpenSound))
            {
                yield return new(OpCodes.Ldarg_0);
                yield return instruction;
                yield return CodeInstruction.Call(typeof(ModPatches), nameof(ModPatches.GetSound));
            }
            else
            {
                yield return instruction;
            }
        }
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static bool Chest_draw_prefix(
        Chest __instance,
        ref int ___currentLidFrame,
        SpriteBatch spriteBatch,
        int x,
        int y,
        float alpha)
    {
        if (!__instance.playerChest.Value
            || !ModPatches.instance.storages.Data.TryGetValue(__instance.ItemId, out var storage))
        {
            return true;
        }

        var drawX = (float)x;
        var drawY = (float)y;
        if (__instance.localKickStartTile.HasValue)
        {
            drawX = Utility.Lerp(__instance.localKickStartTile.Value.X, drawX, __instance.kickProgress);
            drawY = Utility.Lerp(__instance.localKickStartTile.Value.Y, drawY, __instance.kickProgress);
        }

        var baseSortOrder = Math.Max(0f, (((drawY + 1f) * 64f) - 24f) / 10000f) + (drawX * 1E-05f);
        if (__instance.localKickStartTile.HasValue)
        {
            spriteBatch.Draw(
                Game1.shadowTexture,
                Game1.GlobalToLocal(Game1.viewport, new Vector2((drawX + 0.5f) * 64f, (drawY + 0.5f) * 64f)),
                Game1.shadowTexture.Bounds,
                Color.Black * 0.5f,
                0f,
                new(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y),
                4f,
                SpriteEffects.None,
                0.0001f);

            drawY -= (float)Math.Sin(__instance.kickProgress * Math.PI) * 0.5f;
        }

        var colored = storage.PlayerColor && !__instance.playerChoiceColor.Value.Equals(Color.Black);
        var color = colored ? __instance.playerChoiceColor.Value : __instance.Tint;

        var data = ItemRegistry.GetDataOrErrorItem(__instance.QualifiedItemId);
        var texture = data.GetTexture();
        var pos = Game1.GlobalToLocal(Game1.viewport, new Vector2(drawX, drawY - 1f) * Game1.tileSize);
        var startingLidFrame = __instance.startingLidFrame.Value;
        var lastLidFrame = __instance.getLastLidFrame();
        var frame = new Rectangle(
            Math.Min((lastLidFrame - startingLidFrame) + 1, Math.Max(0, ___currentLidFrame - startingLidFrame)) * 16,
            colored ? 32 : 0,
            16,
            32);

        // Draw Base Layer
        spriteBatch.Draw(
            texture,
            pos + (__instance.shakeTimer > 0 ? new(Game1.random.Next(-1, 2), 0) : Vector2.Zero),
            frame,
            color * alpha,
            0f,
            Vector2.Zero,
            4f,
            SpriteEffects.None,
            baseSortOrder);

        if (frame.Y == 0)
        {
            return false;
        }

        // Draw Top Layer
        frame.Y = 64;
        spriteBatch.Draw(
            texture,
            pos + (__instance.shakeTimer > 0 ? new(Game1.random.Next(-1, 2), 0) : Vector2.Zero),
            frame,
            __instance.Tint * alpha,
            0f,
            Vector2.Zero,
            4f,
            SpriteEffects.None,
            baseSortOrder + 1E-05f);

        return false;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static bool Chest_drawLocal_prefix(
        Chest __instance,
        SpriteBatch spriteBatch,
        int x,
        int y,
        float alpha,
        bool local)
    {
        if (!__instance.playerChest.Value
            || !ModPatches.instance.storages.Data.TryGetValue(__instance.ItemId, out var storage))
        {
            return true;
        }

        var colored = storage.PlayerColor && !__instance.playerChoiceColor.Value.Equals(Color.Black);
        var color = colored ? __instance.playerChoiceColor.Value : __instance.Tint;

        var data = ItemRegistry.GetDataOrErrorItem(__instance.QualifiedItemId);
        var texture = data.GetTexture();
        var pos = local ? new(x, y - 64) : Game1.GlobalToLocal(Game1.viewport, new Vector2(x, y - 1) * Game1.tileSize);
        var frame = new Rectangle(0, colored ? 32 : 0, 16, 32);
        var baseSortOrder = local ? 0.89f : ((y * 64) + 4) / 10000f;

        // Draw Base Layer
        spriteBatch.Draw(
            texture,
            pos + (__instance.shakeTimer > 0 ? new(Game1.random.Next(-1, 2), 0) : Vector2.Zero),
            frame,
            color * alpha,
            0f,
            Vector2.Zero,
            4f,
            SpriteEffects.None,
            baseSortOrder);

        if (frame.Y == 0)
        {
            return false;
        }

        // Draw Top Layer
        frame.Y = 64;
        spriteBatch.Draw(
            texture,
            pos + (__instance.shakeTimer > 0 ? new(Game1.random.Next(-1, 2), 0) : Vector2.Zero),
            frame,
            __instance.Tint * alpha,
            0f,
            Vector2.Zero,
            4f,
            SpriteEffects.None,
            baseSortOrder + 1E-05f);

        return false;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void Chest_getLastLidFrame_postfix(Chest __instance, ref int __result)
    {
        if (!__instance.playerChest.Value
            || !ModPatches.instance.storages.Data.TryGetValue(__instance.ItemId, out var storage))
        {
            return;
        }

        __result = (__instance.startingLidFrame.Value + storage.Frames) - 1;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static IEnumerable<CodeInstruction> Chest_UpdateFarmerNearby_transpiler(
        IEnumerable<CodeInstruction> instructions)
    {
        foreach (var instruction in instructions)
        {
            if (instruction.LoadsConstant(ModPatches.LidOpenSound))
            {
                yield return new(OpCodes.Ldarg_0);
                yield return instruction;
                yield return CodeInstruction.Call(typeof(ModPatches), nameof(ModPatches.GetSound));
            }
            else if (instruction.LoadsConstant(ModPatches.LidCloseSound))
            {
                yield return new(OpCodes.Ldarg_0);
                yield return instruction;
                yield return CodeInstruction.Call(typeof(ModPatches), nameof(ModPatches.GetSound));
            }
            else
            {
                yield return instruction;
            }
        }
    }

    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Harmony")]
    private static string GetSound(Chest chest, string sound)
    {
        if (!ModPatches.instance.storages.Data.TryGetValue(chest.ItemId, out var storage))
        {
            return sound;
        }

        return sound switch
        {
            ModPatches.ChestOpenSound => storage.OpenSound,
            ModPatches.LidOpenSound => storage.OpenNearbySound,
            ModPatches.LidCloseSound => storage.CloseNearbySound,
            _ => sound,
        };
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void Chest_updateWhenCurrentLocation_postfix(
        Chest __instance,
        ref int ____shippingBinFrameCounter,
        ref bool ____farmerNearby,
        ref int ___currentLidFrame)
    {
        if (!__instance.playerChest.Value
            || __instance.Location is null
            || !ModPatches.instance.storages.Data.TryGetValue(__instance.ItemId, out var storage)
            || !storage.OpenNearby)
        {
            return;
        }

        __instance.UpdateFarmerNearby();
        if (____shippingBinFrameCounter > -1)
        {
            ____shippingBinFrameCounter--;
            if (____shippingBinFrameCounter <= 0)
            {
                ____shippingBinFrameCounter = 5;
                if (____farmerNearby && ___currentLidFrame < __instance.getLastLidFrame())
                {
                    ___currentLidFrame++;
                }
                else if (!____farmerNearby && ___currentLidFrame > __instance.startingLidFrame.Value)
                {
                    ___currentLidFrame--;
                }
                else
                {
                    ____shippingBinFrameCounter = -1;
                }
            }
        }

        if (Game1.activeClickableMenu == null && __instance.GetMutex().IsLockHeld())
        {
            __instance.GetMutex().ReleaseLock();
        }
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void ItemGrabMenu_constructor_postfix(ItemGrabMenu __instance, ref Item ___sourceItem) =>
        ModPatches.UpdateColorPicker(__instance, ___sourceItem);

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void ItemGrabMenu_gameWindowSizeChanged_postfix(ItemGrabMenu __instance, ref Item ___sourceItem) =>
        ModPatches.UpdateColorPicker(__instance, ___sourceItem);

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void ItemGrabMenu_setSourceItem_postfix(ItemGrabMenu __instance, ref Item ___sourceItem) =>
        ModPatches.UpdateColorPicker(__instance, ___sourceItem);

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void Object_placementAction_postfix(
        SObject __instance,
        ref bool __result,
        GameLocation location,
        int x,
        int y)
    {
        if (!__result || !ModPatches.instance.storages.Data.TryGetValue(__instance.ItemId, out var storage))
        {
            return;
        }

        var tile = new Vector2((int)(x / (float)Game1.tileSize), (int)(y / (float)Game1.tileSize));
        if (location is MineShaft or VolcanoDungeon)
        {
            location.Objects[tile] = null;
            Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.13053"));
            __result = false;
            return;
        }

        location.Objects[tile] = new Chest(true, tile, __instance.ItemId)
        {
            shakeTimer = 50,
        };

        __result = true;
        location.playSound(storage.PlaceSound);
    }

    private static void UpdateColorPicker(ItemGrabMenu itemGrabMenu, Item sourceItem)
    {
        if (sourceItem is not Chest chest
            || !ModPatches.instance.storages.Data.TryGetValue(chest.ItemId, out var storage))
        {
            return;
        }

        if (storage.PlayerColor || itemGrabMenu.chestColorPicker is not null)
        {
            return;
        }

        itemGrabMenu.chestColorPicker = null;
        itemGrabMenu.colorPickerToggleButton = null;
        itemGrabMenu.discreteColorPickerCC = null;
        itemGrabMenu.RepositionSideButtons();
    }
}
