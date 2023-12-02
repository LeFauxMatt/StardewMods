namespace StardewMods.HelpfulSpouses.Chores;

using StardewMods.Common.Helpers;
using StardewValley.Extensions;
using StardewValley.Internal;

/// <inheritdoc />
internal sealed class MakeBreakfast : IChore
{
    private Item? breakfast;

    /// <inheritdoc/>
    public void AddTokens(Dictionary<string, object> tokens)
    {
        if (this.breakfast is null)
        {
            return;
        }

        tokens["ItemName"] = this.breakfast.DisplayName;
        tokens["ItemId"] = $"[{this.breakfast.QualifiedItemId}]";
    }

    /// <inheritdoc/>
    public bool IsPossibleForSpouse(NPC spouse)
    {
        return true;
    }

    /// <inheritdoc/>
    public bool TryPerformChore(NPC spouse)
    {
        const string query = $"ALL_ITEMS {ItemRegistry.type_object} @has_category -7";
        var items = ItemQueryResolver.TryResolve(
            query,
            null,
            ItemQuerySearchMode.AllOfTypeItem,
            true,
            null,
            delegate(string _, string error) { Log.Error("Failed parsing that query: " + error); })
            .Select(result => result.Item as Item)
            .Where(item => item is not null && item.HasContextTag("food_breakfast"))
            .ToList();
        this.breakfast = Game1.random.ChooseFrom(items);
        return true;
    }
}