namespace StardewMods.HelpfulSpouses.Framework.Models.Chores;

using StardewMods.HelpfulSpouses.Framework.Services.Chores;

/// <summary>Config data for <see cref="BirthdayGift" />.</summary>
internal sealed class BirthdayGiftOptions
{
    /// <summary>Gets or sets the chance that a disliked item will be given.</summary>
    public double ChanceForDislike { get; set; }

    /// <summary>Gets or sets the chance that a hated item will be given.</summary>
    public double ChanceForHate { get; set; }

    /// <summary>Gets or sets the chance that a liked item will be given.</summary>
    public double ChanceForLike { get; set; } = 0.5;

    /// <summary>Gets or sets the chance that a loved item will be given.</summary>
    public double ChanceForLove { get; set; } = 0.2;

    /// <summary>Gets or sets the chance that a neutral item will be given.</summary>
    public double ChanceForNeutral { get; set; } = 0.1;
}