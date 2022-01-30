namespace StardewMods.BetterChests;

using System;
using System.Collections.Generic;
using System.Linq;
using Common.Integrations.BetterChests;
using StardewModdingAPI;
using StardewMods.BetterChests.Models;
using StardewMods.BetterChests.Services;
using StardewMods.FuryCore.Interfaces;

/// <inheritdoc />
public class BetterChestsApi : IBetterChestsApi
{
    private const string CraftablesData = "Data/BigCraftablesInformation";

    private readonly Lazy<ModConfigMenu> _modConfigMenu;

    /// <summary>
    /// Initializes a new instance of the <see cref="BetterChestsApi"/> class.
    /// </summary>
    /// <param name="chestData">Chest Data as representing the chests.json asset.</param>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    public BetterChestsApi(Dictionary<string, ChestData> chestData, IModHelper helper, IModServices services)
    {
        this.ChestData = chestData;
        this.Helper = helper;
        this._modConfigMenu = services.Lazy<ModConfigMenu>();
    }

    private Dictionary<string, ChestData> ChestData { get; }

    private IModHelper Helper { get; }

    private ModConfigMenu ModConfigMenu
    {
        get => this._modConfigMenu.Value;
    }

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

    /// <inheritdoc/>
    public void AddChestOptions(IManifest manifest, IDictionary<string, string> data)
    {
        this.ModConfigMenu.ChestConfig(manifest, data);
    }
}