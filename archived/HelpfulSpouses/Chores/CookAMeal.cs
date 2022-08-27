namespace StardewMods.HelpfulSpouses.Chores;

/// <summary>
///     Cook a meal for the farmer.
/// </summary>
internal class CookAMeal : IChore
{
    private static CookAMeal? Instance;

    private readonly IModHelper _helper;

    private CookAMeal(IModHelper helper)
    {
        this._helper = helper;
    }

    /// <inheritdoc/>
    public bool IsPossible => true;

    /// <summary>
    ///     Initializes <see cref="CookAMeal" />.
    /// </summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <returns>Returns an instance of the <see cref="CookAMeal" /> class.</returns>
    public static CookAMeal Init(IModHelper helper)
    {
        return CookAMeal.Instance ??= new(helper);
    }

    /// <inheritdoc/>
    public bool TryToDo(NPC spouse)
    {
        throw new System.NotImplementedException();
    }
}