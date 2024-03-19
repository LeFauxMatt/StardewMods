namespace StardewMods.HelpfulSpouses.Framework.Models;

using StardewMods.HelpfulSpouses.Framework.Enums;

/// <summary>Represents the options for a character's chores.</summary>
internal sealed class CharacterOptions
{
    private readonly Dictionary<ChoreOption, double> data = new();

    /// <summary>Gets or sets the chance that the character will perform a <see cref="ChoreOption" /> chore.</summary>
    /// <param name="choreOption">The chore to get or set the value for.</param>
    public double this[ChoreOption choreOption]
    {
        get => this.data.GetValueOrDefault(choreOption, 0);
        set => this.data[choreOption] = value;
    }
}