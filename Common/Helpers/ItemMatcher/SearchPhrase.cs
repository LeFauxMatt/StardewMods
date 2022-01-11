namespace Common.Helpers.ItemMatcher;

using System;
using System.Collections.Generic;
using System.Linq;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;
using SObject = StardewValley.Object;

/// <summary>
///     A search phrase for an Item name or tags.
/// </summary>
internal record SearchPhrase
{
    private const string CategoryFurniture = "category_furniture";
    private const string CategoryArtifact = "category_artifact";
    private const string DonateMuseum = "donate_museum";
    private const string DonateBundle = "donate_bundle";

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <param name="tagMatch"></param>
    /// <param name="exactMatch"></param>
    public SearchPhrase(string value, bool tagMatch = true, bool exactMatch = false)
    {
        this.NotMatch = (value[..1] == "!");
        this.ExactMatch = exactMatch;
        this.TagMatch = tagMatch;
        this.Value = this.NotMatch ? value[1..] : value;
    }

    public bool ExactMatch { get; }
    public bool NotMatch { get; }
    public bool TagMatch { get; }
    public string Value { get; }

    public static IEnumerable<string> GetContextTags(Item item)
    {
        foreach (var contextTag in item.GetContextTags().Where(contextTag => !contextTag.StartsWith("id_")))
        {
            yield return contextTag;
        }

        if (item is SObject obj && SearchPhrase.CanDonateToBundle(obj))
        {
            yield return SearchPhrase.DonateBundle;
        }

        if (SearchPhrase.CanDonateToMuseum(item))
        {
            yield return SearchPhrase.DonateMuseum;
        }
    }

    /// <summary>
    ///     Checks if item matches this search phrase.
    /// </summary>
    /// <param name="item">The item to check.</param>
    /// <returns>Returns true if item matches the search phrase.</returns>
    public bool Matches(Item item)
    {
        if (!this.TagMatch)
        {
            return this.Matches(item.Name) != this.NotMatch;
        }

        return item switch
        {
            Furniture when this.Matches(SearchPhrase.CategoryFurniture) => true,
            SObject {Type: "Arch"} when this.Matches(SearchPhrase.CategoryArtifact) => true,
            SObject {Type: "Arch"} when this.Matches(SearchPhrase.DonateMuseum) => SearchPhrase.CanDonateToMuseum(item),
            SObject {Type: "Minerals"} when this.Matches(SearchPhrase.DonateMuseum) => SearchPhrase.CanDonateToMuseum(item),
            SObject obj when this.Matches(SearchPhrase.DonateBundle) => SearchPhrase.CanDonateToBundle(obj),
            _ => item.GetContextTags().Any(this.Matches) != this.NotMatch,
        };
    }

    private static bool CanDonateToMuseum(Item item)
    {
        return Game1.locations
                    .OfType<LibraryMuseum>()
                    .FirstOrDefault()?.isItemSuitableForDonation(item)
               ?? false;
    }

    private static bool CanDonateToBundle(SObject obj)
    {
        return Game1.locations
                    .OfType<CommunityCenter>()
                    .FirstOrDefault()?.couldThisIngredienteBeUsedInABundle(obj)
               ?? false;
    }

    private bool Matches(string match)
    {
        return this.ExactMatch
            ? this.Value.Equals(match, StringComparison.OrdinalIgnoreCase)
            : match.IndexOf(this.Value, StringComparison.OrdinalIgnoreCase) > -1;
    }
}