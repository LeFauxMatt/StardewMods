namespace StardewMods.GarbageDay;

using System;
using System.Linq;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Objects;

/// <summary>
///     Harmony Patches for GarbageDay.
/// </summary>
internal sealed class ModPatches
{
    private static ModPatches? instance;

    private ModPatches(IManifest manifest)
    {
        var harmony = new Harmony(manifest.UniqueID);
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
            AccessTools.Method(typeof(Chest), nameof(Chest.performToolAction)),
            new(typeof(ModPatches), nameof(ModPatches.Chest_performToolAction_prefix)));
        harmony.Patch(
            AccessTools.Method(typeof(Chest), nameof(Chest.UpdateFarmerNearby)),
            new(typeof(ModPatches), nameof(ModPatches.Chest_UpdateFarmerNearby_prefix)));
        harmony.Patch(
            AccessTools.Method(typeof(Chest), nameof(Chest.updateWhenCurrentLocation)),
            new(typeof(ModPatches), nameof(ModPatches.Chest_updateWhenCurrentLocation_prefix)));
    }

    /// <summary>
    ///     Initializes <see cref="ModPatches" />.
    /// </summary>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <returns>Returns an instance of the <see cref="ModPatches" /> class.</returns>
    public static ModPatches Init(IManifest manifest) => ModPatches.instance ??= new(manifest);

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
        if (!__instance.modData.ContainsKey("furyx639.GarbageDay/WhichCan"))
        {
            return true;
        }

        var texture = Game1.content.Load<Texture2D>("furyx639.GarbageDay/Texture");
        var currentLidFrame = ___currentLidFrame % 3;
        var layerDepth = Math.Max(0.0f, (((y + 1f) * 64f) - 24f) / 10000f) + (x * 1E-05f);
        if (__instance.playerChoiceColor.Value.Equals(Color.Black))
        {
            spriteBatch.Draw(
                texture,
                Game1.GlobalToLocal(Game1.viewport, new Vector2(x, y - 1) * Game1.tileSize),
                new Rectangle(currentLidFrame * 16, 0, 16, 32),
                Color.White * alpha,
                0f,
                Vector2.Zero,
                Game1.pixelZoom,
                SpriteEffects.None,
                layerDepth + ((1 + layerDepth) * 1E-05f));
            return false;
        }

        spriteBatch.Draw(
            texture,
            Game1.GlobalToLocal(Game1.viewport, new Vector2(x, y - 1) * Game1.tileSize),
            new Rectangle(currentLidFrame * 16, 64, 16, 32),
            Color.White * alpha,
            0f,
            Vector2.Zero,
            Game1.pixelZoom,
            SpriteEffects.None,
            layerDepth * 1E-05f);

        spriteBatch.Draw(
            texture,
            Game1.GlobalToLocal(Game1.viewport, new Vector2(x, y - 1) * Game1.tileSize),
            new Rectangle(currentLidFrame * 16, 32, 16, 32),
            __instance.playerChoiceColor.Value * alpha,
            0f,
            Vector2.Zero,
            Game1.pixelZoom,
            SpriteEffects.None,
            layerDepth * 1E-05f);
        return false;
    }

    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Harmony")]
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static bool Chest_performToolAction_prefix(Chest __instance) => !__instance.modData.ContainsKey("furyx639.GarbageDay/WhichCan");

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static bool Chest_UpdateFarmerNearby_prefix(
        Chest __instance,
        ref bool ____farmerNearby,
        ref int ____shippingBinFrameCounter,
        ref int ___currentLidFrame,
        bool animate)
    {
        if (!__instance.modData.ContainsKey("furyx639.GarbageDay/WhichCan"))
        {
            return true;
        }

        var location = __instance.Location;
        var shouldOpen = location.farmers.Any(
            farmer => Math.Abs(farmer.Tile.X - __instance.TileLocation.X) <= 1f
                && Math.Abs(farmer.Tile.Y - __instance.TileLocation.Y) <= 1f);
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
            location.localSound("trashcanlid");
        }

        return false;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static bool Chest_updateWhenCurrentLocation_prefix(
        Chest __instance,
        ref int ____shippingBinFrameCounter,
        ref bool ____farmerNearby,
        ref int ___currentLidFrame)
    {
        if (!__instance.modData.ContainsKey("furyx639.GarbageDay/WhichCan"))
        {
            return true;
        }

        if (__instance.synchronized.Value)
        {
            __instance.openChestEvent.Poll();
        }

        var environment = __instance.Location;
        __instance.mutex.Update(environment);

        __instance.UpdateFarmerNearby();
        if (____shippingBinFrameCounter > -1)
        {
            --____shippingBinFrameCounter;
            if (____shippingBinFrameCounter <= 0)
            {
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
            }
        }

        if (Game1.activeClickableMenu is null && __instance.GetMutex().IsLockHeld())
        {
            __instance.GetMutex().ReleaseLock();
        }

        return false;
    }
}
