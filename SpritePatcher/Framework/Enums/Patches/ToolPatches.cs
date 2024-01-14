namespace StardewMods.SpritePatcher.Framework.Enums.Patches;

using NetEscapades.EnumGenerators;
using StardewValley.Tools;

/// <summary>Represents the classes of each tool object that is supported by this mod.</summary>
[EnumExtensions]
public enum ToolPatches
{
    /// <summary>An object of type <see cref="FishingRod" />.</summary>
    PatchedFishingRod,

    /// <summary>An object of type <see cref="MeleeWeapon" />.</summary>
    PatchedMeleeWeapon,

    /// <summary>An object of type <see cref="Slingshot" />.</summary>
    PatchedSlingshot,

    /// <summary>An object of type <see cref="WateringCan" />.</summary>
    PatchedWateringCan,
}