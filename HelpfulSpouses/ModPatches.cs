namespace StardewMods.HelpfulSpouses;

using HarmonyLib;

/// <summary>
///     Harmony Patches for HelpfulSpouses.
/// </summary>
internal sealed class ModPatches
{
    private static ModPatches? instance;

    private ModPatches(IManifest manifest)
    {
        var harmony = new Harmony(manifest.UniqueID);
    }

    /// <summary>
    ///     Initializes <see cref="ModPatches" />.
    /// </summary>
    /// <param name="manifest">A manifest to describe the mod.</param>
    /// <returns>Returns an instance of the <see cref="ModPatches" /> class.</returns>
    public static ModPatches Init(IManifest manifest)
    {
        return ModPatches.instance ??= new(manifest);
    }
}