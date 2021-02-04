using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using ExpandedStorage.Framework.Extensions;
using Harmony;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;

namespace ExpandedStorage.Framework.Patches
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    internal class ObjectPatch : HarmonyPatch
    {
        private static readonly HashSet<string> ExcludeModDataKeys = new()
        {
            "aedenthorn.CustomChestTypes/IsCustomChest"
        };

        internal ObjectPatch(IMonitor monitor, ModConfig config)
            : base(monitor, config) { }
        
        protected internal override void Apply(HarmonyInstance harmony)
        {
            harmony.Patch(
                AccessTools.Method(typeof(StardewValley.Object), nameof(StardewValley.Object.placementAction)),
                new HarmonyMethod(GetType(), nameof(PlacementAction))
            );

            if (!Config.AllowCarryingChests)
                return;
            
            harmony.Patch(
                original: AccessTools.Method(typeof(StardewValley.Object), nameof(StardewValley.Object.getDescription)),
                postfix: new HarmonyMethod(GetType(), nameof(getDescription_Postfix))
            );

            harmony.Patch(
                original: AccessTools.Method(typeof(StardewValley.Object), nameof(StardewValley.Object.drawWhenHeld)),
                new HarmonyMethod(GetType(), nameof(drawWhenHeld_Prefix))
            );
        }
        
        public static bool PlacementAction(StardewValley.Object __instance, ref bool __result, GameLocation location, int x, int y, Farmer who)
        {
            var config = ExpandedStorage.GetConfig(__instance);
            
            // Disallow non-placeable storages
            if (config != null && !config.IsPlaceable)
            {
                __result = false;
                return false;
            }
            
            if (config == null)
                return true;
            
            var pos = new Vector2(x, y) / 64f;
            pos.X = (int) pos.X;
            pos.Y = (int) pos.Y;
            if (location.objects.ContainsKey(pos) || location is MineShaft || location is VolcanoDungeon)
            {
                Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.13053"));
                __result = false;
                return false;
            }

            // Place Expanded Storage Chest
            var chest = __instance.ToChest(config);
            chest.shakeTimer = 50;
            chest.owner.Value = who?.UniqueMultiplayerID ?? Game1.player.UniqueMultiplayerID;
            
            // Automate preferences
            if (config.DisableAutomate)
            {
                if (!chest.modData.ContainsKey("Pathoschild.Automate/StoreItems"))
                    chest.modData.Add("Pathoschild.Automate/StoreItems", "Disable");
                if (!chest.modData.ContainsKey("Pathoschild.Automate/TakeItems"))
                    chest.modData.Add("Pathoschild.Automate/StoreItems", "Disable");
            }

            location.objects.Add(pos, chest);
            location.playSound("hammer");
            __result = true;
            return false;
        }

        /// <summary>Adds count of chests contents to its description.</summary>
        public static void getDescription_Postfix(StardewValley.Object __instance, ref string __result)
        {
            if (__instance is not Chest chest || !ExpandedStorage.HasConfig(__instance))
                return;
            if (chest.items?.Count > 0)
                __result += "\n" + $"Contains {chest.items.Count} items.";
        }

        public static bool drawWhenHeld_Prefix(StardewValley.Object __instance, SpriteBatch spriteBatch, Vector2 objectPosition, Farmer f)
        {
            var config = ExpandedStorage.GetConfig(__instance);
            if (config == null
                || __instance is not Chest chest
                || !chest.playerChest.Value
                || __instance.modData.Keys.Any(ExcludeModDataKeys.Contains))
                return true;
            
            chest.Draw(spriteBatch, objectPosition, Vector2.Zero);
            return false;
        }
    }
}