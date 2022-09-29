namespace StardewMods.HelpfulSpouses.Chores;

using System;

internal sealed class LoveThePets : IChore
{
    private static LoveThePets? Instance;

    private readonly IModHelper _helper;

    private LoveThePets(IModHelper helper)
    {
        this._helper = helper;
    }

    /// <inheritdoc />
    public bool IsPossible { get; }

    /// <summary>
    ///     Initializes <see cref="LoveThePets" />.
    /// </summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <returns>Returns an instance of the <see cref="LoveThePets" /> class.</returns>
    public static LoveThePets Init(IModHelper helper)
    {
        return LoveThePets.Instance ??= new(helper);
    }

    /// <inheritdoc />
    public bool TryToDo(NPC spouse)
    {
        throw new NotImplementedException();
    }
}