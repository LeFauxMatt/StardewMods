namespace StardewMods.HelpfulSpouses.Framework.Enums;

using NetEscapades.EnumGenerators;

/// <summary>Spouse chores supported by this mod.</summary>
[EnumExtensions]
internal enum ChoreOption
{
    /// <summary>Give the player an item that can be gifted to another NPC if it is their birthday today.</summary>
    BirthdayGift,

    /// <summary>Feed barn, coop, and other animals.</summary>
    FeedTheAnimals,

    /// <summary>Fill the pets water bowls and pet them increasing their friendship level towards the farmer.</summary>
    LoveThePets,

    /// <summary>Gift the player a random breakfast food item.</summary>
    MakeBreakfast,

    /// <summary>Pet barn, coop, and other animals.</summary>
    PetTheAnimals,

    /// <summary>Repair fences on the farm.</summary>
    RepairTheFences,

    /// <summary>Water crops on the farm.</summary>
    WaterTheCrops,

    /// <summary>Fill water troughs in slime hutches on the farm.</summary>
    WaterTheSlimes,
}