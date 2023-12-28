namespace StardewMods.HelpfulSpouses.Chores;

using StardewValley.Extensions;

/// <inheritdoc />
internal sealed class MakeBreakfast : IChore
{
    private static readonly Lazy<List<Item>> Items = new(
        delegate
        {
            return ItemRegistry
                .GetObjectTypeDefinition()
                .GetAllIds()
                .Select(localId => ItemRegistry.type_object + localId)
                .Where(id => ItemContextTagManager.HasBaseTag(id, "food_breakfast"))
                .Select(id => ItemRegistry.Create(id))
                .ToList();
        });

    private Item? breakfast;

    /// <inheritdoc />
    public void AddTokens(Dictionary<string, object> tokens)
    {
        if (this.breakfast is null)
        {
            return;
        }

        tokens["ItemName"] = this.breakfast.DisplayName;
        tokens["ItemId"] = $"[{this.breakfast.QualifiedItemId}]";
    }

    /// <inheritdoc />
    public bool IsPossibleForSpouse(NPC spouse) => true;

    /// <inheritdoc />
    public bool TryPerformChore(NPC spouse)
    {
        this.breakfast = Game1.random.ChooseFrom(MakeBreakfast.Items.Value);
        return true;
    }
}