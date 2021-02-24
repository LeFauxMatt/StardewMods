using System;
using Harmony;
using ImJustMatt.Common.PatternPatches;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;

namespace ImJustMatt.ExpandedStorage.Framework.Patches
{
    internal class UtilityPatch : Patch<ModConfig>
    {
        private static IReflectedMethod _itemCanBePlaced;

        public UtilityPatch(IMonitor monitor, ModConfig config, IReflectionHelper reflection) : base(monitor, config)
        {
            _itemCanBePlaced = reflection.GetMethod(typeof(Utility), "itemCanBePlaced");
        }

        private static bool ItemCanBePlaced(GameLocation location, Vector2 tileLocation, Item item) =>
            _itemCanBePlaced.Invoke<bool>(location, tileLocation, item);

        protected internal override void Apply(HarmonyInstance harmony)
        {
            harmony.Patch(
                AccessTools.Method(typeof(Utility), nameof(Utility.playerCanPlaceItemHere)),
                new HarmonyMethod(GetType(), nameof(PlayerCanPlaceItemHerePrefix))
            );
        }

        public static bool PlayerCanPlaceItemHerePrefix(ref bool __result, GameLocation location, Item item, int x, int y, Farmer f)
        {
            var config = ExpandedStorage.GetConfig(item);
            if (config?.Texture == null)
                return true;

            x = 64 * (x / 64);
            y = 64 * (y / 64);

            if (Utility.isPlacementForbiddenHere(location) || item == null || Game1.eventUp || f.bathingClothes.Value || f.onBridge.Value)
            {
                __result = false;
                return false;
            }

            var width = config.Width / 16;
            var height = (config.Depth == 0 ? config.Height - 16 : config.Depth) / 16;

            // Is Within Tile With Leeway
            if (!Utility.withinRadiusOfPlayer(x, y, Math.Max(width, height), f))
            {
                __result = false;
                return false;
            }

            var rect = new Rectangle(x, y, width * 64, height * 64);

            // Position intersects with farmer
            foreach (var farmer in location.farmers)
            {
                if (farmer.GetBoundingBox().Intersects(rect))
                {
                    __result = false;
                    return false;
                }
            }

            // Is Close Enough to Farmer
            rect.Inflate(32, 32);
            if (!rect.Intersects(f.GetBoundingBox()))
            {
                __result = false;
                return false;
            }

            for (var i = 0; i < width; i++)
            {
                for (var j = 0; j < height; j++)
                {
                    var tileLocation = new Vector2(x / 64 + i, y / 64 + j);

                    // Item cannot be placed here
                    if (!item.canBePlacedHere(location, tileLocation))
                    {
                        __result = false;
                        return false;
                    }

                    // Space is already occupied
                    if (location.getObjectAtTile((int) tileLocation.X, (int) tileLocation.Y) != null)
                    {
                        __result = false;
                        return false;
                    }

                    // Invalid tile placement for item
                    if (!location.isTilePlaceable(tileLocation, item))
                    {
                        __result = false;
                        return false;
                    }
                }
            }

            __result = true;
            return false;
        }
    }
}