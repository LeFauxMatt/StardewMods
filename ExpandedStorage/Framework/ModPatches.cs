namespace StardewMods.ExpandedStorage.Framework;

using System;
using System.Collections.Generic;
using System.Globalization;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.Common.Helpers.AtraBase.StringHandlers;
using StardewMods.Common.Integrations.ExpandedStorage;
using StardewMods.ExpandedStorage.Models;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;

/// <summary>
///     Harmony Patches for Expanded Storage.
/// </summary>
[SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
[SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Harmony")]
[SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
internal sealed class ModPatches
{
#nullable disable
    private static ModPatches Instance;
#nullable enable

    private readonly IModHelper _helper;
    private readonly IDictionary<string, ICustomStorage> _storages;

    private ModPatches(IModHelper helper, IManifest manifest, IDictionary<string, ICustomStorage> storages)
    {
        this._helper = helper;
        this._storages = storages;
        var harmony = new Harmony(manifest.UniqueID);

        // Drawing
        harmony.Patch(
            AccessTools.Method(
                typeof(Chest),
                nameof(Chest.draw),
                new[]
                {
                    typeof(SpriteBatch),
                    typeof(int),
                    typeof(int),
                    typeof(float),
                }),
            new(typeof(ModPatches), nameof(ModPatches.Chest_draw_prefix)));
        harmony.Patch(
            AccessTools.Method(
                typeof(Chest),
                nameof(Chest.draw),
                new[]
                {
                    typeof(SpriteBatch),
                    typeof(int),
                    typeof(int),
                    typeof(float),
                    typeof(bool),
                }),
            new(typeof(ModPatches), nameof(ModPatches.Chest_drawLocal_prefix)));
        harmony.Patch(
            AccessTools.Method(typeof(Chest), nameof(Chest.getLastLidFrame)),
            postfix: new(typeof(ModPatches), nameof(ModPatches.Chest_getLastLidFrame_postfix)));
        harmony.Patch(
            AccessTools.Method(
                typeof(SObject),
                nameof(SObject.draw),
                new[]
                {
                    typeof(SpriteBatch),
                    typeof(int),
                    typeof(int),
                    typeof(float),
                }),
            new(typeof(ModPatches), nameof(ModPatches.Object_draw_prefix)));
        harmony.Patch(
            AccessTools.Method(
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
                }),
            new(typeof(ModPatches), nameof(ModPatches.Object_drawInMenu_prefix)));
        harmony.Patch(
            AccessTools.Method(typeof(SObject), nameof(SObject.drawPlacementBounds)),
            new(typeof(ModPatches), nameof(ModPatches.Object_drawPlacementBounds_prefix)));
        harmony.Patch(
            AccessTools.Method(typeof(SObject), nameof(SObject.drawWhenHeld)),
            new(typeof(ModPatches), nameof(ModPatches.Object_drawWhenHeld_prefix)));

        // Crafting
        harmony.Patch(
            AccessTools.Method(typeof(CraftingPage), "layoutRecipes"),
            postfix: new(typeof(ModPatches), nameof(ModPatches.CraftingPage_layoutRecipes_postfix)));
        harmony.Patch(
            AccessTools.Constructor(
                typeof(CraftingRecipe),
                new[]
                {
                    typeof(string),
                    typeof(bool),
                }),
            postfix: new(typeof(ModPatches), nameof(ModPatches.CraftingRecipe_constructor_postfix)));
        harmony.Patch(
            AccessTools.Method(typeof(CraftingRecipe), nameof(CraftingRecipe.createItem)),
            postfix: new(typeof(ModPatches), nameof(ModPatches.CraftingRecipe_createItem_postfix)));
        harmony.Patch(
            AccessTools.Method(typeof(SObject), nameof(SObject.getDescription)),
            postfix: new(typeof(ModPatches), nameof(ModPatches.Object_getDescription_postfix)));
        harmony.Patch(
            AccessTools.Method(typeof(SObject), "loadDisplayName"),
            postfix: new(typeof(ModPatches), nameof(ModPatches.Object_loadDisplayName_postfix)));

        // Inventory
        harmony.Patch(
            AccessTools.Method(typeof(Item), nameof(Item.canStackWith)),
            postfix: new(typeof(ModPatches), nameof(ModPatches.Item_canStackWith_postfix)));

        // World
        harmony.Patch(
            AccessTools.Method(typeof(Chest), nameof(Chest.checkForAction)),
            new(typeof(ModPatches), nameof(ModPatches.Chest_chestForAction_prefix)));
        harmony.Patch(
            AccessTools.Method(typeof(Chest), nameof(Chest.performToolAction)),
            new(typeof(ModPatches), nameof(ModPatches.Chest_performToolAction_prefix)));
        harmony.Patch(
            AccessTools.Method(typeof(Chest), nameof(Chest.UpdateFarmerNearby)),
            new(typeof(ModPatches), nameof(ModPatches.Chest_UpdateFarmerNearby_prefix)));
        harmony.Patch(
            AccessTools.Method(typeof(Chest), nameof(Chest.updateWhenCurrentLocation)),
            new(typeof(ModPatches), nameof(ModPatches.Chest_updateWhenCurrentLocation_prefix)));
        harmony.Patch(
            AccessTools.Method(typeof(SObject), nameof(SObject.checkForAction)),
            postfix: new(typeof(ModPatches), nameof(ModPatches.Object_checkForAction_postfix)));
        harmony.Patch(
            AccessTools.Method(typeof(SObject), nameof(SObject.isPlaceable)),
            postfix: new(typeof(ModPatches), nameof(ModPatches.Object_isPlaceable_postfix)));
        harmony.Patch(
            AccessTools.Method(typeof(SObject), nameof(SObject.performToolAction)),
            new(typeof(ModPatches), nameof(ModPatches.Object_performToolAction_prefix)));
        harmony.Patch(
            AccessTools.Method(typeof(SObject), nameof(SObject.performRemoveAction)),
            postfix: new(typeof(ModPatches), nameof(ModPatches.Object_performRemoveAction_postfix)));
        harmony.Patch(
            AccessTools.Method(typeof(SObject), nameof(SObject.placementAction)),
            postfix: new(typeof(ModPatches), nameof(ModPatches.Object_placementAction_postfix)));
        harmony.Patch(
            AccessTools.Method(typeof(Utility), nameof(Utility.isWithinTileWithLeeway)),
            postfix: new(typeof(ModPatches), nameof(ModPatches.Utility_isWithinTileWithLeeway_postfix)));
        harmony.Patch(
            AccessTools.Method(typeof(Utility), nameof(Utility.playerCanPlaceItemHere)),
            postfix: new(typeof(ModPatches), nameof(ModPatches.Utility_playerCanPlaceItemHere_postfix)));

        // Using
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
                }),
            postfix: new(typeof(ModPatches), nameof(ModPatches.ItemGrabMenu_constructor_postfix)));
        harmony.Patch(
            AccessTools.Method(typeof(ItemGrabMenu), nameof(ItemGrabMenu.gameWindowSizeChanged)),
            postfix: new(typeof(ModPatches), nameof(ModPatches.ItemGrabMenu_gameWindowSizeChanged_postfix)));
        harmony.Patch(
            AccessTools.Method(typeof(ItemGrabMenu), nameof(ItemGrabMenu.setSourceItem)),
            postfix: new(typeof(ModPatches), nameof(ModPatches.ItemGrabMenu_setSourceItem_postfix)));

        // Buying
        harmony.Patch(
            AccessTools.Method(typeof(GameLocation), "sandyShopStock"),
            postfix: new(typeof(ModPatches), nameof(ModPatches.GameLocation_sandyShopStock_postfix)));
        harmony.Patch(
            AccessTools.Method(typeof(IslandNorth), nameof(IslandNorth.getIslandMerchantTradeStock)),
            postfix: new(typeof(ModPatches), nameof(ModPatches.IslandNorth_getIslandMerchantTradeStock_postfix)));
        harmony.Patch(
            AccessTools.Method(typeof(SeedShop), nameof(SeedShop.shopStock)),
            postfix: new(typeof(ModPatches), nameof(ModPatches.SeedShop_shopStock_postfix)));
        harmony.Patch(
            AccessTools.Method(typeof(Sewer), nameof(Sewer.getShadowShopStock)),
            postfix: new(typeof(ModPatches), nameof(ModPatches.Sewer_getShadowShopStock_postfix)));
        harmony.Patch(
            AccessTools.Constructor(
                typeof(ShopMenu),
                new[]
                {
                    typeof(Dictionary<ISalable, int[]>),
                    typeof(int),
                    typeof(string),
                    typeof(Func<ISalable, Farmer, int, bool>),
                    typeof(Func<ISalable, bool>),
                    typeof(string),
                }),
            new(typeof(ModPatches), nameof(ModPatches.ShopMenu_constructor_prefix)));
        harmony.Patch(
            AccessTools.Method(typeof(SObject), nameof(SObject.getOne)),
            postfix: new(typeof(ModPatches), nameof(ModPatches.Object_getOne_postfix)));
        harmony.Patch(
            AccessTools.Method(typeof(Utility), nameof(Utility.getAdventureShopStock)),
            postfix: new(typeof(ModPatches), nameof(ModPatches.Utility_getAdventureShopStock_postfix)));
        harmony.Patch(
            AccessTools.Method(typeof(Utility), nameof(Utility.getBlacksmithStock)),
            postfix: new(typeof(ModPatches), nameof(ModPatches.Utility_getBlacksmithStock_postfix)));
        harmony.Patch(
            AccessTools.Method(typeof(Utility), nameof(Utility.getCarpenterStock)),
            postfix: new(typeof(ModPatches), nameof(ModPatches.Utility_getCarpenterStock_postfix)));
        harmony.Patch(
            AccessTools.Method(typeof(Utility), nameof(Utility.getDwarfShopStock)),
            postfix: new(typeof(ModPatches), nameof(ModPatches.Utility_getDwarfShopStock_postfix)));
        harmony.Patch(
            AccessTools.Method(typeof(Utility), nameof(Utility.getFishShopStock)),
            postfix: new(typeof(ModPatches), nameof(ModPatches.Utility_getFishShopStock_postfix)));
        harmony.Patch(
            AccessTools.Method(typeof(Utility), nameof(Utility.getHospitalStock)),
            postfix: new(typeof(ModPatches), nameof(ModPatches.Utility_getHospitalStock_postfix)));
        harmony.Patch(
            AccessTools.Method(typeof(Utility), nameof(Utility.getJojaStock)),
            postfix: new(typeof(ModPatches), nameof(ModPatches.Utility_getJojaStock_postfix)));
        harmony.Patch(
            AccessTools.Method(typeof(Utility), nameof(Utility.GetQiChallengeRewardStock)),
            postfix: new(typeof(ModPatches), nameof(ModPatches.Utility_GetQiChallengeRewardStock_postfix)));
        harmony.Patch(
            AccessTools.Method(typeof(Utility), nameof(Utility.getQiShopStock)),
            postfix: new(typeof(ModPatches), nameof(ModPatches.Utility_getQiShopStock_postfix)));
        harmony.Patch(
            AccessTools.Method(typeof(Utility), nameof(Utility.getSaloonStock)),
            postfix: new(typeof(ModPatches), nameof(ModPatches.Utility_getSaloonStock_postfix)));
        harmony.Patch(
            AccessTools.Method(typeof(Utility), nameof(Utility.getTravelingMerchantStock)),
            postfix: new(typeof(ModPatches), nameof(ModPatches.Utility_getTravelingMerchantStock_postfix)));
    }

    private static IGameContentHelper GameContent => ModPatches.Instance._helper.GameContent;

    private static IReflectionHelper Reflection => ModPatches.Instance._helper.Reflection;

    private static IDictionary<string, ICustomStorage> Storages => ModPatches.Instance._storages;

    /// <summary>
    ///     Initializes <see cref="ModPatches" />.
    /// </summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <param name="manifest">A manifest to describe the mod.</param>
    /// <param name="storages">All custom chests currently loaded in the game.</param>
    /// <returns>Returns an instance of the <see cref="ModPatches" /> class.</returns>
    public static ModPatches Init(IModHelper helper, IManifest manifest, IDictionary<string, ICustomStorage> storages)
    {
        return ModPatches.Instance ??= new(helper, manifest, storages);
    }

    private static void AddToShopStock(string shop, Dictionary<ISalable, int[]> stock)
    {
        var buy = ModPatches.GameContent.Load<Dictionary<string, ShopEntry>>("furyx639.ExpandedStorage/Buy");
        var recipes = ModPatches.GameContent.Load<Dictionary<string, string>>("Data/CraftingRecipes");
        foreach (var (id, entry) in buy)
        {
            if (entry.ShopId != shop
             || (entry.IsRecipe && Game1.player.craftingRecipes.ContainsKey(id))
             || !ModPatches.Storages.ContainsKey(id))
            {
                continue;
            }

            var parentSheetIndex = 232;
            if (recipes.TryGetValue(id, out var recipeData))
            {
                var recipe = new SpanSplit(recipeData, '/');
                parentSheetIndex = int.Parse(recipe[2]);
            }

            Utility.AddStock(
                stock,
                new SObject(Vector2.Zero, parentSheetIndex, entry.IsRecipe)
                {
                    name = id,
                    modData = { ["furyx639.ExpandedStorage/Storage"] = id },
                    Stack = entry.IsRecipe ? 1 : int.MaxValue,
                },
                entry.Price);
        }
    }

    private static bool Chest_chestForAction_prefix(Chest __instance, ref bool __result, bool justCheckingForActivity)
    {
        if (justCheckingForActivity
         || !__instance.playerChest.Value
         || !__instance.modData.TryGetValue("furyx639.ExpandedStorage/Storage", out var name)
         || !ModPatches.Storages.TryGetValue(name, out var storage))
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
                          if (storage.OpenNearby != 0)
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

    private static bool Chest_draw_prefix(
        Chest __instance,
        ref int ___currentLidFrame,
        SpriteBatch spriteBatch,
        int x,
        int y,
        float alpha)
    {
        if (!__instance.playerChest.Value
         || !__instance.modData.TryGetValue("furyx639.ExpandedStorage/Storage", out var name)
         || !ModPatches.Storages.TryGetValue(name, out var storage))
        {
            return true;
        }

        var drawX = (float)x;
        var drawY = (float)y;
        if (__instance.localKickStartTile.HasValue)
        {
            drawX = Utility.Lerp(__instance.localKickStartTile.Value.X, drawX, __instance.kickProgress);
            drawY = Utility.Lerp(__instance.localKickStartTile.Value.Y, drawY, __instance.kickProgress);
            spriteBatch.Draw(
                Game1.shadowTexture,
                Game1.GlobalToLocal(Game1.viewport, new Vector2(drawX + 0.5f, drawY + 0.5f) * Game1.tileSize),
                Game1.shadowTexture.Bounds,
                Color.Black * 0.5f,
                0f,
                new(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y),
                4f,
                SpriteEffects.None,
                0.0001f);
            drawY -= (float)Math.Sin(__instance.kickProgress * Math.PI) * 0.5f;
        }

        storage.Draw(
            __instance,
            ___currentLidFrame,
            spriteBatch,
            Game1.GlobalToLocal(
                Game1.viewport,
                new Vector2(drawX, drawY - (storage.Height - storage.Depth - 16) / 16f - 1) * Game1.tileSize),
            storage.PlayerColor && !__instance.playerChoiceColor.Value.Equals(Color.Black)
                ? __instance.playerChoiceColor.Value
                : __instance.Tint,
            alpha: alpha,
            layerDepth: Math.Max(0f, ((drawY + 1f) * Game1.tileSize - 24f) / 10_000f) + drawX * 1E-05f);
        return false;
    }

    private static bool Chest_drawLocal_prefix(
        Chest __instance,
        ref int ___currentLidFrame,
        SpriteBatch spriteBatch,
        int x,
        int y,
        float alpha,
        bool local)
    {
        if (!__instance.playerChest.Value
         || !__instance.modData.TryGetValue("furyx639.ExpandedStorage/Storage", out var name)
         || !ModPatches.Storages.TryGetValue(name, out var storage))
        {
            return true;
        }

        storage.Draw(
            __instance,
            ___currentLidFrame,
            spriteBatch,
            local
                ? new(x, y - storage.GetTileHeight() + 1)
                : Game1.GlobalToLocal(Game1.viewport, new Vector2(x, y - 1) * Game1.tileSize),
            storage.PlayerColor && !__instance.playerChoiceColor.Value.Equals(Color.Black)
                ? __instance.playerChoiceColor.Value
                : __instance.Tint,
            alpha: alpha,
            layerDepth: local ? 0.89f : (y * Game1.tileSize + 4f) / 10_000f);
        return false;
    }

    private static void Chest_getLastLidFrame_postfix(Chest __instance, ref int __result)
    {
        if (!__instance.playerChest.Value
         || !__instance.modData.TryGetValue("furyx639.ExpandedStorage/Storage", out var name)
         || !ModPatches.Storages.TryGetValue(name, out var storage))
        {
            return;
        }

        __result = __instance.startingLidFrame.Value + storage.GetFrames() - 1;
    }

    private static bool Chest_performToolAction_prefix(Chest __instance, Tool? t, GameLocation location)
    {
        if (t?.getLastFarmerToUse() != Game1.player
         || t is MeleeWeapon
         || !t.isHeavyHitter()
         || !__instance.playerChest.Value
         || !__instance.isEmpty()
         || !__instance.modData.TryGetValue("furyx639.ExpandedStorage/Storage", out _))
        {
            return true;
        }

        var c = __instance.TileLocation;
        if (c.X == 0f && c.Y == 0f)
        {
            var found = false;
            foreach (var (pos, obj) in location.Objects.Pairs)
            {
                if (obj != __instance)
                {
                    continue;
                }

                c.X = (int)pos.X;
                c.Y = (int)pos.Y;
                found = true;
                break;
            }

            if (!found)
            {
                c = Game1.player.GetToolLocation() / 64f;
                c.X = (int)c.X;
                c.Y = (int)c.Y;
            }
        }

        __instance.GetMutex()
                  .RequestLock(
                      () =>
                      {
                          __instance.clearNulls();
                          if (__instance.isEmpty())
                          {
                              __instance.performRemoveAction(__instance.TileLocation, location);
                              if (location.Objects.Remove(c))
                              {
                                  var newChest = new Chest(true, Vector2.Zero, __instance.ParentSheetIndex)
                                  {
                                      Name = __instance.Name,
                                      SpecialChestType = __instance.SpecialChestType,
                                      fridge = { Value = __instance.fridge.Value },
                                      lidFrameCount = { Value = __instance.lidFrameCount.Value },
                                      playerChoiceColor = { Value = __instance.playerChoiceColor.Value },
                                  };

                                  // Copy properties
                                  newChest._GetOneFrom(__instance);

                                  // Remove tile location
                                  newChest.modData.Remove("furyx639.ExpandedStorage/X");
                                  newChest.modData.Remove("furyx639.ExpandedStorage/Y");

                                  location.debris.Add(
                                      new(
                                          __instance.ParentSheetIndex,
                                          Game1.player.GetToolLocation(),
                                          new(
                                              Game1.player.GetBoundingBox().Center.X,
                                              Game1.player.GetBoundingBox().Center.Y))
                                      {
                                          item = newChest,
                                      });
                              }
                          }

                          __instance.GetMutex().ReleaseLock();
                      });

        return false;
    }

    private static bool Chest_UpdateFarmerNearby_prefix(
        Chest __instance,
        ref bool ____farmerNearby,
        ref int ____shippingBinFrameCounter,
        ref int ___currentLidFrame,
        GameLocation location,
        bool animate)
    {
        if (!__instance.playerChest.Value
         || !__instance.modData.TryGetValue("furyx639.ExpandedStorage/Storage", out var name)
         || !ModPatches.Storages.TryGetValue(name, out var storage)
         || storage.OpenNearby == 0)
        {
            return true;
        }

        var shouldOpen = false;
        var bounds = new Rectangle(
            (int)__instance.TileLocation.X * Game1.tileSize,
            (int)__instance.TileLocation.Y * Game1.tileSize,
            storage.GetTileWidth() * Game1.tileSize,
            storage.GetTileHeight() * Game1.tileSize);
        bounds.Inflate(storage.OpenNearby * Game1.tileSize, storage.OpenNearby * Game1.tileSize);
        foreach (var farmer in location.farmers)
        {
            if (!farmer.GetBoundingBox().Intersects(bounds))
            {
                continue;
            }

            shouldOpen = true;
            break;
        }

        if (shouldOpen == ____farmerNearby)
        {
            return false;
        }

        ____farmerNearby = shouldOpen;
        ____shippingBinFrameCounter = 5;

        if (!animate)
        {
            ____shippingBinFrameCounter = -1;
            ___currentLidFrame = ____farmerNearby ? __instance.getLastLidFrame() : __instance.startingLidFrame.Value;
        }
        else if (Game1.gameMode != 6)
        {
            location.localSound(____farmerNearby ? storage.OpenNearbySound : storage.CloseNearbySound);
        }

        return false;
    }

    private static bool Chest_updateWhenCurrentLocation_prefix(
        Chest __instance,
        ref int ____shippingBinFrameCounter,
        ref bool ____farmerNearby,
        ref int ___currentLidFrame,
        ref int ___health,
        GameTime time,
        GameLocation environment)
    {
        if (!__instance.playerChest.Value
         || !__instance.modData.TryGetValue("furyx639.ExpandedStorage/Storage", out var name)
         || !ModPatches.Storages.TryGetValue(name, out var storage)
         || storage.OpenNearby == 0)
        {
            return true;
        }

        if (__instance.synchronized.Value)
        {
            __instance.openChestEvent.Poll();
        }

        if (__instance.localKickStartTile.HasValue)
        {
            if (Game1.currentLocation.Equals(environment))
            {
                if (__instance.kickProgress == 0f)
                {
                    if (Utility.isOnScreen(
                            (__instance.localKickStartTile.Value + new Vector2(0.5f, 0.5f)) * Game1.tileSize,
                            Game1.tileSize))
                    {
                        Game1.playSound("clubhit");
                    }

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
                __instance.kickProgress += (float)(time.ElapsedGameTime.TotalSeconds / 0.25f);
                if (__instance.kickProgress >= 1f)
                {
                    __instance.kickProgress = -1f;
                    __instance.localKickStartTile = null;
                }
            }
        }
        else
        {
            __instance.kickProgress = -1f;
        }

        if (___currentLidFrame == 0)
        {
            ___currentLidFrame = __instance.startingLidFrame.Value;
        }

        __instance.mutex.Update(environment);
        if (__instance.shakeTimer > 0)
        {
            __instance.shakeTimer -= time.ElapsedGameTime.Milliseconds;
            if (__instance.shakeTimer <= 0)
            {
                ___health = 10;
            }
        }

        __instance.UpdateFarmerNearby(environment);
        if (____shippingBinFrameCounter <= -1)
        {
            return false;
        }

        --____shippingBinFrameCounter;
        if (____shippingBinFrameCounter > 0)
        {
            return false;
        }

        ____shippingBinFrameCounter = 5;
        switch (____farmerNearby)
        {
            case true when ___currentLidFrame < __instance.getLastLidFrame():
                ++___currentLidFrame;
                break;
            case false when ___currentLidFrame > __instance.startingLidFrame.Value:
                --___currentLidFrame;
                break;
            default:
                ____shippingBinFrameCounter = -1;
                break;
        }

        return false;
    }

    private static void CraftingPage_layoutRecipes_postfix(CraftingPage __instance)
    {
        foreach (var page in __instance.pagesOfCraftingRecipes)
        {
            foreach (var (component, recipe) in page)
            {
                if (!ModPatches.Storages.TryGetValue(recipe.name, out var storage))
                {
                    continue;
                }

                component.texture = ModPatches.GameContent.Load<Texture2D>(storage.Image);
                component.sourceRect = new(0, 0, storage.Width, storage.Height);
                component.baseScale *= storage.GetScaleMultiplier();
                component.scale = component.baseScale;
            }
        }
    }

    private static void CraftingRecipe_constructor_postfix(CraftingRecipe __instance)
    {
        if (ModPatches.Storages.TryGetValue(__instance.name, out var storage))
        {
            __instance.description = storage.Description;
        }
    }

    private static void CraftingRecipe_createItem_postfix(CraftingRecipe __instance, ref Item __result)
    {
        if (__result is not SObject { bigCraftable.Value: true, ParentSheetIndex: 216 or 232 or 248 or 256 } obj
         || !ModPatches.Storages.TryGetValue(__instance.name, out var storage))
        {
            return;
        }

        // Craft unplaceable storages as Chest
        if (!storage.IsPlaceable && obj is not Chest)
        {
            __result = new Chest(true, obj.ParentSheetIndex);
            __result._GetOneFrom(obj);
        }

        foreach (var (key, value) in storage.ModData)
        {
            __result.modData[key] = value;
        }

        __result.modData["furyx639.ExpandedStorage/Storage"] = __instance.name;
    }

    private static void GameLocation_sandyShopStock_postfix(ref Dictionary<ISalable, int[]> __result)
    {
        ModPatches.AddToShopStock("Sandy", __result);
    }

    private static void IslandNorth_getIslandMerchantTradeStock_postfix(ref Dictionary<ISalable, int[]> __result)
    {
        ModPatches.AddToShopStock("IslandTrade", __result);
    }

    private static void Item_canStackWith_postfix(Item __instance, ref bool __result, ISalable other)
    {
        if (!__result
         || __instance is not SObject { bigCraftable.Value: true, ParentSheetIndex: 216 or 232 or 248 or 256 }
         || !__instance.modData.TryGetValue("furyx639.ExpandedStorage/Storage", out var name)
         || !ModPatches.Storages.ContainsKey(name))
        {
            return;
        }

        if (other is not SObject { bigCraftable.Value: true, ParentSheetIndex: 216 or 232 or 248 or 256 } obj
         || !obj.modData.TryGetValue("furyx639.ExpandedStorage/Storage", out var otherName)
         || !name.Equals(otherName, StringComparison.OrdinalIgnoreCase))
        {
            __result = false;
        }
    }

    private static void ItemGrabMenu_constructor_postfix(ItemGrabMenu __instance, ref Item ___sourceItem)
    {
        ModPatches.UpdateColorPicker(__instance, ___sourceItem);
    }

    private static void ItemGrabMenu_gameWindowSizeChanged_postfix(ItemGrabMenu __instance, ref Item ___sourceItem)
    {
        ModPatches.UpdateColorPicker(__instance, ___sourceItem);
    }

    private static void ItemGrabMenu_setSourceItem_postfix(ItemGrabMenu __instance, ref Item ___sourceItem)
    {
        ModPatches.UpdateColorPicker(__instance, ___sourceItem);
    }

    private static void Object_checkForAction_postfix(
        SObject __instance,
        ref bool __result,
        Farmer who,
        bool justCheckingForActivity)
    {
        if (justCheckingForActivity
         || !__instance.modData.TryGetValue("furyx639.ExpandedStorage/Storage", out _)
         || !__instance.modData.TryGetValue("furyx639.ExpandedStorage/X", out var x)
         || !__instance.modData.TryGetValue("furyx639.ExpandedStorage/Y", out var y))
        {
            return;
        }

        var tile = new Vector2(int.Parse(x), int.Parse(y));
        if (!who.currentLocation.Objects.TryGetValue(tile, out var obj) || obj is not Chest chest)
        {
            return;
        }

        __result = chest.checkForAction(who);
    }

    private static bool Object_draw_prefix(SObject __instance, SpriteBatch spriteBatch, int x, int y, float alpha)
    {
        if (!__instance.modData.TryGetValue("furyx639.ExpandedStorage/Storage", out var name)
         || !ModPatches.Storages.TryGetValue(name, out var storage))
        {
            return true;
        }

        if (__instance.modData.ContainsKey("furyx639.ExpandedStorage/X")
         || __instance.modData.ContainsKey("furyx639.ExpandedStorage/Y"))
        {
            return false;
        }

        storage.Draw(
            __instance,
            0,
            spriteBatch,
            Game1.GlobalToLocal(
                Game1.viewport,
                new Vector2(x, y - (storage.Height - storage.Depth - 16) / 16f - 1) * Game1.tileSize),
            alpha: alpha,
            layerDepth: Math.Max(0f, ((y + 1f) * Game1.tileSize - 24f) / 10_000f) + x * 1E-05f);
        return false;
    }

    [SuppressMessage("ReSharper", "PossibleLossOfFraction", Justification = "Harmony")]
    private static bool Object_drawInMenu_prefix(
        SObject __instance,
        SpriteBatch spriteBatch,
        Vector2 location,
        float scaleSize,
        float transparency,
        float layerDepth,
        StackDrawType drawStackNumber,
        Color color)
    {
        if (!__instance.modData.TryGetValue("furyx639.ExpandedStorage/Storage", out var name)
         || !ModPatches.Storages.TryGetValue(name, out var storage))
        {
            return true;
        }

        if (__instance.IsRecipe)
        {
            transparency = 0.5f;
            scaleSize *= 0.75f;
        }

        switch (__instance)
        {
            case Chest chest:
                storage.Draw(
                    __instance,
                    0,
                    spriteBatch,
                    location + new Vector2(32f, 32f),
                    storage.PlayerColor && !chest.playerChoiceColor.Value.Equals(Color.Black)
                        ? chest.playerChoiceColor.Value
                        : chest.Tint,
                    new(storage.Width / 2, storage.Height / 2),
                    scaleSize: storage.GetScaleMultiplier() * scaleSize < 0.2
                        ? storage.GetScaleMultiplier() * scaleSize
                        : storage.GetScaleMultiplier() * scaleSize / 2f,
                    layerDepth: layerDepth);
                break;

            default:
                storage.Draw(
                    __instance,
                    0,
                    spriteBatch,
                    location + new Vector2(32f, 32f),
                    color * transparency,
                    new(storage.Width / 2, storage.Height / 2),
                    scaleSize: storage.GetScaleMultiplier() * scaleSize < 0.2
                        ? storage.GetScaleMultiplier() * scaleSize
                        : storage.GetScaleMultiplier() * scaleSize / 2f,
                    layerDepth: layerDepth);
                break;
        }

        switch (__instance.IsRecipe)
        {
            case false when drawStackNumber is StackDrawType.Draw
                         && __instance.Stack > 1
                         && __instance.Stack != int.MaxValue
                         && scaleSize > 0.3:
                Utility.drawTinyDigits(
                    __instance.Stack,
                    spriteBatch,
                    location
                  + new Vector2(
                        Game1.tileSize
                      - Utility.getWidthOfTinyDigitString(__instance.Stack, 3f * scaleSize)
                      + 3f * scaleSize,
                        Game1.tileSize - 18f * scaleSize + 2f),
                    3f * scaleSize,
                    1f,
                    color);
                break;
            case true:
                spriteBatch.Draw(
                    Game1.objectSpriteSheet,
                    location + new Vector2(16f, 16f),
                    Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 451, 16, 16),
                    color,
                    0f,
                    Vector2.Zero,
                    3f,
                    SpriteEffects.None,
                    layerDepth + 0.0001f);
                break;
        }

        return false;
    }

    private static bool Object_drawPlacementBounds_prefix(
        SObject __instance,
        SpriteBatch spriteBatch,
        GameLocation location)
    {
        if (!__instance.isPlaceable()
         || !__instance.modData.TryGetValue("furyx639.ExpandedStorage/Storage", out var name)
         || !ModPatches.Storages.TryGetValue(name, out var storage)
         || (storage.GetTileHeight() == 1 && storage.GetTileWidth() == 1))
        {
            return true;
        }

        var x = (int)Game1.GetPlacementGrabTile().X * Game1.tileSize;
        var y = (int)Game1.GetPlacementGrabTile().Y * Game1.tileSize;
        Game1.isCheckingNonMousePlacement = !Game1.IsPerformingMousePlacement();
        if (Game1.isCheckingNonMousePlacement)
        {
            var nearbyValidPlacementPosition =
                Utility.GetNearbyValidPlacementPosition(Game1.player, location, __instance, x, y);
            x = (int)nearbyValidPlacementPosition.X;
            y = (int)nearbyValidPlacementPosition.Y;
        }

        if (Utility.isThereAnObjectHereWhichAcceptsThisItem(location, __instance, x, y))
        {
            return false;
        }

        var canPlaceHere = Utility.playerCanPlaceItemHere(location, __instance, x, y, Game1.player)
                        || (Utility.isThereAnObjectHereWhichAcceptsThisItem(location, __instance, x, y)
                         && Utility.withinRadiusOfPlayer(x, y, 1, Game1.player));
        Game1.isCheckingNonMousePlacement = false;
        for (var x_offset = 0; x_offset < storage.GetTileWidth(); ++x_offset)
        {
            for (var y_offset = 0; y_offset < storage.GetTileDepth(); ++y_offset)
            {
                spriteBatch.Draw(
                    Game1.mouseCursors,
                    new(
                        (x / Game1.tileSize + x_offset) * Game1.tileSize - Game1.viewport.X,
                        (y / Game1.tileSize + y_offset) * Game1.tileSize - Game1.viewport.Y),
                    new Rectangle(canPlaceHere ? 194 : 210, 388, 16, 16),
                    Color.White,
                    0f,
                    Vector2.Zero,
                    Game1.pixelZoom,
                    SpriteEffects.None,
                    0.01f);
            }
        }

        __instance.draw(spriteBatch, x / Game1.tileSize, y / Game1.tileSize, 0.5f);
        return false;
    }

    private static bool Object_drawWhenHeld_prefix(
        SObject __instance,
        SpriteBatch spriteBatch,
        Vector2 objectPosition,
        Farmer f)
    {
        if (!__instance.modData.TryGetValue("furyx639.ExpandedStorage/Storage", out var name)
         || !ModPatches.Storages.TryGetValue(name, out var storage))
        {
            return true;
        }

        var posAdj = storage.GetTileHeight() == 2 && storage.GetTileWidth() == 1
            ? Vector2.Zero
            : new Vector2(storage.GetTileWidth() - 1, storage.GetTileHeight() - 2) * Game1.tileSize / 2f;

        switch (__instance)
        {
            case Chest chest:
                storage.Draw(
                    __instance,
                    ModPatches.Reflection.GetField<int>(chest, "currentLidFrame").GetValue(),
                    spriteBatch,
                    objectPosition - posAdj,
                    storage.PlayerColor && !chest.playerChoiceColor.Value.Equals(Color.Black)
                        ? chest.playerChoiceColor.Value
                        : chest.Tint,
                    layerDepth: Math.Max(0f, (f.getStandingY() + 3) / 10_000f));
                break;

            default:
                storage.Draw(
                    __instance,
                    0,
                    spriteBatch,
                    objectPosition - posAdj,
                    layerDepth: Math.Max(0f, (f.getStandingY() + 3) / 10_000f));
                break;
        }

        return false;
    }

    private static void Object_getDescription_postfix(SObject __instance, ref string __result)
    {
        if (__instance.modData.TryGetValue("furyx639.ExpandedStorage/Storage", out var name)
         && ModPatches.Storages.TryGetValue(name, out var storage))
        {
            __result = storage.Description;
        }
    }

    private static void Object_getOne_postfix(SObject __instance, ref Item __result)
    {
        if (__result is not SObject { bigCraftable.Value: true, ParentSheetIndex: 216 or 232 or 248 or 256 } obj
         || !ModPatches.Storages.TryGetValue(__instance.name, out var storage))
        {
            return;
        }

        // Craft unplaceable storages as Chest
        if (!storage.IsPlaceable && obj is not Chest)
        {
            __result = new Chest(true, obj.ParentSheetIndex);
            __result._GetOneFrom(obj);
        }

        foreach (var (key, value) in storage.ModData)
        {
            __result.modData[key] = value;
        }

        __result.modData["furyx639.ExpandedStorage/Storage"] = __instance.name;
    }

    private static void Object_isPlaceable_postfix(SObject __instance, ref bool __result)
    {
        if (__result
         && __instance.modData.TryGetValue("furyx639.ExpandedStorage/Storage", out var name)
         && ModPatches.Storages.TryGetValue(name, out var storage)
         && !storage.IsPlaceable)
        {
            __result = false;
        }
    }

    private static void Object_loadDisplayName_postfix(SObject __instance, ref string __result)
    {
        if (__instance.modData.TryGetValue("furyx639.ExpandedStorage/Storage", out var name)
         && ModPatches.Storages.TryGetValue(name, out var storage))
        {
            __result = storage.DisplayName;
        }
    }

    private static void Object_performRemoveAction_postfix(SObject __instance, GameLocation environment)
    {
        if (!__instance.modData.TryGetValue("furyx639.ExpandedStorage/Storage", out var name)
         || !ModPatches.Storages.TryGetValue(name, out var storage))
        {
            return;
        }

        var c = __instance.modData.TryGetValue("furyx639.ExpandedStorage/X", out var x)
             && __instance.modData.TryGetValue("furyx639.ExpandedStorage/Y", out var y)
            ? new(int.Parse(x), int.Parse(y))
            : __instance.TileLocation;

        if (c.X == 0f && c.Y == 0f)
        {
            var found = false;
            foreach (var (pos, obj) in environment.Objects.Pairs)
            {
                if (obj != __instance)
                {
                    continue;
                }

                c.X = (int)pos.X;
                c.Y = (int)pos.Y;
                found = true;
                break;
            }

            if (!found)
            {
                c = Game1.player.GetToolLocation() / 64f;
                c.X = (int)c.X;
                c.Y = (int)c.Y;
            }
        }

        for (var xOffset = 0; xOffset < storage.GetTileWidth(); ++xOffset)
        {
            for (var yOffset = 0; yOffset < storage.GetTileDepth(); ++yOffset)
            {
                var currentTile = c + new Vector2(xOffset, yOffset);
                if (!c.Equals(currentTile))
                {
                    environment.Objects.Remove(currentTile);
                }
            }
        }
    }

    private static bool Object_performToolAction_prefix(
        SObject __instance,
        ref bool __result,
        Tool t,
        GameLocation location)
    {
        if (__instance is Chest
         || !__instance.modData.ContainsKey("furyx639.ExpandedStorage/Storage")
         || !__instance.modData.TryGetValue("furyx639.ExpandedStorage/X", out var x)
         || !__instance.modData.TryGetValue("furyx639.ExpandedStorage/Y", out var y))
        {
            return true;
        }

        var tile = new Vector2(int.Parse(x), int.Parse(y));
        if (!location.Objects.TryGetValue(tile, out var obj) || obj is not Chest chest)
        {
            return true;
        }

        __result = chest.performToolAction(t, location);
        return false;
    }

    [SuppressMessage("ReSharper", "PossibleLossOfFraction", Justification = "Harmony")]
    private static void Object_placementAction_postfix(
        SObject __instance,
        ref bool __result,
        GameLocation location,
        int x,
        int y)
    {
        var tile = new Vector2(x / Game1.tileSize, y / Game1.tileSize);
        if (!__instance.modData.TryGetValue("furyx639.ExpandedStorage/Storage", out var name)
         || !ModPatches.Storages.TryGetValue(name, out var storage)
         || !location.Objects.TryGetValue(tile, out var placed)
         || placed is not Chest chest)
        {
            return;
        }

        // Remove unplaceable object
        if (!storage.IsPlaceable)
        {
            location.Objects.Remove(tile);
            __result = false;
            return;
        }

        chest._GetOneFrom(__instance);
        foreach (var (key, value) in storage.ModData)
        {
            chest.modData[key] = value;
        }

        chest.modData["furyx639.ExpandedStorage/Storage"] = name;
        if (storage.GetTileHeight() <= 2 && storage.GetTileWidth() <= 1)
        {
            return;
        }

        // Place additional objects
        for (var xOffset = 0; xOffset < storage.GetTileWidth(); ++xOffset)
        {
            for (var yOffset = 0; yOffset < storage.Depth / 16f; ++yOffset)
            {
                var currentTile = tile + new Vector2(xOffset, yOffset);
                if (!location.Objects.TryGetValue(currentTile, out var obj))
                {
                    obj = new(currentTile, __instance.ParentSheetIndex);
                    obj._GetOneFrom(__instance);
                    location.Objects[currentTile] = obj;
                }

                obj.modData["furyx639.ExpandedStorage/X"] = tile.X.ToString(CultureInfo.InvariantCulture);
                obj.modData["furyx639.ExpandedStorage/Y"] = tile.Y.ToString(CultureInfo.InvariantCulture);
            }
        }
    }

    private static void SeedShop_shopStock_postfix(ref Dictionary<ISalable, int[]> __result)
    {
        ModPatches.AddToShopStock("SeedShop", __result);
    }

    private static void Sewer_getShadowShopStock_postfix(ref Dictionary<ISalable, int[]> __result)
    {
        ModPatches.AddToShopStock("ShadowShop", __result);
    }

    private static void ShopMenu_constructor_prefix(Dictionary<ISalable, int[]> itemPriceAndStock, string who)
    {
        if (who != "VolcanoShop")
        {
            return;
        }

        ModPatches.AddToShopStock("VolcanoShop", itemPriceAndStock);
    }

    private static void UpdateColorPicker(ItemGrabMenu itemGrabMenu, Item sourceItem)
    {
        if (itemGrabMenu.context is not Chest chest
         || sourceItem is not Chest item
         || !chest.modData.TryGetValue("furyx639.ExpandedStorage/Storage", out var name)
         || !ModPatches.Storages.TryGetValue(name, out var storage)
         || (storage.PlayerColor && itemGrabMenu.chestColorPicker is not null)
         || (!storage.PlayerColor && itemGrabMenu.chestColorPicker is null))
        {
            return;
        }

        if (!storage.PlayerColor)
        {
            itemGrabMenu.chestColorPicker = null;
            itemGrabMenu.colorPickerToggleButton = null;
            itemGrabMenu.discreteColorPickerCC = null;
            itemGrabMenu.RepositionSideButtons();
            return;
        }

        var itemToDrawAsColored = new Chest(true, item.ParentSheetIndex);
        itemGrabMenu.chestColorPicker = new(
            itemGrabMenu.xPositionOnScreen,
            itemGrabMenu.yPositionOnScreen - Game1.tileSize - IClickableMenu.borderWidth * 2,
            0,
            itemToDrawAsColored);

        itemGrabMenu.chestColorPicker.colorSelection =
            itemGrabMenu.chestColorPicker.getSelectionFromColor(item.playerChoiceColor.Value);

        itemToDrawAsColored.playerChoiceColor.Value =
            itemGrabMenu.chestColorPicker.getColorFromSelection(itemGrabMenu.chestColorPicker.colorSelection);
        itemGrabMenu.colorPickerToggleButton = new(
            new(
                itemGrabMenu.xPositionOnScreen + itemGrabMenu.width,
                itemGrabMenu.yPositionOnScreen + itemGrabMenu.height / 3 - Game1.tileSize - 160,
                Game1.tileSize,
                Game1.tileSize),
            Game1.mouseCursors,
            new(119, 469, 16, 16),
            Game1.pixelZoom)
        {
            hoverText = Game1.content.LoadString("Strings\\UI:Toggle_ColorPicker"),
        };
        itemGrabMenu.RepositionSideButtons();
        itemGrabMenu.discreteColorPickerCC = new();
        for (var i = 0; i < itemGrabMenu.chestColorPicker.totalColors; i++)
        {
            itemGrabMenu.discreteColorPickerCC.Add(
                new(
                    new(
                        itemGrabMenu.chestColorPicker.xPositionOnScreen + IClickableMenu.borderWidth / 2 + i * 9 * 4,
                        itemGrabMenu.chestColorPicker.yPositionOnScreen + IClickableMenu.borderWidth / 2,
                        36,
                        28),
                    string.Empty)
                {
                    myID = i + 4343,
                    rightNeighborID = i < itemGrabMenu.chestColorPicker.totalColors - 1 ? i + 4343 + 1 : -1,
                    leftNeighborID = i > 0 ? i + 4343 - 1 : -1,
                    downNeighborID =
                        itemGrabMenu.ItemsToGrabMenu != null && itemGrabMenu.ItemsToGrabMenu.inventory.Count > 0
                            ? 53910
                            : 0,
                });
        }
    }

    private static void Utility_getAdventureShopStock_postfix(ref Dictionary<ISalable, int[]> __result)
    {
        ModPatches.AddToShopStock("AdventureShop", __result);
    }

    private static void Utility_getBlacksmithStock_postfix(ref Dictionary<ISalable, int[]> __result)
    {
        ModPatches.AddToShopStock("Blacksmith", __result);
    }

    private static void Utility_getCarpenterStock_postfix(ref Dictionary<ISalable, int[]> __result)
    {
        ModPatches.AddToShopStock("Carpenter", __result);
    }

    private static void Utility_getDwarfShopStock_postfix(ref Dictionary<ISalable, int[]> __result)
    {
        ModPatches.AddToShopStock("Dwarf", __result);
    }

    private static void Utility_getFishShopStock_postfix(ref Dictionary<ISalable, int[]> __result)
    {
        ModPatches.AddToShopStock("FishShop", __result);
    }

    private static void Utility_getHospitalStock_postfix(ref Dictionary<ISalable, int[]> __result)
    {
        ModPatches.AddToShopStock("Hospital", __result);
    }

    private static void Utility_getJojaStock_postfix(ref Dictionary<ISalable, int[]> __result)
    {
        ModPatches.AddToShopStock("Joja", __result);
    }

    private static void Utility_GetQiChallengeRewardStock_postfix(ref Dictionary<ISalable, int[]> __result)
    {
        var buy = ModPatches.GameContent.Load<Dictionary<string, ShopEntry>>("furyx639.ExpandedStorage/Buy");
        var recipes = ModPatches.GameContent.Load<Dictionary<string, string>>("Data/CraftingRecipes");
        foreach (var (id, entry) in buy)
        {
            if (entry.ShopId != "QiGems"
             || (entry.IsRecipe && Game1.player.craftingRecipes.ContainsKey(id))
             || !ModPatches.Storages.ContainsKey(id))
            {
                continue;
            }

            var parentSheetIndex = 232;
            if (recipes.TryGetValue(id, out var recipeData))
            {
                var recipe = new SpanSplit(recipeData, '/');
                parentSheetIndex = int.Parse(recipe[2]);
            }

            __result.Add(
                new SObject(Vector2.Zero, parentSheetIndex, entry.IsRecipe)
                {
                    name = id,
                    modData = { ["furyx639.ExpandedStorage/Storage"] = id },
                    Stack = entry.IsRecipe ? 1 : int.MaxValue,
                },
                new[]
                {
                    0,
                    entry.IsRecipe ? 1 : int.MaxValue,
                    858,
                    entry.Price,
                });
        }
    }

    private static void Utility_getQiShopStock_postfix(ref Dictionary<ISalable, int[]> __result)
    {
        ModPatches.AddToShopStock("Casino", __result);
    }

    private static void Utility_getSaloonStock_postfix(ref Dictionary<ISalable, int[]> __result)
    {
        ModPatches.AddToShopStock("Saloon", __result);
    }

    private static void Utility_getTravelingMerchantStock_postfix(ref Dictionary<ISalable, int[]> __result)
    {
        ModPatches.AddToShopStock("Traveler", __result);
    }

    private static void Utility_isWithinTileWithLeeway_postfix(ref bool __result, int x, int y, Item item)
    {
        if (__result
         || !item.modData.TryGetValue("furyx639.ExpandedStorage/Storage", out var name)
         || !ModPatches.Storages.TryGetValue(name, out var storage)
         || (storage.GetTileHeight() == 2 && storage.GetTileWidth() == 1))
        {
            return;
        }

        // Furniture.IsCloseEnoughToFarmer
        var rect = new Rectangle(
            x,
            y,
            storage.GetTileWidth() * Game1.tileSize,
            storage.GetTileHeight() * Game1.tileSize);
        rect.Inflate(96, 96);
        if (rect.Contains(Game1.player.getStandingPosition().ToPoint()))
        {
            __result = true;
        }
    }

    [SuppressMessage("ReSharper", "PossibleLossOfFraction", Justification = "Harmony")]
    private static void Utility_playerCanPlaceItemHere_postfix(
        ref bool __result,
        GameLocation location,
        Item item,
        int x,
        int y)
    {
        if (!__result
         || !item.modData.TryGetValue("furyx639.ExpandedStorage/Storage", out var name)
         || !ModPatches.Storages.TryGetValue(name, out var storage))
        {
            return;
        }

        // Furniture.canBePlacedHere
        var tile = new Vector2(x / Game1.tileSize, y / Game1.tileSize);
        for (var xOffset = 0; xOffset < storage.GetTileWidth(); ++xOffset)
        {
            for (var yOffset = 0; yOffset < storage.GetTileDepth(); ++yOffset)
            {
                var currentTile = tile + new Vector2(xOffset, yOffset);
                if (!location.Objects.ContainsKey(currentTile)
                 && location.getLargeTerrainFeatureAt((int)currentTile.X, (int)currentTile.Y) is null
                 && (!location.terrainFeatures.ContainsKey(currentTile)
                  || location.terrainFeatures[currentTile] is not Tree)
                 && !location.isTerrainFeatureAt((int)currentTile.X, (int)currentTile.Y)
                 && item.canBePlacedHere(location, currentTile))
                {
                    continue;
                }

                __result = false;
                return;
            }
        }

        var boundingBox = new Rectangle(
            (int)tile.X * Game1.tileSize,
            (int)tile.Y * Game1.tileSize,
            storage.Width,
            storage.Height);
        foreach (var farmer in location.farmers)
        {
            if (farmer.GetBoundingBox().Intersects(boundingBox))
            {
                __result = false;
                return;
            }
        }

        foreach (var character in location.characters)
        {
            if (character.GetBoundingBox().Intersects(boundingBox))
            {
                __result = false;
                return;
            }
        }
    }
}