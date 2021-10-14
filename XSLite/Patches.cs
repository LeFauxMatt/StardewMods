namespace XSLite
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection.Emit;
    using Common.Helpers;
    using CommonHarmony;
    using HarmonyLib;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Netcode;
    using StardewValley;
    using StardewValley.Locations;
    using StardewValley.Menus;
    using StardewValley.Network;
    using StardewValley.Objects;
    using StardewValley.Tools;
    using SObject = StardewValley.Object;

    /// <summary>
    ///     Encompasses all patches required by this mod.
    /// </summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Naming is determined by Harmony.")]
    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Type is determined by Harmony.")]
    internal class Patches
    {
        /// <summary>Initializes a new instance of the <see cref="Patches" /> class.</summary>
        public Patches(Harmony harmony)
        {
            // Use GetItemsForPlayer for all chest types.
            harmony.Patch(
                AccessTools.Method(typeof(Chest), nameof(Chest.addItem)),
                transpiler: new(typeof(Patches), nameof(Patches.Chest_addItem_transpiler)));

            // Clear nulls for heldStorage items
            harmony.Patch(
                AccessTools.Method(typeof(Chest), nameof(Chest.clearNulls)),
                postfix: new(typeof(Patches), nameof(Patches.Chest_clearNulls_postfix)));

            // Draw bigger storages from the origin chest.
            harmony.Patch(
                AccessTools.Method(
                    typeof(Chest),
                    nameof(Chest.draw),
                    new[]
                    {
                        typeof(SpriteBatch), typeof(int), typeof(int), typeof(float),
                    }),
                new(typeof(Patches), nameof(Patches.Chest_draw_prefix)));

            // Draw chest with playerChoiceColor and animation when held.
            harmony.Patch(
                AccessTools.Method(
                    typeof(Chest),
                    nameof(Chest.draw),
                    new[]
                    {
                        typeof(SpriteBatch), typeof(int), typeof(int), typeof(float), typeof(bool),
                    }),
                new(typeof(Patches), nameof(Patches.Chest_drawLocal_prefix)));

            // Draw chest with playerChoiceColor and animation in menu.
            harmony.Patch(
                AccessTools.Method(
                    typeof(Chest),
                    nameof(Chest.drawInMenu),
                    new[]
                    {
                        typeof(SpriteBatch), typeof(Vector2), typeof(float), typeof(float), typeof(float), typeof(StackDrawType), typeof(Color), typeof(bool),
                    }),
                new(typeof(Patches), nameof(Patches.Chest_drawInMenu_prefix)));

            // Return items from heldItem Chest.
            harmony.Patch(
                AccessTools.Method(typeof(Chest), nameof(Chest.GetItemsForPlayer)),
                postfix: new(typeof(Patches), nameof(Patches.Chest_GetItemsForPlayer_postfix)));

            // Create expanded storage debris.
            harmony.Patch(
                AccessTools.Method(typeof(Chest), nameof(Chest.performToolAction)),
                transpiler: new(typeof(Patches), nameof(Patches.Chest_performToolAction_transpiler)));

            // Disallow stacking Chests holding items.
            harmony.Patch(
                AccessTools.Method(typeof(Item), nameof(Item.canStackWith)),
                postfix: new(typeof(Patches), nameof(Patches.Item_canStackWith_postfix)));

            // Remove disabled components
            harmony.Patch(
                AccessTools.Constructor(
                    typeof(ItemGrabMenu),
                    new[]
                    {
                        typeof(IList<Item>), typeof(bool), typeof(bool), typeof(InventoryMenu.highlightThisItem), typeof(ItemGrabMenu.behaviorOnItemSelect), typeof(string), typeof(ItemGrabMenu.behaviorOnItemSelect), typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(int), typeof(Item), typeof(int), typeof(object),
                    }),
                postfix: new(typeof(Patches), nameof(Patches.ItemGrabMenu_constructor_postfix)));

            // Remove disabled components
            harmony.Patch(
                AccessTools.Method(typeof(ItemGrabMenu), nameof(ItemGrabMenu.setSourceItem)),
                postfix: new(typeof(Patches), nameof(Patches.ItemGrabMenu_setSourceItem_postfix)));

            // Draw carried chests
            harmony.Patch(
                AccessTools.Method(typeof(SObject), nameof(SObject.drawWhenHeld)),
                new(typeof(Patches), nameof(Patches.Object_drawWhenHeld_prefix)));

            // Return custom description for Object.
            harmony.Patch(
                AccessTools.Method(typeof(SObject), nameof(SObject.getDescription)),
                postfix: new(typeof(Patches), nameof(Patches.Object_getDescription_postfix)));

            harmony.Patch(
                AccessTools.Method(typeof(SObject), nameof(SObject.getOne)),
                postfix: new(typeof(Patches), nameof(Patches.Object_getOne_postfix)));

            // Return custom display name for Object.
            harmony.Patch(
                AccessTools.Method(typeof(SObject), "loadDisplayName"),
                postfix: new(typeof(Patches), nameof(Patches.Object_loadDisplayName_postfix)));

            // Disallow invalid chest placement locations.
            harmony.Patch(
                AccessTools.Method(typeof(SObject), nameof(SObject.placementAction)),
                new(typeof(Patches), nameof(Patches.Object_placementAction_prefix)));
        }

        private static IEnumerable<CodeInstruction> Chest_addItem_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            Log.Trace("Using getItemsForPlayer for all items.");
            var getItemsPatch = new PatternPatch();
            getItemsPatch
                .Find(
                    new[]
                    {
                        new CodeInstruction(OpCodes.Call, AccessTools.Property(typeof(Chest), nameof(Chest.SpecialChestType)).GetGetMethod()), new CodeInstruction(OpCodes.Ldc_I4_2), new CodeInstruction(OpCodes.Bne_Un_S),
                    })
                .Patch(
                    list =>
                    {
                        list.RemoveLast();
                        list.RemoveLast();
                        list.RemoveLast();
                        list.RemoveLast();
                        list.RemoveLast();
                        list.RemoveLast();
                        list.RemoveLast();
                        list.RemoveLast();
                        list.RemoveLast();
                        list.RemoveLast();
                        list.RemoveLast();
                    });

            var patternPatches = new PatternPatches(instructions, getItemsPatch);
            foreach (var patternPatch in patternPatches)
            {
                yield return patternPatch;
            }

            if (!patternPatches.Done)
            {
                Log.Warn($"Failed to apply all patches in {typeof(Patches)}::{nameof(Patches.Chest_addItem_transpiler)}.");
            }
        }

        private static void Chest_clearNulls_postfix(Chest __instance)
        {
            var items = __instance.GetItemsForPlayer(Game1.player.UniqueMultiplayerID);
            for (var i = items.Count - 1; i >= 0; i--)
            {
                if (items[i] is null)
                {
                    items.RemoveAt(i);
                }
            }
        }

        private static bool Chest_draw_prefix(Chest __instance, ref int ___currentLidFrame, SpriteBatch spriteBatch, int x, int y, float alpha)
        {
            if (!__instance.TryGetStorage(out var storage) || storage.Format == Storage.AssetFormat.Vanilla)
            {
                return true;
            }

            float draw_x = x;
            float draw_y = y;
            if (__instance.localKickStartTile.HasValue)
            {
                draw_x = Utility.Lerp(__instance.localKickStartTile.Value.X, draw_x, __instance.kickProgress);
                draw_y = Utility.Lerp(__instance.localKickStartTile.Value.Y, draw_y, __instance.kickProgress);
            }

            var globalPosition = new Vector2(draw_x, (int)(draw_y - storage.Depth / 16f - 1f));
            var layerDepth = Math.Max(0.0f, ((draw_y + 1f) * 64f - 24f) / 10000f) + draw_x * 1E-05f;
            return !storage.Draw(
                __instance,
                ___currentLidFrame,
                spriteBatch,
                Game1.GlobalToLocal(Game1.viewport, globalPosition * 64),
                Vector2.Zero,
                alpha,
                layerDepth);
        }

        private static bool Chest_drawLocal_prefix(Chest __instance, ref int ___currentLidFrame, SpriteBatch spriteBatch, int x, int y, float alpha)
        {
            if (!__instance.TryGetStorage(out var storage) || storage.Format == Storage.AssetFormat.Vanilla)
            {
                return true;
            }

            storage.Draw(
                __instance,
                ___currentLidFrame,
                spriteBatch,
                new(x, y),
                Vector2.Zero,
                alpha);

            return false;
        }

        private static bool Chest_drawInMenu_prefix(Chest __instance, ref int ___currentLidFrame, SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, Color color)
        {
            if (!__instance.TryGetStorage(out var storage) || storage.Format == Storage.AssetFormat.Vanilla)
            {
                return true;
            }

            if (!storage.Draw(
                __instance,
                ___currentLidFrame,
                spriteBatch,
                location + new Vector2(32, 32),
                new(storage.Width / 2f, storage.Height / 2f),
                transparency,
                layerDepth,
                scaleSize * storage.ScaleSize))
            {
                return true;
            }

            // Draw Stack
            if (__instance.Stack > 1)
            {
                Utility.drawTinyDigits(__instance.Stack, spriteBatch, location + new Vector2(64 - Utility.getWidthOfTinyDigitString(__instance.Stack, 3f * scaleSize) - 3f * scaleSize, 64f - 18f * scaleSize + 2f), 3f * scaleSize, 1f, color);
            }

            // Draw Held Items
            var items = __instance.GetItemsForPlayer(Game1.player.UniqueMultiplayerID).Count;
            if (items > 0)
            {
                Utility.drawTinyDigits(items, spriteBatch, location + new Vector2(64 - Utility.getWidthOfTinyDigitString(items, 3f * scaleSize) - 3f * scaleSize, 2f * scaleSize), 3f * scaleSize, 1f, color);
            }

            return false;
        }

        private static void Chest_GetItemsForPlayer_postfix(Chest __instance, ref NetObjectList<Item> __result, long id)
        {
            if (__instance.heldObject.Value is Chest chest)
            {
                __result = chest.GetItemsForPlayer(id);
            }
        }

        private static IEnumerable<CodeInstruction> Chest_performToolAction_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            Log.Trace("Override create debris for Chest removeAction.");
            var createDebrisPatch = new PatternPatch();
            createDebrisPatch
                .Find(new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(NetMutex), nameof(NetMutex.RequestLock))))
                .Patch(
                    list =>
                    {
                        list.RemoveLast();
                        list.RemoveLast();
                        list.RemoveLast();
                        list.RemoveLast();
                        list.RemoveLast();
                        list.RemoveLast();
                        list.AddLast(new CodeInstruction(OpCodes.Ldarg_1));
                        list.AddLast(new CodeInstruction(OpCodes.Ldarg_2));
                        list.AddLast(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Patches), nameof(Patches.Chest_performToolAction_delegate))));
                    });

            var patternPatches = new PatternPatches(instructions);
            patternPatches.AddPatch(createDebrisPatch);

            foreach (var patternPatch in patternPatches)
            {
                yield return patternPatch;
            }

            if (!patternPatches.Done)
            {
                Log.Warn($"Failed to apply all patches in {typeof(Patches)}::{nameof(Patches.Chest_performToolAction_transpiler)}");
            }
        }

        private static void Chest_performToolAction_delegate(Chest chest, Tool tool, GameLocation location)
        {
            var player = tool.getLastFarmerToUse();
            if (player is null)
            {
                return;
            }

            var c = chest.TileLocation;
            if (c == Vector2.Zero)
            {
                var obj = location.Objects.Pairs.SingleOrDefault(obj => obj.Value == chest);
                c = obj.Value is not null ? obj.Key : player.GetToolLocation() / 64;
                c.X = (int)c.X;
                c.Y = (int)c.Y;
            }

            chest.GetMutex().RequestLock(
                () =>
                {
                    chest.clearNulls();
                    if (chest.isEmpty())
                    {
                        chest.performRemoveAction(chest.TileLocation, location);
                        if (location.Objects.Remove(c) && chest.Type.Equals("Crafting") && chest.Fragility != 2)
                        {
                            var debris = new Debris(
                                chest.bigCraftable.Value ? -chest.ParentSheetIndex : chest.ParentSheetIndex,
                                player.GetToolLocation(),
                                new(player.GetBoundingBox().Center.X, player.GetBoundingBox().Center.Y))
                            {
                                item = chest,
                            };

                            location.debris.Add(debris);
                        }
                    }
                    else if (tool.isHeavyHitter() && tool is not MeleeWeapon)
                    {
                        location.playSound("hammer");
                        chest.shakeTimer = 100;
                        if (tool != player.CurrentTool)
                        {
                            // var zero = Vector2.Zero;
                            var zero = player.FacingDirection switch
                            {
                                1 => new(1f, 0f),
                                3 => new(-1f, 0f),
                                0 => new(0f, -1f),
                                _ => new Vector2(0f, 1f),
                            };

                            if (chest.TileLocation.X == 0f && chest.TileLocation.Y == 0f && location.getObjectAtTile((int)c.X, (int)c.Y) == chest)
                            {
                                chest.TileLocation = c;
                            }

                            chest.MoveToSafePosition(location, chest.TileLocation, 0, zero);
                        }
                    }

                    chest.GetMutex().ReleaseLock();
                });
        }

        private static void Item_canStackWith_postfix(Item __instance, ref bool __result, ISalable other)
        {
            if (!__result)
            {
                return;
            }

            var chest = __instance as Chest;
            var otherChest = other as Chest;

            // Block if either chest has any items
            if (chest is not null && chest.GetItemsForPlayer(Game1.player.UniqueMultiplayerID).Any()
                || otherChest is not null && otherChest.GetItemsForPlayer(Game1.player.UniqueMultiplayerID).Any())
            {
                __result = false;
                return;
            }

            if (chest is null || otherChest is null)
            {
                return;
            }

            // Block if mismatched data
            if (chest.playerChoiceColor.Value.PackedValue != otherChest.playerChoiceColor.Value.PackedValue)
            {
                __result = false;
                return;
            }

            // Block if mismatched modData
            foreach (var key in chest.modData.Keys.Concat(otherChest.modData.Keys).Distinct())
            {
                var hasValue = chest.modData.TryGetValue(key, out var value);
                var OtherHasValue = otherChest.modData.TryGetValue(key, out var otherValue);
                if (hasValue && OtherHasValue)
                {
                    if (value == otherValue)
                    {
                        continue;
                    }

                    __result = false;
                    return;
                }

                if (!hasValue)
                {
                    chest.modData[key] = otherValue;
                    continue;
                }

                otherChest.modData[key] = value;
            }
        }

        private static void ItemGrabMenu_constructor_postfix(ItemGrabMenu __instance)
        {
            if (__instance.context is not Chest chest || !chest.TryGetStorage(out var storage))
            {
                return;
            }

            if (!storage.PlayerColor)
            {
                __instance.chestColorPicker = null;
                __instance.colorPickerToggleButton = null;
                __instance.discreteColorPickerCC = null;
                __instance.SetupBorderNeighbors();
                __instance.RepositionSideButtons();
            }
        }

        private static void ItemGrabMenu_setSourceItem_postfix(ItemGrabMenu __instance)
        {
            if (__instance.context is not Chest chest || !chest.TryGetStorage(out var storage))
            {
                return;
            }

            if (!storage.PlayerColor)
            {
                __instance.chestColorPicker = null;
                __instance.colorPickerToggleButton = null;
                __instance.discreteColorPickerCC = null;
                __instance.RepositionSideButtons();
            }
        }

        private static bool Object_drawWhenHeld_prefix(SObject __instance, SpriteBatch spriteBatch, Vector2 objectPosition)
        {
            if (__instance is not Chest chest || !__instance.TryGetStorage(out var storage) || storage.Format == Storage.AssetFormat.Vanilla || storage.Width != 16 || storage.Height != 32)
            {
                return true;
            }

            chest.draw(spriteBatch, (int)objectPosition.X, (int)objectPosition.Y);
            return false;
        }

        private static void Object_getDescription_postfix(SObject __instance, ref string __result)
        {
            if (__instance.TryGetStorage(out var storage) && storage.Format != Storage.AssetFormat.Vanilla && !string.IsNullOrWhiteSpace(storage.Description))
            {
                __result = storage.Description;
            }
        }

        private static void Object_getOne_postfix(SObject __instance, ref Item __result)
        {
            if (__instance.TryGetStorage(out var storage) && storage.Format != Storage.AssetFormat.Vanilla && !string.IsNullOrWhiteSpace(storage.Name))
            {
                __result = __result.ToChest(storage);
            }
        }

        private static void Object_loadDisplayName_postfix(SObject __instance, ref string __result)
        {
            if (__instance.TryGetStorage(out var storage) && storage.Format != Storage.AssetFormat.Vanilla && !string.IsNullOrWhiteSpace(storage.Name))
            {
                __result = storage.DisplayName;
            }
        }

        [HarmonyPriority(Priority.High)]
        private static bool Object_placementAction_prefix(SObject __instance, ref bool __result, GameLocation location, int x, int y)
        {
            if (!__instance.TryGetStorage(out var storage) || storage.Format == Storage.AssetFormat.Vanilla)
            {
                return true;
            }

            __result = false;
            var placementTile = new Vector2((int)(x / 64f), (int)(y / 64f));
            if (location.objects.ContainsKey(placementTile) || location is MineShaft or VolcanoDungeon || storage.IsFridge && location is not FarmHouse && location is not IslandFarmHouse)
            {
                Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.13053"));
                return false;
            }

            if (storage.IsFridge && location is FarmHouse {upgradeLevel: < 1})
            {
                Game1.showRedMessage(Game1.content.LoadString("Strings\\UI:MiniFridge_NoKitchen"));
                return false;
            }

            __result = true;
            return true;
        }
    }
}