namespace HelpForHire.Chores
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using Common.Helpers.ItemRepository;
    using Common.Services;
    using StardewValley;

    internal class BirthdayGift : GenericChore
    {
        private static List<SearchableItem> AllItems;

        public BirthdayGift(ServiceManager serviceManager)
            : base("birthday-gift", serviceManager)
        {
            BirthdayGift.AllItems = new ItemRepository().GetAll().ToList();
        }

        protected override bool DoChore()
        {
            var gifts = BirthdayGift.GetGifts().ToList();
            if (!gifts.Any())
            {
                return false;
            }

            var mailText = new StringBuilder();
            mailText.Append("%item object ");
            mailText.Append(string.Join(" 1 ", gifts));
            mailText.Append(" 1 %%");

            Game1.mailbox.Add("MyModMail");

            return true;
        }

        protected override bool TestChore()
        {
            return Utility.getTodaysBirthdayNPC(Game1.currentSeason, Game1.dayOfMonth) is not null;
        }

        private static IEnumerable<int> GetGifts()
        {
            foreach (var npc in Utility.getAllCharacters())
            {
                if (!npc.isBirthday(Game1.currentSeason, Game1.dayOfMonth) || !Game1.NPCGiftTastes.TryGetValue(npc.getName(), out var giftTastes))
                {
                    continue;
                }

                var loved = true;
                var personalData = giftTastes.Split('/')[loved ? 1 : 3];
                var itemIds =
                    from s in personalData.Split(' ')
                    select Convert.ToInt32(s, CultureInfo.InvariantCulture);

                foreach (var itemId in itemIds)
                {
                    if (itemId >= 0)
                    {
                        yield return itemId;
                        continue;
                    }

                    foreach (var (objectId, objectInfo) in Game1.objectInformation)
                    {
                        var objectData = objectInfo.Split('/')[3].Split(' ');
                        if (objectData.Length != 2 || itemId != Convert.ToInt32(objectData[1], CultureInfo.InvariantCulture))
                        {
                            continue;
                        }

                        yield return objectId;
                    }
                }
            }
        }
    }
}