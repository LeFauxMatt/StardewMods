namespace StardewMods.HelpfulSpouses.Chores;

/// <summary>Defines a chore that can be done.</summary>
internal interface IChore
{
    /// <summary>Adds dialogue tokens for chore.</summary>
    /// <param name="tokens">The dictionary to add tokens to.</param>
    void AddTokens(Dictionary<string, object> tokens);

    /// <summary>Checks if it is possible for spouse to perform the chore.</summary>
    /// <param name="spouse">The spouse performing the chore.</param>
    /// <returns>Returns true if the chore can be performed.</returns>
    bool IsPossibleForSpouse(NPC spouse);

    /// <summary>Attempts to perform the chore.</summary>
    /// <param name="spouse">The spouse performing the chore.</param>
    /// <returns>Returns true if the chore was performed.</returns>
    bool TryPerformChore(NPC spouse);
}