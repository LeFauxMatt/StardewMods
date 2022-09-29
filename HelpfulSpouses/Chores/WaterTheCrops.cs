namespace StardewMods.HelpfulSpouses.Chores;

using System;

internal sealed class WaterTheCrops : IChore
{
    private static WaterTheCrops? Instance;

    private readonly IModHelper _helper;

    private WaterTheCrops(IModHelper helper)
    {
        this._helper = helper;
    }

    /// <inheritdoc />
    public bool IsPossible { get; }

    /// <summary>
    ///     Initializes <see cref="WaterTheCrops" />.
    /// </summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <returns>Returns an instance of the <see cref="WaterTheCrops" /> class.</returns>
    public static WaterTheCrops Init(IModHelper helper)
    {
        return WaterTheCrops.Instance ??= new(helper);
    }

    /// <inheritdoc />
    public bool TryToDo(NPC spouse)
    {
        throw new NotImplementedException();
    }
}