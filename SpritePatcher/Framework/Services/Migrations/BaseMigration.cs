namespace StardewMods.SpritePatcher.Framework.Services.Migrations;

using StardewMods.SpritePatcher.Framework.Interfaces;

/// <inheritdoc />
internal abstract class BaseMigration : IMigration
{
    /// <summary>Initializes a new instance of the <see cref="BaseMigration" /> class.</summary>
    /// <param name="version">The version to migrate to.</param>
    protected BaseMigration(SemanticVersion version) => this.Version = version;

    /// <inheritdoc />
    public ISemanticVersion Version { get; }
}