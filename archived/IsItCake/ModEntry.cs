namespace StardewMods.IsItCake;

using StardewMods.Common.Helpers;
using StardewMods.IsItCake.Framework;

/// <inheritdoc />
public sealed class ModEntry : Mod
{
    /// <inheritdoc />
    public override void Entry(IModHelper helper)
    {
        // Init
        I18n.Init(this.Helper.Translation);
        Log.Monitor = this.Monitor;
        ModPatches.Init(this.ModManifest);
    }
}