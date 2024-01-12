namespace StardewMods.SpritePatcher.Framework.Enums;

/// <summary>How the patch will be applied.</summary>
public enum PatchMode
{
    /// <summary>Overlays the patch on top of the target.</summary>
    Overlay,

    /// <summary>Replaces the target with the patch.</summary>
    Replace,
}