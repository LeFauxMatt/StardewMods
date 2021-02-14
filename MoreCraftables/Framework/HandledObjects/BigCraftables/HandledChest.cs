using System;
using Microsoft.Xna.Framework;
using MoreCraftables.API;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;
using Object = StardewValley.Object;

namespace MoreCraftables.Framework.HandledObjects.BigCraftables
{
    public class HandledChest : HandledBigCraftable
    {
        private readonly bool _fridge;

        private readonly Chest.SpecialChestTypes _specialChestType;

        public HandledChest(IHandledObject handledObject) : base(handledObject)
        {
            _specialChestType =
                Properties.TryGetValue("SpecialChestType", out var specialChestType)
                && specialChestType is string specialChestTypeString
                && Enum.TryParse(specialChestTypeString, out Chest.SpecialChestTypes specialChestTypeValue)
                    ? specialChestTypeValue
                    : Chest.SpecialChestTypes.None;
            _fridge = Properties.TryGetValue("fridge", out var fridge) && (bool) fridge;
        }

        public override Object CreateInstance(Object obj, GameLocation location, Vector2 pos)
        {
            if (location is MineShaft || location is VolcanoDungeon)
            {
                Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.13053"));
                return null;
            }

            PlaySoundOrDefault(location, "axe");
            var chest = new Chest(true, pos, obj.ParentSheetIndex)
            {
                shakeTimer = 50,
                SpecialChestType = _specialChestType
            };
            chest.fridge.Value = _fridge;

            if (obj is not Chest otherChest)
                return chest;

            chest.playerChoiceColor.Value = otherChest.playerChoiceColor.Value;
            if (otherChest.items.Any())
                chest.items.CopyFrom(otherChest.items);
            
            return chest;
        }
    }
}