namespace StardewMods.HelpfulSpouses.Chores;

using System;

internal sealed class PetTheAnimals : IChore
{
    private static PetTheAnimals? Instance;

    private readonly IModHelper _helper;

    private PetTheAnimals(IModHelper helper)
    {
        this._helper = helper;
    }

    /// <inheritdoc />
    public bool IsPossible { get; }

    /// <summary>
    ///     Initializes <see cref="PetTheAnimals" />.
    /// </summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <returns>Returns an instance of the <see cref="PetTheAnimals" /> class.</returns>
    public static PetTheAnimals Init(IModHelper helper)
    {
        return PetTheAnimals.Instance ??= new(helper);
    }

    /// <inheritdoc />
    public bool TryToDo(NPC spouse)
    {
        throw new NotImplementedException();
    }
}