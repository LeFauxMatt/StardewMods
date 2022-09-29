namespace StardewMods.HelpfulSpouses.Chores;

using System;

internal sealed class WaterTheSlimes : IChore
{
    private static WaterTheSlimes? Instance;

    private readonly IModHelper _helper;

    private WaterTheSlimes(IModHelper helper)
    {
        this._helper = helper;
    }

    /// <inheritdoc />
    public bool IsPossible { get; }

    /// <summary>
    ///     Initializes <see cref="WaterTheSlimes" />.
    /// </summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <returns>Returns an instance of the <see cref="WaterTheSlimes" /> class.</returns>
    public static WaterTheSlimes Init(IModHelper helper)
    {
        return WaterTheSlimes.Instance ??= new(helper);
    }

    /// <inheritdoc />
    public bool TryToDo(NPC spouse)
    {
        throw new NotImplementedException();
    }
}