using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Harmony;
using ImJustMatt.Common.PatternPatches;
using ImJustMatt.ExpandedStorage.Framework.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;

// ReSharper disable InconsistentNaming

namespace ImJustMatt.ExpandedStorage.Framework.Patches
{
    internal class ObjectPatch : Patch<ModConfig>
    {
        private static readonly HashSet<string> ExcludeModDataKeys = new();

        public ObjectPatch(IMonitor monitor, ModConfig config) : base(monitor, config)
        {
        }

        internal static void AddExclusion(string modDataKey)
        {
            if (!ExcludeModDataKeys.Contains(modDataKey))
                ExcludeModDataKeys.Add(modDataKey);
        }

        protected internal override void Apply(HarmonyInstance harmony)
        {
            harmony.Patch(
                AccessTools.Method(typeof(Object), nameof(Object.placementAction)),
                new HarmonyMethod(GetType(), nameof(PlacementActionPrefix))
            );

            harmony.Patch(
                AccessTools.Method(typeof(Object), nameof(Object.drawWhenHeld)),
                new HarmonyMethod(GetType(), nameof(DrawWhenHeldPrefix))
            );

            harmony.Patch(
                AccessTools.Method(typeof(Object), nameof(Object.drawPlacementBounds)),
                new HarmonyMethod(GetType(), nameof(DrawPlacementBoundsPrefix))
            );
        }

        public static bool PlacementActionPrefix(Object __instance, ref bool __result, GameLocation location, int x, int y, Farmer who)
        {
            var config = ExpandedStorage.GetConfig(__instance);
            if (config == null)
                return true;

            if (!config.IsPlaceable)
            {
                __result = false;
                return false;
            }

            // Verify pos is not already occupied
            var pos = new Vector2(x, y) / 64f;
            pos.X = (int) pos.X;
            pos.Y = (int) pos.Y;

            if (location.objects.ContainsKey(pos))
            {
                Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.13053"));
                __result = false;
                return false;
            }

            if (location is MineShaft || location is VolcanoDungeon)
            {
                Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.13053"));
                __result = false;
                return false;
            }

            if (config.IsFridge)
            {
                if (location is not FarmHouse && location is not IslandFarmHouse)
                {
                    Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.13053"));
                    __result = false;
                    return false;
                }

                if (location is FarmHouse {upgradeLevel: < 1})
                {
                    Game1.showRedMessage(Game1.content.LoadString("Strings\\UI:MiniFridge_NoKitchen"));
                    __result = false;
                    return false;
                }
            }

            __instance.owner.Value = who?.UniqueMultiplayerID ?? Game1.player.UniqueMultiplayerID;
            __instance.modData["furyx639.ExpandedStorage/X"] = pos.X.ToString(CultureInfo.InvariantCulture);
            __instance.modData["furyx639.ExpandedStorage/Y"] = pos.Y.ToString(CultureInfo.InvariantCulture);

            // Get instance of object to place
            var chest = __instance.ToChest(config);
            chest.shakeTimer = 50;
            chest.TileLocation = pos;

            // Place object at location
            location.objects.Add(pos, chest);
            location.playSound(config.PlaceSound);

            // Place clones at additional tile locations
            if (config.Texture != null)
            {
                var width = config.Width / 16;
                var height = (config.Depth == 0 ? config.Height - 16 : config.Depth) / 16;
                for (var i = 0; i < width; i++)
                {
                    for (var j = 0; j < height; j++)
                    {
                        if (i == 0 && j == 0)
                            continue;
                        location.objects.Add(pos + new Vector2(i, j), chest);
                    }
                }
            }

            __result = true;
            return false;
        }

        public static bool DrawWhenHeldPrefix(Object __instance, SpriteBatch spriteBatch, Vector2 objectPosition, Farmer f)
        {
            var config = ExpandedStorage.GetConfig(__instance);
            if (config == null || __instance is not Chest chest || __instance.modData.Keys.Any(ExcludeModDataKeys.Contains))
                return true;

            if (config.Texture != null)
            {
                objectPosition.X -= config.Width * 2f - 32;
                objectPosition.Y -= config.Height * 2f - 64;
            }

            chest.Draw(config, spriteBatch, objectPosition, Vector2.Zero);
            return false;
        }

        public static bool DrawPlacementBoundsPrefix(Object __instance, SpriteBatch spriteBatch, GameLocation location)
        {
            var config = ExpandedStorage.GetConfig(__instance);
            if (config?.Texture == null || __instance.modData.Keys.Any(ExcludeModDataKeys.Contains))
                return true;

            var tile = 64 * Game1.GetPlacementGrabTile();
            var width = config.Width / 16;
            var height = (config.Depth == 0 ? config.Height - 16 : config.Depth) / 16;

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

            for (var i = 0; i < width; i++)
            {
                for (var j = 0; j < height; j++)
                {
                    spriteBatch.Draw(Game1.mouseCursors,
                        new Vector2((x / 64 + i) * 64 - Game1.viewport.X, (y / 64 + j) * 64 - Game1.viewport.Y),
                        new Rectangle(canPlaceHere ? 194 : 210, 388, 16, 16),
                        Color.White,
                        0f,
                        Vector2.Zero,
                        4f,
                        SpriteEffects.None,
                        0.01f);
                }
            }

            __instance.draw(spriteBatch, x / 64, y / 64, 0.5f);
            return false;
        }
    }
}