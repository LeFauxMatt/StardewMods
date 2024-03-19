namespace StardewMods.SpritePatcher.Framework.Services.Migrations;

using StardewMods.SpritePatcher.Framework.Interfaces;

/// <inheritdoc />
internal sealed class Migration_1_0 : BaseMigration
{
    /// <summary>Initializes a new instance of the <see cref="Migration_1_0"/> class.</summary>
    public Migration_1_0()
        : base(new SemanticVersion(1, 0, 0)) { }
}