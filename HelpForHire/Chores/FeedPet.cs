namespace HelpForHire.Chores
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Common.Services;
    using StardewValley;
    using StardewValley.Characters;

    internal class FeedPet : GenericChore
    {
        public FeedPet(ServiceManager serviceManager)
            : base("feed-pet", serviceManager)
        {
        }

        protected override bool DoChore()
        {
            var petted = false;
            Game1.getFarm().petBowlWatered.Set(true);

            foreach (var pet in FeedPet.GetPets())
            {
                if (!pet.lastPetDay.ContainsKey(Game1.player.UniqueMultiplayerID))
                {
                    pet.lastPetDay.Add(Game1.player.UniqueMultiplayerID, -1);
                }

                pet.lastPetDay[Game1.player.UniqueMultiplayerID] = Game1.Date.TotalDays;
                pet.grantedFriendshipForPet.Set(true);
                pet.friendshipTowardFarmer.Set(Math.Min(1000, pet.friendshipTowardFarmer.Value + 12));
                petted = true;
            }

            return petted;
        }

        protected override bool TestChore()
        {
            return FeedPet.GetPets().Any();
        }

        private static IEnumerable<Pet> GetPets()
        {
            return
                from pet in Game1.getFarm().characters.OfType<Pet>()
                where !pet.grantedFriendshipForPet.Value && pet.lastPetDay[Game1.player.UniqueMultiplayerID] != Game1.Date.TotalDays
                select pet;
        }
    }
}