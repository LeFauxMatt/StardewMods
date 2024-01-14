namespace StardewMods.SpritePatcher.Framework.Enums.Patches;

using NetEscapades.EnumGenerators;
using StardewValley.Buildings;

/// <summary>Represents the classes of each building object that is supported by this mod.</summary>
[EnumExtensions]
public enum BuildingPatches
{
    /// <summary>An object of type <see cref="Building" />.</summary>
    PatchedBuilding,

    /// <summary>An object of type <see cref="FishPond" />.</summary>
    PatchedFishPond,

    /// <summary>An object of type <see cref="JunimoHut" />.</summary>
    PatchedJunimoHut,

    /// <summary>An object of type <see cref="PetBowl" />.</summary>
    PatchedPetBowl,

    /// <summary>An object of type <see cref="ShippingBin" />.</summary>
    PatchedShippingBin,
}