namespace XSPlus
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Xna.Framework;
    using StardewValley;
    using StardewValley.Objects;
    using SObject = StardewValley.Object;

    internal static class Extensions
    {
        /// <summary></summary>
        /// <param name="item"></param>
        /// <param name="key"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        public static bool GetModDataList(this Item item, string key, out List<string> list)
        {
            if (!item.modData.TryGetValue($"{XSPlus.ModPrefix}/{key}", out string value))
            {
                list = new List<string>();
                return false;
            }

            list = value.Split(' ').ToList();
            return true;
        }

        /// <summary></summary>
        /// <param name="item"></param>
        /// <param name="key"></param>
        /// <param name="parts"></param>
        public static void SetModDataList(this Item item, string key, IEnumerable<string> parts)
        {
            string value = string.Join(" ", parts.Where(part => !string.IsNullOrWhiteSpace(part)));
            if (!string.IsNullOrWhiteSpace(value))
            {
                item.modData[$"{XSPlus.ModPrefix}/{key}"] = value;
            }
            else
            {
                item.modData.Remove($"{XSPlus.ModPrefix}/{key}");
            }
        }

        /// <summary></summary>
        /// <param name="farmer"></param>
        /// <param name="item"></param>
        /// <param name="chests"></param>
        /// <returns></returns>
        public static Item AddItemToInventory(this Farmer farmer, Item item, IList<Chest> chests)
        {
            if (!farmer.IsLocalPlayer || !chests.Any())
            {
                return item;
            }

            Item tmp = null;
            uint stack = (uint)item.Stack;
            foreach (Chest chest in chests)
            {
                tmp = chest.addItem(item);
                if (tmp == null)
                {
                    break;
                }
            }

            if (item.HasBeenInInventory)
            {
                return null;
            }

            if (tmp?.Stack == item.Stack && item is not SpecialItem)
            {
                return tmp;
            }

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
                            {
                                farmer.specialBigCraftables.Add(obj.ParentSheetIndex);
                            }
                        }
                        else if (!farmer.specialItems.Contains(obj.ParentSheetIndex))
                        {
                            farmer.specialItems.Add(obj.ParentSheetIndex);
                        }
                    }

                    if (!obj.HasBeenPickedUpByFarmer)
                    {
                        if (obj.Category == -2 || (obj.Type != null && obj.Type.Contains("Mineral")))
                        {
                            farmer.foundMineral(obj.ParentSheetIndex);
                        }
                        else if (item is not Furniture && obj.Type != null && obj.Type.Contains("Arch"))
                        {
                            farmer.foundArtifact(obj.ParentSheetIndex, 1);
                        }
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

            string itemName = item.DisplayName;
            Color hudColor = Color.WhiteSmoke;
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