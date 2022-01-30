namespace Mod.BetterChests;

using System.Collections.Generic;
using System.Linq;
using Common.Integrations.BetterChests;
using Mod.BetterChests.Models;
using StardewModdingAPI;

/// <inheritdoc />
internal class BetterChestsApi : IBetterChestsApi
{
    private const string CraftablesData = "Data/BigCraftablesInformation";

    /// <summary>
    /// Initializes a new instance of the <see cref="BetterChestsApi"/> class.
    /// </summary>
    /// <param name="chestData">Chest Data as representing the chests.json asset.</param>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    public BetterChestsApi(Dictionary<string, ChestData> chestData, IModHelper helper)
    {
        this.ChestData = chestData;
        this.Helper = helper;
    }

    private Dictionary<string, ChestData> ChestData { get; }

    private IModHelper Helper { get; }

    private IEnumerable<string[]> Craftables
    {
        get => this.Helper.Content.Load<Dictionary<int, string>>(BetterChestsApi.CraftablesData, ContentSource.GameContent).Values.Select(info => info.Split('/'));
    }

    /// <inheritdoc/>
    public bool RegisterChest(string name)
    {
        var found = (from info in this.Craftables where info[0] == name select info[8]).Any();
        if (found && !this.ChestData.ContainsKey(name))
        {
            this.ChestData.Add(name, new());
            this.Helper.Data.WriteJsonFile("assets/chests.json", this.ChestData);
            return true;
        }

        return false;
    }
}