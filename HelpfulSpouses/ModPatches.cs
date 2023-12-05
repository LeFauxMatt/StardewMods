namespace StardewMods.HelpfulSpouses;

using HarmonyLib;

/// <summary>
/// Harmony Patches for HelpfulSpouses.
/// </summary>
internal sealed class ModPatches
{
    private static ModPatches? instance;

    private ModPatches(IManifest manifest)
    {
        var harmony = new Harmony(manifest.UniqueID);
        harmony.Patch(
            AccessTools.Method(typeof(NPC), nameof(NPC.marriageDuties)),
            new(typeof(ModPatches), nameof(ModPatches.NPC_marriageDuties_prefix)));
    }

    /// <summary>
    /// Initializes <see cref="ModPatches" />.
    /// </summary>
    /// <param name="manifest">A manifest to describe the mod.</param>
    /// <returns>Returns an instance of the <see cref="ModPatches" /> class.</returns>
    public static ModPatches Init(IManifest manifest) => ModPatches.instance ??= new(manifest);

    private static void NPC_marriageDuties_prefix()
    {
        NPC.hasSomeoneFedTheAnimals = true;
        NPC.hasSomeoneFedThePet = true;
        NPC.hasSomeoneRepairedTheFences = true;
        NPC.hasSomeoneWateredCrops = true;
    }
}
