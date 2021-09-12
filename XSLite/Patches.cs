using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection.Emit;
using Common.Extensions;
using CommonHarmony;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Network;
using StardewValley.Objects;
using StardewValley.Tools;
using SObject = StardewValley.Object;

namespace XSLite
{
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Naming convention defined by Harmony")]
    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter")]
    [SuppressMessage("ReSharper", "UnusedParameter.Local")]
    internal class Patches
    {
        private static IModHelper Helper;
        private static IMonitor Monitor;
        public Patches(IModHelper helper, IMonitor monitor, Harmony harmony)
        {
            Helper = helper;
            Monitor = monitor;
            
            #region Chest Patches
            // Use GetItemsForPlayer for all chest types.
            harmony.Patch(
                original: AccessTools.Method(typeof(Chest), nameof(Chest.addItem)),
                transpiler: new HarmonyMethod(typeof(Patches), nameof(Patches.Chest_addItem_transpiler))
            );
            // Clear nulls for heldStorage items
            harmony.Patch(
                original: AccessTools.Method(typeof(Chest), nameof(Chest.clearNulls)),
                postfix: new HarmonyMethod(typeof(Patches), nameof(Patches.Chest_clearNulls_postfix))
            );
            // Draw bigger storages from the origin chest.
            harmony.Patch(
                original: AccessTools.Method(typeof(Chest), nameof(Chest.draw), new[] {typeof(SpriteBatch), typeof(int), typeof(int), typeof(float)}),
                prefix: new HarmonyMethod(typeof(Patches), nameof(Patches.Chest_draw_prefix))
            );
            // Draw chest with playerChoiceColor and animation when held.
            harmony.Patch(
                original: AccessTools.Method(typeof(Chest), nameof(Chest.draw), new[] {typeof(SpriteBatch), typeof(int), typeof(int), typeof(float), typeof(bool)}),
                prefix: new HarmonyMethod(typeof(Patches), nameof(Patches.Chest_drawLocal_prefix))
            );
            // Draw chest with playerChoiceColor and animation in menu.
            harmony.Patch(
                original: AccessTools.Method(typeof(Chest), nameof(Chest.drawInMenu), new[] {typeof(SpriteBatch), typeof(Vector2), typeof(float), typeof(float), typeof(float), typeof(StackDrawType), typeof(Color), typeof(bool)}),
                prefix: new HarmonyMethod(typeof(Patches), nameof(Patches.Chest_drawInMenu_prefix))
            );
            // Prevent OpenNearby chests from resetting their lid frame automatically.
            harmony.Patch(
                original: AccessTools.Method(typeof(Chest), nameof(Chest.fixLidFrame)),
                prefix: new HarmonyMethod(typeof(Patches), nameof(Patches.Chest_fixLidFrame_prefix))
            );
            // Allow chests to hold more or less items than the default (36).
            if (Helper.ModRegistry.IsLoaded("furyx639.XSPlus"))
            {
                harmony.Patch(
                    original: AccessTools.Method(typeof(Chest), nameof(Chest.GetActualCapacity)),
                    postfix: new HarmonyMethod(typeof(Patches), nameof(Patches.Chest_GetActualCapacity_postfix))
                );
            }
            // Return items from heldItem Chest.
            harmony.Patch(
                original: AccessTools.Method(typeof(Chest), nameof(Chest.GetItemsForPlayer)),
                postfix: new HarmonyMethod(typeof(Patches), nameof(Patches.Chest_GetItemsForPlayer_postfix))
            );
            // Create expanded storage debris.
            harmony.Patch(
                original: AccessTools.Method(typeof(Chest), nameof(Chest.performToolAction)),
                transpiler: new HarmonyMethod(typeof(Patches), nameof(Patches.Chest_performToolAction_transpiler))
            );
            // Support calculating distance correctly for bigger chests.
            harmony.Patch(
                original: AccessTools.Method(typeof(Chest), nameof(Chest.UpdateFarmerNearby)),
                prefix: new HarmonyMethod(typeof(Patches), nameof(Patches.Chest_UpdateFarmerNearby_prefix))
            );
            // Animate the lids for OpenNearby chests.
            harmony.Patch(
                original: AccessTools.Method(typeof(Chest), nameof(Chest.updateWhenCurrentLocation)),
                prefix: new HarmonyMethod(typeof(Patches), nameof(Patches.Chest_updateWhenCurrentLocation_prefix))
            );
            #endregion
            
            #region Item Patches
            // Disallow stacking Chests holding items.
            harmony.Patch(
                original: AccessTools.Method(typeof(Item), nameof(Item.canStackWith)),
                postfix: new HarmonyMethod(typeof(Patches), nameof(Patches.Item_canStackWith_postfix))
            );
            #endregion
            
            #region ItemGrabMenu Patches
            // Remove disabled components
            harmony.Patch(
                original: AccessTools.Constructor(typeof(ItemGrabMenu), new []{ typeof(IList<Item>), typeof(bool), typeof(bool), typeof(InventoryMenu.highlightThisItem), typeof(ItemGrabMenu.behaviorOnItemSelect), typeof(string), typeof(ItemGrabMenu.behaviorOnItemSelect), typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(int), typeof(Item), typeof(int), typeof(object) }),
                postfix: new HarmonyMethod(typeof(Patches), nameof(Patches.ItemGrabMenu_constructor_postfix))
            );
            // Remove disabled components
            harmony.Patch(
                original: AccessTools.Method(typeof(ItemGrabMenu), nameof(ItemGrabMenu.setSourceItem)),
                postfix: new HarmonyMethod(typeof(Patches), nameof(Patches.ItemGrabMenu_setSourceItem_postfix))
            );
            #endregion
            
            #region Object Patches
            // Disable drawing extension objects for bigger storages.
            harmony.Patch(
                original: AccessTools.Method(typeof(SObject), nameof(SObject.draw), new[] {typeof(SpriteBatch), typeof(int), typeof(int), typeof(float)}),
                prefix: new HarmonyMethod(typeof(Patches), nameof(Patches.Object_draw_prefix))
            );
            // Draw bigger held storages.
            harmony.Patch(
                original: AccessTools.Method(typeof(SObject), nameof(SObject.drawWhenHeld)),
                prefix: new HarmonyMethod(typeof(Patches), nameof(Patches.Object_drawWhenHeld_prefix))
            );
            // Draw placement bounds for bigger storages.
            harmony.Patch(
                original: AccessTools.Method(typeof(SObject), nameof(SObject.drawPlacementBounds)),
                prefix: new HarmonyMethod(typeof(Patches), nameof(Patches.Object_drawPlacementBounds_prefix))
            );
            // Return custom description for Object.
            harmony.Patch(
                original: AccessTools.Method(typeof(SObject), nameof(SObject.getDescription)),
                prefix: new HarmonyMethod(typeof(Patches), nameof(Patches.Object_getDescription_prefix))
            );
            // Return custom display name for Object.
            harmony.Patch(
                original: AccessTools.Method(typeof(SObject), "loadDisplayName"),
                prefix: new HarmonyMethod(typeof(Patches), nameof(Patches.Object_loadDisplayName_prefix))
            );
            // Perform tool actions at origin chest for bigger storages.
            harmony.Patch(
                original: AccessTools.Method(typeof(SObject), nameof(SObject.performToolAction)),
                prefix: new HarmonyMethod(typeof(Patches), nameof(Patches.Object_performToolAction_prefix))
            );
            // Disallow invalid chest placement locations.
            harmony.Patch(
                original: AccessTools.Method(typeof(SObject), nameof(SObject.placementAction)),
                prefix: new HarmonyMethod(typeof(Patches), nameof(Patches.Object_placementAction_prefix))
            );
            // Convert XS storages placed as objects into chests.
            harmony.Patch(
                original: AccessTools.Method(typeof(SObject), nameof(SObject.placementAction)),
                postfix: new HarmonyMethod(typeof(Patches), nameof(Patches.Object_placementAction_postfix))
            );
            #endregion
            
            #region Utility Patches
            // Include chests in player inventory for iterateChestsAndStorage.
            harmony.Patch(
                original: AccessTools.Method(typeof(Utility), nameof(Utility.iterateChestsAndStorage)),
                postfix: new HarmonyMethod(typeof(Patches), nameof(Patches.Utility_iterateChestsAndStorage_postfix))
            );
            // Check placement parameters for bigger storages.
            harmony.Patch(
                original: AccessTools.Method(typeof(Utility), nameof(Utility.playerCanPlaceItemHere)),
                postfix: new HarmonyMethod(typeof(Patches), nameof(Patches.Utility_playerCanPlaceItemHere_postfix))
            );
            #endregion
        }
        #region Chest Patches
        private static IEnumerable<CodeInstruction> Chest_addItem_transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (var instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Ldfld && instruction.operand.Equals(AccessTools.Field(typeof(Chest), nameof(Chest.items))))
                {
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Property(typeof(Game1), nameof(Game1.player)).GetGetMethod());
                    yield return new CodeInstruction(OpCodes.Callvirt, AccessTools.Property(typeof(Farmer), nameof(Farmer.UniqueMultiplayerID)).GetGetMethod());
                    yield return new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(Chest), nameof(Chest.GetItemsForPlayer)));
                }
                else
                {
                    yield return instruction;
                }
            }
        }
        private static void Chest_clearNulls_postfix(Chest __instance)
        {
            var items = __instance.GetItemsForPlayer(Game1.player.UniqueMultiplayerID);
            for (var i = items.Count - 1; i >= 0; i--)
            {
                if (items[i] == null)
                {
                    items.RemoveAt(i);
                }
            }
        }
        private static bool Chest_draw_prefix(Chest __instance, ref int ___currentLidFrame, SpriteBatch spriteBatch, int x, int y, float alpha)
        {
            if (!__instance.TryGetStorage(out var storage) || storage.Format == Storage.AssetFormat.Vanilla)
                return true;
            if (storage.TileHeight > 1 || storage.TileWidth > 1)
            {
                if (!__instance.modData.TryGetValue($"{XSLite.ModPrefix}/X", out var xStr)
                    || !__instance.modData.TryGetValue($"{XSLite.ModPrefix}/Y", out var yStr)
                    || !int.TryParse(xStr, out var xPos)
                    || !int.TryParse(yStr, out var yPos))
                    return true;
                if (x != xPos || y != yPos)
                    return false;
            }
            var draw_x = (float) x;
            var draw_y = (float) y;
            if (__instance.localKickStartTile.HasValue)
            {
                draw_x = Utility.Lerp(__instance.localKickStartTile.Value.X, draw_x, __instance.kickProgress);
                draw_y = Utility.Lerp(__instance.localKickStartTile.Value.Y, draw_y, __instance.kickProgress);
            }
            var globalPosition = new Vector2(draw_x, (int) (draw_y - storage.Depth / 16f - 1f));
            var layerDepth = Math.Max(0.0f, ((draw_y + 1f) * 64f - 24f) / 10000f) + draw_x * 1E-05f;
            return !storage.Draw(
                __instance,
                ___currentLidFrame,
                spriteBatch,
                Game1.GlobalToLocal(Game1.viewport, globalPosition * 64),
                Vector2.Zero,
                alpha,
                layerDepth
            );
        }
        private static bool Chest_drawLocal_prefix(Chest __instance, ref int ___currentLidFrame, SpriteBatch spriteBatch, int x, int y, float alpha, bool local)
        {
            if (!__instance.TryGetStorage(out var storage) || storage.Format == Storage.AssetFormat.Vanilla)
                return true;
            storage.Draw(
                __instance,
                ___currentLidFrame,
                spriteBatch,
                new Vector2(x, y - 64),
                Vector2.Zero,
                alpha
            );
            return false;
        }
        private static bool Chest_drawInMenu_prefix(Chest __instance, ref int ___currentLidFrame, SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
        {
            if (!__instance.TryGetStorage(out var storage) || storage.Format == Storage.AssetFormat.Vanilla)
                return true;
            var origin = new Vector2(storage.Width / 2f, storage.Height / 2f);
            var drawScaleSize = scaleSize * storage.ScaleSize;
            var draw = storage.Draw(
                __instance,
                ___currentLidFrame,
                spriteBatch,
                location + new Vector2(32, 32),
                origin,
                transparency,
                layerDepth,
                drawScaleSize
            );
            if (!draw)
                return true;
            // Draw Stack
            if (__instance.Stack > 1)
                Utility.drawTinyDigits(__instance.Stack, spriteBatch, location + new Vector2(64 - Utility.getWidthOfTinyDigitString(__instance.Stack, 3f * scaleSize) - 3f * scaleSize, 64f - 18f * scaleSize + 2f), 3f * scaleSize, 1f, color);
            // Draw Held Items
            var items = __instance.GetItemsForPlayer(Game1.player.UniqueMultiplayerID).Count;
            if (items > 0)
                Utility.drawTinyDigits(items, spriteBatch, location + new Vector2(64 - Utility.getWidthOfTinyDigitString(items, 3f * scaleSize) - 3f * scaleSize, 2f * scaleSize), 3f * scaleSize, 1f, color);
            return false;
        }
        private static bool Chest_fixLidFrame_prefix(Chest __instance, ref int ___currentLidFrame)
        {
            if (!__instance.TryGetStorage(out var storage) || storage.OpenNearby <= 0)
                return true;
            if (___currentLidFrame == 0)
                ___currentLidFrame = __instance.startingLidFrame.Value;
            return false;
        }
        [HarmonyPriority(Priority.High)]
        private static void Chest_GetActualCapacity_postfix(Chest __instance, ref int __result)
        {
            if (!__instance.TryGetStorage(out var storage) || storage.Capacity == 0)
                return;
            __result = storage.Capacity switch
            {
                -1 => int.MaxValue,
                > 0 => storage.Capacity,
                _ => __result
            };
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
            var patternPatches = new PatternPatches(instructions, Monitor);
            
            patternPatches
                .Find(
                    new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(NetMutex), nameof(NetMutex.RequestLock)))
                )
                .Log("Override create debris for Chest removeAction.")
                .Patch(delegate(LinkedList<CodeInstruction> list)
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
            
            foreach (var patternPatch in patternPatches)
                yield return patternPatch;
            
            if (!patternPatches.Done)
                Monitor.Log($"Failed to apply all patches in {typeof(Patches)}::{nameof(Patches.Chest_performToolAction_transpiler)}", LogLevel.Warn);
        }
        private static void Chest_performToolAction_delegate(Chest chest, Tool tool, GameLocation location)
        {
            var player = tool.getLastFarmerToUse();
            if (player == null)
                return;
            var c = chest.TileLocation;
            if (c == Vector2.Zero)
            {
                var obj = location.Objects.Pairs.SingleOrDefault(obj => obj.Value == chest);
                c = obj.Value is not null ? obj.Key : player.GetToolLocation() / 64;
                c.X = (int)c.X;
                c.Y = (int)c.Y;
            }
            chest.GetMutex().RequestLock(delegate
            {
                chest.clearNulls();
                if (chest.isEmpty())
                {
                    chest.performRemoveAction(chest.TileLocation, location);
                    if (location.Objects.Remove(c) && chest.Type.Equals("Crafting") && chest.Fragility != 2)
                    {
                        var debris = new Debris(
                            objectIndex: chest.bigCraftable.Value ? -chest.ParentSheetIndex : chest.ParentSheetIndex,
                            debrisOrigin: player.GetToolLocation(),
                            playerPosition: new Vector2(player.GetBoundingBox().Center.X, player.GetBoundingBox().Center.Y)
                        )
                        {
                            item = chest
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
                        //var zero = Vector2.Zero;
                        var zero = player.FacingDirection switch
                        {
                            1 => new Vector2(1f, 0f),
                            3 => new Vector2(-1f, 0f),
                            0 => new Vector2(0f, -1f),
                            _ => new Vector2(0f, 1f)
                        };
                        if (chest.TileLocation.X == 0f && chest.TileLocation.Y == 0f && location.getObjectAtTile((int) c.X, (int) c.Y) == chest)
                        {
                            chest.TileLocation = c;
                        }
                        chest.MoveToSafePosition(location, chest.TileLocation, 0, zero);
                    }
                }
                chest.GetMutex().ReleaseLock();
            });
        }
        private static bool Chest_UpdateFarmerNearby_prefix(Chest __instance, ref bool ____farmerNearby, ref int ____shippingBinFrameCounter, ref int ___currentLidFrame, GameLocation location, bool animate)
        {
            if (!__instance.TryGetStorage(out var storage) || storage.OpenNearby <= 0)
                return true;
            if (ReferenceEquals(XSLite.CurrentChest.Value, __instance))
            {
                if (____farmerNearby)
                    return false;
                ____farmerNearby = true;
                ____shippingBinFrameCounter = 5;
                return false;
            }
            if (Game1.player.Items.Take(12).Any(item => ReferenceEquals(item, __instance)))
            {
                if (!____farmerNearby)
                    return false;
                ____farmerNearby = false;
                ____shippingBinFrameCounter = 5;
                return false;
            }
            var shouldOpen = false;
            if (!__instance.modData.TryGetValue($"{XSLite.ModPrefix}/X", out var xStr) || !int.TryParse(xStr, out var xPos) || xPos == 0)
                xPos = (int) __instance.TileLocation.X;
            if (!__instance.modData.TryGetValue($"{XSLite.ModPrefix}/Y", out var yStr) || !int.TryParse(yStr, out var yPos) || yPos == 0)
                yPos = (int) __instance.TileLocation.Y;
            var tileHeight = storage.TileHeight >= 1 ? storage.TileHeight : 1;
            var tileWidth = storage.TileWidth >= 1 ? storage.TileWidth : 1;
            for (var i = 0; i < tileHeight; i++)
            {
                for (var j = 0; j < tileWidth; j++)
                {
                    var pos = new Vector2(xPos + j, yPos + i);
                    shouldOpen = location.farmers.Any(farmer => Math.Abs(farmer.getTileX() - pos.X) <= storage.OpenNearby && Math.Abs(farmer.getTileY() - pos.Y) <= storage.OpenNearby);
                    if (shouldOpen)
                        break;
                }
                if (shouldOpen)
                    break;
            }
            if (shouldOpen == ____farmerNearby)
                return false;
            ____farmerNearby = shouldOpen;
            ____shippingBinFrameCounter = 5;
            if (!animate)
            {
                ____shippingBinFrameCounter = -1;
                ___currentLidFrame = ____farmerNearby ? __instance.getLastLidFrame() : __instance.startingLidFrame.Value;
            }
            else if (Game1.gameMode != 6)
            {
                if (____farmerNearby && !string.IsNullOrWhiteSpace(storage.OpenNearbySound))
                    location.localSound(storage.OpenNearbySound);
                else if (!____farmerNearby && !string.IsNullOrWhiteSpace(storage.CloseNearbySound))
                    location.localSound(storage.CloseNearbySound);
            }
            return false;
        }
        private static bool Chest_updateWhenCurrentLocation_prefix(Chest __instance, ref int ___health, ref int ____shippingBinFrameCounter, ref bool ____farmerNearby, ref int ___currentLidFrame, GameTime time, GameLocation environment)
        {
            if (!__instance.TryGetStorage(out var storage))
                return true;
            if (__instance.synchronized.Value)
                __instance.openChestEvent.Poll();
            if (!__instance.localKickStartTile.HasValue)
                __instance.kickProgress = -1f;
            else
            {
                if (Game1.currentLocation.Equals(environment))
                {
                    if (__instance.kickProgress == 0f)
                    {
                        if (Utility.isOnScreen((__instance.localKickStartTile.Value + new Vector2(0.5f, 0.5f)) * 64f, 64))
                            environment.localSound("clubhit");
                        __instance.shakeTimer = 100;
                    }
                }
                else
                {
                    __instance.localKickStartTile = null;
                    __instance.kickProgress = -1f;
                }
                if (__instance.kickProgress >= 0f)
                {
                    __instance.kickProgress += (float) (time.ElapsedGameTime.TotalSeconds / 0.25f);
                    if (__instance.kickProgress >= 1f)
                    {
                        __instance.kickProgress = -1f;
                        __instance.localKickStartTile = null;
                    }
                }
            }
            __instance.fixLidFrame();
            __instance.mutex.Update(environment);
            if (__instance.shakeTimer > 0)
            {
                __instance.shakeTimer -= time.ElapsedGameTime.Milliseconds;
                if (__instance.shakeTimer <= 0)
                    ___health = 10;
            }
            if (storage.OpenNearby <= 0)
            {
                if (__instance.frameCounter.Value > -1 && ___currentLidFrame < __instance.getLastLidFrame() + 1)
                {
                    __instance.frameCounter.Value--;
                    if (__instance.frameCounter.Value > 0 || !__instance.GetMutex().IsLockHeld())
                        return false;
                    if (___currentLidFrame == __instance.getLastLidFrame())
                    {
                        __instance.ShowMenu();
                        __instance.frameCounter.Value = -1;
                    }
                    else
                    {
                        __instance.frameCounter.Value = 5;
                        ___currentLidFrame++;
                    }
                }
                else if ((__instance.frameCounter.Value == -1 && ___currentLidFrame > __instance.startingLidFrame.Value || ___currentLidFrame >= __instance.getLastLidFrame()) && Game1.activeClickableMenu == null && __instance.GetMutex().IsLockHeld())
                {
                    __instance.GetMutex().ReleaseLock();
                    ___currentLidFrame = __instance.getLastLidFrame();
                    __instance.frameCounter.Value = 2;
                    environment.localSound(storage.CloseNearbySound);
                }
                return false;
            }
            __instance.UpdateFarmerNearby(environment);
            if (____shippingBinFrameCounter <= -1)
                return false;
            ____shippingBinFrameCounter--;
            if (____shippingBinFrameCounter <= 0)
            {
                ____shippingBinFrameCounter = 5;
                if (____farmerNearby && ___currentLidFrame < __instance.getLastLidFrame())
                    ___currentLidFrame++;
                else if (!____farmerNearby && ___currentLidFrame > __instance.startingLidFrame.Value)
                    ___currentLidFrame--;
                else
                    ____shippingBinFrameCounter = -1;
            }
            if (Game1.activeClickableMenu == null && __instance.GetMutex().IsLockHeld())
                __instance.GetMutex().ReleaseLock();
            return false;
        }
        #endregion
        
        #region Item Patches
        private static void Item_canStackWith_postfix(Item __instance, ref bool __result, ISalable other)
        {
            var chest = __instance is Chest chest1 ? chest1 : null;
            var otherChest = other is Chest chest2 ? chest2 : null;
            if (!__result || chest is null && otherChest is null)
                return;
            
            // Block if either chest has any items 
            if (chest != null && chest.GetItemsForPlayer(Game1.player.UniqueMultiplayerID).Any()
                || otherChest != null && otherChest.GetItemsForPlayer(Game1.player.UniqueMultiplayerID).Any())
            {
                __result = false;
                return;
            }
            
            if (chest == null || otherChest == null)
                return;
            
            // Block if mismatched data
            if (chest.playerChoiceColor.Value != otherChest.playerChoiceColor.Value
                || !chest.modData.Keys.All(key => otherChest.modData.TryGetValue(key, out var value) && chest.modData[key] == value)
                || !otherChest.modData.Keys.All(key => chest.modData.TryGetValue(key, out var value) && otherChest.modData[key] == value))
            {
                __result = false;
            }
        }
        #endregion
        
        #region ItemGrabMenu Patches
        private static void ItemGrabMenu_constructor_postfix(ItemGrabMenu __instance)
        {
            if (__instance.context is not Chest chest || !chest.TryGetStorage(out var storage))
                return;
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
                return;
            if (!storage.PlayerColor)
            {
                __instance.chestColorPicker = null;
                __instance.colorPickerToggleButton = null;
                __instance.discreteColorPickerCC = null;
                __instance.RepositionSideButtons();
            }
        }
        #endregion
        
        #region Object Patches
        private static bool Object_draw_prefix(SObject __instance, SpriteBatch spriteBatch, int x, int y, float alpha)
        {
            if (!__instance.modData.TryGetValue($"{XSLite.ModPrefix}/Storage", out _)
                || !__instance.modData.TryGetValue($"{XSLite.ModPrefix}/X", out var xStr)
                || !__instance.modData.TryGetValue($"{XSLite.ModPrefix}/Y", out var yStr)
                || !int.TryParse(xStr, out var xPos)
                || !int.TryParse(yStr, out var yPos))
                return true;
            return xPos == x && yPos == y;
        }
        private static bool Object_drawWhenHeld_prefix(SObject __instance, SpriteBatch spriteBatch, Vector2 objectPosition, Farmer f)
        {
            if (!__instance.TryGetStorage(out var storage) || storage.Format == Storage.AssetFormat.Vanilla)
                return true;
            objectPosition.X -= storage.Width * 2f - 32;
            objectPosition.Y -= storage.Height * 2f - 64;
            var currentFrame = XSLite.CurrentLidFrame.Value?.GetValue() ?? 0;
            return !storage.Draw(__instance, currentFrame, spriteBatch, objectPosition, Vector2.Zero);
        }
        private static bool Object_drawPlacementBounds_prefix(SObject __instance, SpriteBatch spriteBatch, GameLocation location)
        {
            if (!__instance.TryGetStorage(out var storage) || storage.Format == Storage.AssetFormat.Vanilla)
                return true;
            
            var tile = 64 * Game1.GetPlacementGrabTile();
            var x = (int) tile.X;
            var y = (int) tile.Y;
            
            Game1.isCheckingNonMousePlacement = !Game1.IsPerformingMousePlacement();
            if (Game1.isCheckingNonMousePlacement)
            {
                var pos = Utility.GetNearbyValidPlacementPosition(Game1.player, location, __instance, x, y);
                x = (int) pos.X;
                y = (int) pos.Y;
            }
            
            var canPlaceHere = Utility.playerCanPlaceItemHere(location, __instance, x, y, Game1.player)
                               || Utility.isThereAnObjectHereWhichAcceptsThisItem(location, __instance, x, y)
                               && Utility.withinRadiusOfPlayer(x, y, 1, Game1.player);
            
            Game1.isCheckingNonMousePlacement = false;
            
            storage.ForEachPos(x / 64, y / 64, delegate(Vector2 pos)
            {
                spriteBatch.Draw(Game1.mouseCursors,
                    pos * 64 - new Vector2(Game1.viewport.X, Game1.viewport.Y),
                    new Rectangle(canPlaceHere ? 194 : 210, 388, 16, 16),
                    Color.White,
                    0f,
                    Vector2.Zero,
                    4f,
                    SpriteEffects.None,
                    0.01f);
            });
            
            var globalPosition = new Vector2((int) (x / 64f), (int) (y / 64f - storage.Depth / 16f - 1f));
            return !storage.Draw(__instance, 0, spriteBatch, Game1.GlobalToLocal(Game1.viewport, globalPosition * 64), Vector2.Zero, 0.5f);
        }

        private static bool Object_getDescription_prefix(SObject __instance, ref string __result)
        {
            if (!__instance.TryGetStorage(out var storage) || storage.Format == Storage.AssetFormat.Vanilla || string.IsNullOrWhiteSpace(storage.Description))
                return true;
            __result = storage.Description;
            return false;
        }
        private static bool Object_loadDisplayName_prefix(SObject __instance, ref string __result)
        {
            if (!__instance.TryGetStorage(out var storage) || storage.Format == Storage.AssetFormat.Vanilla || string.IsNullOrWhiteSpace(storage.Name))
                return true;
            __result = storage.DisplayName;
            return false;
        }
        private static bool Object_performToolAction_prefix(SObject __instance, Tool t, GameLocation location)
        {
            if (!__instance.modData.ContainsKey($"{XSLite.ModPrefix}/Storage")
                || !__instance.modData.TryGetValue($"{XSLite.ModPrefix}/X", out var xStr)
                || !__instance.modData.TryGetValue($"{XSLite.ModPrefix}/Y", out var yStr)
                || !int.TryParse(xStr, out var xPos)
                || !int.TryParse(yStr, out var yPos)
                || !location.Objects.TryGetValue(new Vector2(xPos, yPos), out var obj)
                || obj == __instance
                || obj is not Chest chest)
                return true;
            return chest.performToolAction(t, location);
        }
        [HarmonyPriority(Priority.High)]
        private static bool Object_placementAction_prefix(SObject __instance, ref bool __result, GameLocation location, int x, int y, Farmer who)
        {
            if (!__instance.TryGetStorage(out var storage) || storage.Format == Storage.AssetFormat.Vanilla)
                return true;
            __result = false;
            var placementTile = new Vector2((int) (x / 64f), (int) (y / 64f));
            if (location.objects.ContainsKey(placementTile) || location is MineShaft or VolcanoDungeon || storage.IsFridge && location is not FarmHouse && location is not IslandFarmHouse)
            {
                Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.13053"));
                return false;
            }
            if (storage.IsFridge && location is FarmHouse { upgradeLevel: < 1 })
            {
                Game1.showRedMessage(Game1.content.LoadString("Strings\\UI:MiniFridge_NoKitchen"));
                return false;
            }
            __result = true;
            return true;
        }
        private static void Object_placementAction_postfix(SObject __instance, ref bool __result, GameLocation location, int x, int y)
        {
            var placementTile = new Vector2((int)(x / 64f), (int)(y / 64f));
            if (!location.Objects.TryGetValue(placementTile, out var obj) || !__instance.TryGetStorage(out var storage))
                return;
            storage.Replace(location, placementTile, __instance);
        }
        #endregion
        
        #region Utility Patches
        private static void Utility_iterateChestsAndStorage_postfix(Action<Item> action)
        {
            foreach (var farmer in Game1.getAllFarmers())
            {
                foreach (var chest in farmer.Items.OfType<Chest>())
                {
                    chest.RecursiveIterate(action);
                }
            }
        }
        private static void Utility_playerCanPlaceItemHere_postfix(ref bool __result, GameLocation location, Item item, int x, int y, Farmer f)
        {
            if (!XSLite.Storages.TryGetValue(item.Name, out var storage)  || storage.Format == Storage.AssetFormat.Vanilla || storage.TileWidth == 1 && storage.TileHeight == 1)
                return;
            
            x = 64 * (x / 64);
            y = 64 * (y / 64);
            
            if (Utility.isPlacementForbiddenHere(location) || Game1.eventUp || f.bathingClothes.Value || f.onBridge.Value)
            {
                __result = false;
                return;
            }
            
            // Is Within Tile With Leeway
            if (!Utility.withinRadiusOfPlayer(x, y, Math.Max(storage.TileWidth, storage.TileHeight), f))
            {
                __result = false;
                return;
            }
            
            // Position intersects with farmer
            var rect = new Rectangle(x, y, storage.TileWidth * 64, storage.TileHeight * 64);
            if (location.farmers.Any(farmer => farmer.GetBoundingBox().Intersects(rect)))
            {
                __result = false;
                return;
            }
            
            // Is Close Enough to Farmer
            rect.Inflate(32, 32);
            if (!rect.Intersects(f.GetBoundingBox()))
            {
                __result = false;
                return;
            }
            
            for (var i = 0; i < storage.TileWidth; i++)
            {
                for (var j = 0; j < storage.TileHeight; j++)
                {
                    var tileLocation = new Vector2(x / 64 + i, y / 64 + j);
                    if (item.canBePlacedHere(location, tileLocation)
                        && location.getObjectAtTile((int) tileLocation.X, (int) tileLocation.Y) == null
                        && location.isTilePlaceable(tileLocation, item))
                        continue;
                    
                    // Item cannot be placed here
                    __result = false;
                    return;
                }
            }
            
            __result = true;
        }
        #endregion
    }
}