namespace StardewMods.SpritePatcher.Framework.Interfaces;

/// <summary>Migrates patches to a given format version.</summary>
internal interface IMigration
{
    /// <summary>Gets the version of the migration.</summary>
    ISemanticVersion Version { get; }
}