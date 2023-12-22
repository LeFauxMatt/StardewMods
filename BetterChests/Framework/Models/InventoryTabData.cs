namespace StardewMods.BetterChests.Framework.Models;

using Newtonsoft.Json;

/// <summary>Represents the data for an inventory tab.</summary>
internal sealed class InventoryTabData
{
    /// <summary>Initializes a new instance of the <see cref="InventoryTabData" /> class.</summary>
    /// <param name="name">The name of the inventory tab.</param>
    /// <param name="path">The path to the tab texture.</param>
    /// <param name="index">The index of the tab icon.</param>
    /// <param name="rules">The rules that will be applied when the tab is selected.</param>
    [JsonConstructor]
    public InventoryTabData(string name, string path, int index, HashSet<string> rules)
    {
        this.Name = name;
        this.Path = path;
        this.Index = index;
        this.Rules = rules;
    }

    /// <summary>Gets or sets the name of the tab.</summary>
    public string Name { get; set; }

    /// <summary>Gets or sets the path to the tab texture.</summary>
    public string Path { get; set; }

    /// <summary>Gets or sets the index of the tab icon.</summary>
    public int Index { get; set; }

    /// <summary>Gets or sets the item rules that will be applied when the tab is selected.</summary>
    public HashSet<string> Rules { get; set; }
}
