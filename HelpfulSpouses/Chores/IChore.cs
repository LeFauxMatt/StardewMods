namespace StardewMods.HelpfulSpouses.Chores;

/// <summary>
///     Implementation of a Helpful Spouses task.
/// </summary>
internal interface IChore
{
    /// <summary>
    ///     Gets a value indicating whether the chore is possible today.
    /// </summary>
    public bool IsPossible { get; }

    /// <summary>
    ///     Attempts to perform the chore.
    /// </summary>
    /// <returns>Returns true if the chore was performed successfully.</returns>
    public bool TryToDo(NPC spouse);
}