using System;
using System.Diagnostics.CodeAnalysis;
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
        private readonly Type _type = typeof(StardewValley.Object);
        
        private static IReflectionHelper Reflection;

        internal ObjectPatch(IMonitor monitor, ModConfig config, IReflectionHelper reflection)
            : base(monitor, config)
        {
            Reflection = reflection;
        }
        
        protected internal override void Apply(HarmonyInstance harmony)
        {
            harmony.Patch(AccessTools.Method(_type, nameof(StardewValley.Object.placementAction)),
                new HarmonyMethod(GetType(), nameof(PlacementAction)));
            
            if (Config.AllowCarryingChests)
            {
                harmony.Patch(AccessTools.Method(_type, nameof(StardewValley.Object.getDescription)),
                    postfix: new HarmonyMethod(GetType(), nameof(getDescription_Postfix)));

                harmony.Patch(AccessTools.Method(_type, nameof(StardewValley.Object.drawWhenHeld)),
                    new HarmonyMethod(GetType(), nameof(drawWhenHeld_Prefix)));
            }
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
                return true;
            
            // Place Expanded Storage Chest
            if (!Enum.TryParse(config.SpecialChestType, out Chest.SpecialChestTypes specialChestType))
                specialChestType = Chest.SpecialChestTypes.None;
            var chest = new Chest(true, pos, __instance.ParentSheetIndex)
            {
                name = __instance.Name,
                shakeTimer = 50,
                SpecialChestType = specialChestType
            };
            chest.owner.Value = who?.UniqueMultiplayerID ?? Game1.player.UniqueMultiplayerID;
            chest.fixLidFrame();

            // Copy properties from previously held chest
            if (__instance is Chest oldChest)
            {
                chest.playerChoiceColor.Value = oldChest.playerChoiceColor.Value;
                if (oldChest.items.Any())
                    chest.items.CopyFrom(oldChest.items);
            }
            
            foreach (var modData in __instance.modData)
                chest.modData.CopyFrom(modData);
            
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
                || !chest.playerChest.Value)
                return true;
            
            chest.draw(spriteBatch, (int)objectPosition.X, (int)objectPosition.Y + 64, 1f, true);
            return false;
        }
    }
}