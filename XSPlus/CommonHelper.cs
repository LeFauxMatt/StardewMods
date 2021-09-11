using System;
using System.Collections.Generic;
using System.Linq;
using Common.Extensions;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;
using SObject = StardewValley.Object;

namespace XSPlus
{
    internal static class CommonHelper
    {
        // ReSharper disable InconsistentNaming
        public static InventoryMenu.highlightThisItem HighlightMethods_ItemsToGrabMenu;
        public static InventoryMenu.highlightThisItem HighlightMethods_inventory;
        public static bool HighlightMethod_ItemsToGrabMenu(Item item)
        {
            return HighlightMethods_ItemsToGrabMenu is null || HighlightMethods_ItemsToGrabMenu.GetInvocationList().All(highlightMethod => (bool)highlightMethod.DynamicInvoke(item));
        }
        public static bool HighlightMethod_inventory(Item item)
        {
            return HighlightMethods_inventory is null || HighlightMethods_inventory.GetInvocationList().All(highlightMethod => (bool)highlightMethod.DynamicInvoke(item));
        }
        // ReSharper restore InconsistentNaming
        public delegate IEnumerable<GameLocation> GetLocations();
        public static IEnumerable<GameLocation> AllLocations =>
            Game1.locations.Concat(
                Game1.locations.OfType<BuildableGameLocation>().SelectMany(
                    location => location.buildings.Where(building => building.indoors.Value is not null).Select(building => building.indoors.Value)
                )
            );
        public static IEnumerable<GameLocation> GetAccessibleLocations(GetLocations getActiveLocations) =>
            Context.IsMainPlayer ? AllLocations : getActiveLocations();
        public static IEnumerable<Chest> GetChests(IEnumerable<GameLocation> locations) => locations.SelectMany(location => location.Objects.Values.OfType<Chest>());
        public static bool GetModDataList(this Item item, string key, out List<string> list)
        {
            if (!item.modData.TryGetValue($"{XSPlus.ModPrefix}/{key}", out var value))
            {
                list = new List<string>();
                return false;
            }
            list = value.Split(' ').ToList();
            return true;
        }
        public static void SetModDataList(this Item item, string key, IEnumerable<string> parts)
        {
            var value = string.Join(" ", parts.Where(part => !string.IsNullOrWhiteSpace(part)));
            if (!string.IsNullOrWhiteSpace(value))
                item.modData[$"{XSPlus.ModPrefix}/{key}"] = value;
            else
                item.modData.Remove($"{XSPlus.ModPrefix}/{key}");
        }
        public static bool SearchTag(this Item item, string searchItem, string searchTagSymbol)
        {
            var matchCondition = !searchItem.StartsWith("!");
            var searchPhrase = matchCondition ? searchItem : searchItem.Substring(1);
            if (string.IsNullOrWhiteSpace(searchPhrase))
                return true;
            if (searchPhrase.StartsWith(searchTagSymbol))
            {
                if (item.MatchesTagExt(searchPhrase.Substring(1), false) != matchCondition)
                    return false;
            }
            else if ((!item.Name.Contains(searchPhrase) &&
                      !item.DisplayName.Contains(searchPhrase)) == matchCondition)
            {
                return false;
            }
            return true;
        }
        public static Item AddItemToInventory(this Farmer farmer, Item item, IEnumerable<Chest> vacuumChests)
        {
            if (!farmer.IsLocalPlayer || !vacuumChests.Any())
                return item;
            
            Item tmp = null;
            var stack = (uint) item.Stack;
            foreach (var vacuumChest in vacuumChests)
            {
                tmp = vacuumChest.addItem(item);
                if (tmp == null)
                    break;
            }
            
            if (item.HasBeenInInventory)
                return null;
            if (tmp?.Stack == item.Stack && item is not SpecialItem)
                return tmp;
            
            switch (item)
            {
                case SpecialItem specialItem:
                    specialItem.actionWhenReceived(farmer);
                    return tmp;
                case SObject obj:
                {
                    if (obj.specialItem)
                    {
                        if (obj.bigCraftable.Value || item is Furniture)
                        {
                            if (!farmer.specialBigCraftables.Contains(obj.ParentSheetIndex))
                                farmer.specialBigCraftables.Add(obj.ParentSheetIndex);
                        }
                        else if (!farmer.specialItems.Contains(obj.ParentSheetIndex))
                        {
                            farmer.specialItems.Add(obj.ParentSheetIndex);
                        }
                    }
                    if (!obj.HasBeenPickedUpByFarmer)
                    {
                        if (obj.Category == -2 || obj.Type != null && obj.Type.Contains("Mineral"))
                            farmer.foundMineral(obj.ParentSheetIndex);
                        else if (item is not Furniture && obj.Type != null && obj.Type.Contains("Arch")) farmer.foundArtifact(obj.ParentSheetIndex, 1);
                    }
                    Utility.checkItemFirstInventoryAdd(item);
                    break;
                }
            }
            
            switch (item.ParentSheetIndex)
            {
                case 384:
                    Game1.stats.GoldFound += stack;
                    break;
                case 378:
                    Game1.stats.CopperFound += stack;
                    break;
                case 380:
                    Game1.stats.IronFound += stack;
                    break;
                case 386:
                    Game1.stats.IridiumFound += stack;
                    break;
            }
            
            var itemName = item.DisplayName;
            var hudColor = Color.WhiteSmoke;
            if (item is SObject showObj)
            {
                switch (showObj.Type)
                {
                    case "Arch":
                        hudColor = Color.Tan;
                        itemName += Game1.content.LoadString("Strings\\StringsFromCSFiles:Farmer.cs.1954");
                        break;
                    case "Fish":
                        hudColor = Color.SkyBlue;
                        break;
                    case "Mineral":
                        hudColor = Color.PaleVioletRed;
                        break;
                    case "Vegetable":
                        hudColor = Color.PaleGreen;
                        break;
                    case "Fruit":
                        hudColor = Color.Pink;
                        break;
                }
            }
            Game1.addHUDMessage(new HUDMessage(itemName, Math.Max(1, item.Stack), true, hudColor, item));
            return tmp;
        }
    }
}