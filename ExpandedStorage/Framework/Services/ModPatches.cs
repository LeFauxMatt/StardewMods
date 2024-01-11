namespace StardewMods.ExpandedStorage.Framework.Services;

using System.Reflection.Emit;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.Common.Enums;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Models;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.FuryCore;
using StardewMods.ExpandedStorage.Framework.Models;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;

/// <summary>Harmony Patches for Expanded Storage.</summary>
internal sealed class ModPatches : BaseService
{
    private const string ChestOpenSound = "openChest";
    private const string LidOpenSound = "doorCreak";
    private const string LidCloseSound = "doorCreakReverse";
    private const string MiniShippingBinOpenSound = "shwip";

#nullable disable
    private static ModPatches instance;
#nullable enable

    private readonly IEventPublisher eventPublisher;
    private readonly StorageManager storageManager;

    /// <summary>Initializes a new instance of the <see cref="ModPatches" /> class.</summary>
    /// <param name="eventPublisher">Dependency used for publishing events.</param>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="patchManager">Dependency used for managing patches.</param>
    /// <param name="storageManager">Dependency used to handle the objects which should be managed by Expanded Storages.</param>
    public ModPatches(
        IEventPublisher eventPublisher,
        ILog log,
        IManifest manifest,
        IPatchManager patchManager,
        StorageManager storageManager)
        : base(log, manifest)
    {
        // Init
        ModPatches.instance = this;
        this.eventPublisher = eventPublisher;
        this.storageManager = storageManager;

        // Patches
        patchManager.Add(
            this.ModId,
            new SavedPatch(
                AccessTools.DeclaredMethod(typeof(Chest), nameof(Chest.checkForAction)),
                AccessTools.DeclaredMethod(typeof(ModPatches), nameof(ModPatches.Chest_checkForAction_transpiler)),
                PatchType.Transpiler),
            new SavedPatch(
                AccessTools.DeclaredMethod(
                    typeof(Chest),
                    nameof(Chest.draw),
                    [typeof(SpriteBatch), typeof(int), typeof(int), typeof(float)]),
                AccessTools.DeclaredMethod(typeof(ModPatches), nameof(ModPatches.Chest_draw_prefix)),
                PatchType.Prefix),
            new SavedPatch(
                AccessTools.DeclaredMethod(
                    typeof(Chest),
                    nameof(Chest.draw),
                    [typeof(SpriteBatch), typeof(int), typeof(int), typeof(float), typeof(bool)]),
                AccessTools.DeclaredMethod(typeof(ModPatches), nameof(ModPatches.Chest_drawLocal_prefix)),
                PatchType.Prefix),
            new SavedPatch(
                AccessTools.DeclaredMethod(typeof(Chest), nameof(Chest.getLastLidFrame)),
                AccessTools.DeclaredMethod(typeof(ModPatches), nameof(ModPatches.Chest_getLastLidFrame_postfix)),
                PatchType.Postfix),
            new SavedPatch(
                AccessTools.DeclaredMethod(typeof(Chest), nameof(Chest.OpenMiniShippingMenu)),
                AccessTools.DeclaredMethod(
                    typeof(ModPatches),
                    nameof(ModPatches.Chest_OpenMiniShippingMenu_transpiler)),
                PatchType.Transpiler),
            new SavedPatch(
                AccessTools.DeclaredMethod(typeof(Chest), nameof(Chest.ShowMenu)),
                AccessTools.DeclaredMethod(typeof(ModPatches), nameof(ModPatches.Chest_SpecialChestType_transpiler)),
                PatchType.Transpiler),
            new SavedPatch(
                AccessTools.DeclaredMethod(typeof(Chest), nameof(Chest.UpdateFarmerNearby)),
                AccessTools.DeclaredMethod(typeof(ModPatches), nameof(ModPatches.Chest_UpdateFarmerNearby_transpiler)),
                PatchType.Transpiler),
            new SavedPatch(
                AccessTools
                    .GetDeclaredConstructors(typeof(ItemGrabMenu))
                    .First(ctor => ctor.GetParameters().Length >= 10),
                AccessTools.DeclaredMethod(typeof(ModPatches), nameof(ModPatches.ItemGrabMenu_constructor_postfix)),
                PatchType.Postfix),
            new SavedPatch(
                AccessTools
                    .GetDeclaredConstructors(typeof(ItemGrabMenu))
                    .First(ctor => ctor.GetParameters().Length >= 10),
                AccessTools.DeclaredMethod(
                    typeof(ModPatches),
                    nameof(ModPatches.ItemGrabMenu_SpecialChestType_transpiler)),
                PatchType.Transpiler),
            new SavedPatch(
                AccessTools.DeclaredMethod(typeof(ItemGrabMenu), nameof(ItemGrabMenu.draw)),
                AccessTools.DeclaredMethod(
                    typeof(ModPatches),
                    nameof(ModPatches.ItemGrabMenu_SpecialChestType_transpiler)),
                PatchType.Transpiler),
            new SavedPatch(
                AccessTools.DeclaredMethod(typeof(ItemGrabMenu), nameof(ItemGrabMenu.gameWindowSizeChanged)),
                AccessTools.DeclaredMethod(
                    typeof(ModPatches),
                    nameof(ModPatches.ItemGrabMenu_gameWindowSizeChanged_postfix)),
                PatchType.Postfix),
            new SavedPatch(
                AccessTools.DeclaredMethod(typeof(ItemGrabMenu), nameof(ItemGrabMenu.gameWindowSizeChanged)),
                AccessTools.DeclaredMethod(
                    typeof(ModPatches),
                    nameof(ModPatches.ItemGrabMenu_SpecialChestType_transpiler)),
                PatchType.Transpiler),
            new SavedPatch(
                AccessTools.DeclaredMethod(typeof(ItemGrabMenu), nameof(ItemGrabMenu.setSourceItem)),
                AccessTools.DeclaredMethod(typeof(ModPatches), nameof(ModPatches.ItemGrabMenu_setSourceItem_postfix)),
                PatchType.Postfix),
            new SavedPatch(
                AccessTools.DeclaredMethod(typeof(ItemGrabMenu), nameof(ItemGrabMenu.setSourceItem)),
                AccessTools.DeclaredMethod(
                    typeof(ModPatches),
                    nameof(ModPatches.ItemGrabMenu_SpecialChestType_transpiler)),
                PatchType.Transpiler),
            new SavedPatch(
                AccessTools.DeclaredMethod(typeof(ItemGrabMenu), nameof(ItemGrabMenu.snapToDefaultClickableComponent)),
                AccessTools.DeclaredMethod(
                    typeof(ModPatches),
                    nameof(ModPatches.ItemGrabMenu_SpecialChestType_transpiler)),
                PatchType.Transpiler),
            new SavedPatch(
                AccessTools.DeclaredMethod(typeof(SObject), nameof(SObject.placementAction)),
                AccessTools.DeclaredMethod(typeof(ModPatches), nameof(ModPatches.Object_placementAction_postfix)),
                PatchType.Postfix));

        patchManager.Patch(this.ModId);
    }

    private static IEnumerable<CodeInstruction> Chest_checkForAction_transpiler(
        IEnumerable<CodeInstruction> instructions)
    {
        foreach (var instruction in instructions)
        {
            if (instruction.LoadsConstant(ModPatches.ChestOpenSound))
            {
                yield return new CodeInstruction(OpCodes.Ldarg_0);
                yield return instruction;
                yield return CodeInstruction.Call(typeof(ModPatches), nameof(ModPatches.GetSound));
            }
            else if (instruction.LoadsConstant(ModPatches.LidOpenSound))
            {
                yield return new CodeInstruction(OpCodes.Ldarg_0);
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
            || !ModPatches.instance.storageManager.TryGetData(__instance, out var storage))
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
                new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y),
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
            Math.Min(lastLidFrame - startingLidFrame + 1, Math.Max(0, ___currentLidFrame - startingLidFrame)) * 16,
            colored ? 32 : 0,
            16,
            32);

        // Draw Base Layer
        spriteBatch.Draw(
            texture,
            pos + (__instance.shakeTimer > 0 ? new Vector2(Game1.random.Next(-1, 2), 0) : Vector2.Zero),
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
            pos + (__instance.shakeTimer > 0 ? new Vector2(Game1.random.Next(-1, 2), 0) : Vector2.Zero),
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
            || !ModPatches.instance.storageManager.TryGetData(__instance, out var storage))
        {
            return true;
        }

        var colored = storage.PlayerColor && !__instance.playerChoiceColor.Value.Equals(Color.Black);
        var color = colored ? __instance.playerChoiceColor.Value : __instance.Tint;

        var data = ItemRegistry.GetDataOrErrorItem(__instance.QualifiedItemId);
        var texture = data.GetTexture();
        var pos = local
            ? new Vector2(x, y - 64)
            : Game1.GlobalToLocal(Game1.viewport, new Vector2(x, y - 1) * Game1.tileSize);

        var frame = new Rectangle(0, colored ? 32 : 0, 16, 32);
        var baseSortOrder = local ? 0.89f : ((y * 64) + 4) / 10000f;

        // Draw Base Layer
        spriteBatch.Draw(
            texture,
            pos + (__instance.shakeTimer > 0 ? new Vector2(Game1.random.Next(-1, 2), 0) : Vector2.Zero),
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
            pos + (__instance.shakeTimer > 0 ? new Vector2(Game1.random.Next(-1, 2), 0) : Vector2.Zero),
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
            || !ModPatches.instance.storageManager.TryGetData(__instance, out var storage))
        {
            return;
        }

        __result = __instance.startingLidFrame.Value + storage.Frames - 1;
    }

    private static IEnumerable<CodeInstruction> Chest_OpenMiniShippingMenu_transpiler(
        IEnumerable<CodeInstruction> instructions)
    {
        foreach (var instruction in instructions)
        {
            if (instruction.LoadsConstant(ModPatches.MiniShippingBinOpenSound))
            {
                yield return new CodeInstruction(OpCodes.Ldarg_0);
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
    private static IEnumerable<CodeInstruction> Chest_UpdateFarmerNearby_transpiler(
        IEnumerable<CodeInstruction> instructions)
    {
        foreach (var instruction in instructions)
        {
            if (instruction.LoadsConstant(ModPatches.LidOpenSound))
            {
                yield return new CodeInstruction(OpCodes.Ldarg_0);
                yield return instruction;
                yield return CodeInstruction.Call(typeof(ModPatches), nameof(ModPatches.GetSound));
            }
            else if (instruction.LoadsConstant(ModPatches.LidCloseSound))
            {
                yield return new CodeInstruction(OpCodes.Ldarg_0);
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
        if (!ModPatches.instance.storageManager.TryGetData(chest, out var storage))
        {
            return sound;
        }

        return sound switch
        {
            ModPatches.ChestOpenSound or ModPatches.MiniShippingBinOpenSound => storage.OpenSound,
            ModPatches.LidOpenSound => storage.OpenNearbySound,
            ModPatches.LidCloseSound => storage.CloseNearbySound,
            _ => sound,
        };
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static void ItemGrabMenu_constructor_postfix(ItemGrabMenu __instance, ref Item ___sourceItem) =>
        ModPatches.UpdateColorPicker(__instance, ___sourceItem);

    private static IEnumerable<CodeInstruction> ItemGrabMenu_SpecialChestType_transpiler(
        IEnumerable<CodeInstruction> instructions)
    {
        foreach (var instruction in instructions)
        {
            if (instruction.Calls(AccessTools.PropertyGetter(typeof(Chest), nameof(Chest.SpecialChestType))))
            {
                yield return instruction;
                yield return new CodeInstruction(OpCodes.Ldarg_0);
                yield return CodeInstruction.LoadField(typeof(ItemGrabMenu), nameof(ItemGrabMenu.sourceItem));
                yield return CodeInstruction.Call(typeof(ModPatches), nameof(ModPatches.GetSpecialChestType));
            }
            else
            {
                yield return instruction;
            }
        }
    }

    private static IEnumerable<CodeInstruction> Chest_SpecialChestType_transpiler(
        IEnumerable<CodeInstruction> instructions)
    {
        foreach (var instruction in instructions)
        {
            if (instruction.Calls(AccessTools.PropertyGetter(typeof(Chest), nameof(Chest.SpecialChestType))))
            {
                yield return instruction;
                yield return new CodeInstruction(OpCodes.Ldarg_0);
                yield return CodeInstruction.Call(typeof(ModPatches), nameof(ModPatches.GetSpecialChestType));
            }
            else
            {
                yield return instruction;
            }
        }
    }

    private static Chest.SpecialChestTypes GetSpecialChestType(
        Chest.SpecialChestTypes specialChestType,
        Item sourceItem)
    {
        if (!ModPatches.instance.storageManager.TryGetData(sourceItem, out var storage) || !storage.OpenNearby)
        {
            return specialChestType;
        }

        return Chest.SpecialChestTypes.None;
    }

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
    [HarmonyBefore("furyx639.BetterChests")]
    private static void Object_placementAction_postfix(
        SObject __instance,
        ref bool __result,
        GameLocation location,
        int x,
        int y)
    {
        if (!__result || !ModPatches.instance.storageManager.TryGetData(__instance, out var storage))
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

        var chest = new Chest(true, tile, __instance.ItemId)
        {
            shakeTimer = 50,
            SpecialChestType =
                storage.OpenNearby ? Chest.SpecialChestTypes.MiniShippingBin : Chest.SpecialChestTypes.None,
        };

        location.Objects[tile] = chest;
        location.playSound(storage.PlaceSound);
        __result = true;
        ModPatches.instance.eventPublisher.Publish(new ChestCreatedEventArgs(chest, location, tile, storage));
    }

    private static void UpdateColorPicker(ItemGrabMenu itemGrabMenu, Item sourceItem)
    {
        if (sourceItem is not Chest chest || !ModPatches.instance.storageManager.TryGetData(chest, out var storage))
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