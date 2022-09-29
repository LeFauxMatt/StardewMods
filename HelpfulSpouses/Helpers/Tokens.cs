namespace StardewMods.HelpfulSpouses.Helpers;

using System.Collections.Generic;

/// <summary>
///     Handles tokens for localizations.
/// </summary>
internal sealed class Tokens
{
    private static Tokens? Instance;

    private readonly IModHelper _helper;

    private Tokens(IModHelper helper)
    {
        this._helper = helper;
    }

    /// <summary>
    ///     Gets tokens for localizations.
    /// </summary>
    /// <param name="spouse">The spouse name.</param>
    /// <param name="tokens">The tokens to append.</param>
    /// <returns>Returns a dictionary with updated tokens.</returns>
    public static Dictionary<string, string> Get(NPC spouse, Dictionary<string, string> tokens)
    {
        foreach (var (key, value) in Tokens.Base(spouse))
        {
            tokens[key] = value;
        }

        return tokens;
    }

    /// <summary>
    ///     Initializes <see cref="Tokens" />.
    /// </summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <returns>Returns an instance of the <see cref="Tokens" /> class.</returns>
    public static Tokens Init(IModHelper helper)
    {
        return Tokens.Instance ??= new(helper);
    }

    private static Dictionary<string, string> Base(NPC spouse)
    {
        return new()
        {
            { "name", Game1.player.Name },
            { "endearment", spouse.getTermOfSpousalEndearment() },
            { "endearment-lower", spouse.getTermOfSpousalEndearment().ToLower() },
            { "pet-name", Game1.player.getPetDisplayName() },
        };
    }
}