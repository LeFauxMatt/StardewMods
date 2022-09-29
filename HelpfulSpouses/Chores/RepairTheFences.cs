namespace StardewMods.HelpfulSpouses.Chores;

using System;

internal sealed class RepairTheFences : IChore
{
    private static RepairTheFences? Instance;

    private readonly IModHelper _helper;

    private RepairTheFences(IModHelper helper)
    {
        this._helper = helper;
    }

    /// <inheritdoc />
    public bool IsPossible { get; }

    /// <summary>
    ///     Initializes <see cref="RepairTheFences" />.
    /// </summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <returns>Returns an instance of the <see cref="RepairTheFences" /> class.</returns>
    public static RepairTheFences Init(IModHelper helper)
    {
        return RepairTheFences.Instance ??= new(helper);
    }

    /// <inheritdoc />
    public bool TryToDo(NPC spouse)
    {
        throw new NotImplementedException();
    }
}