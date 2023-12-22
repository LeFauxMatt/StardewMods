namespace StardewMods.BetterChests.Framework.Models;

using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using StardewValley.Objects;

/// <summary>Data model representing a virtualized chest.</summary>
internal sealed class VirtualizedChestData
{
    /// <summary>Initializes a new instance of the <see cref="VirtualizedChestData" /> class.</summary>
    /// <param name="globalInventoryId">The global inventory id of the chest.</param>
    /// <param name="itemId">The item id of the chest.</param>
    /// <param name="modData">The mod data of the chest.</param>
    /// <param name="name">The name of the chest.</param>
    /// <param name="playerChoiceColor">The color of the chest.</param>
    [JsonConstructor]
    public VirtualizedChestData(string globalInventoryId, string itemId, Dictionary<string, string> modData, string name, Color playerChoiceColor)
    {
        this.GlobalInventoryId = globalInventoryId;
        this.ItemId = itemId;
        this.ModData = modData;
        this.Name = name;
        this.PlayerChoiceColor = playerChoiceColor;
    }

    /// <summary>Gets or sets the global inventory id.</summary>
    public string GlobalInventoryId { get; set; }

    /// <summary>Gets or sets teh item id.</summary>
    public string ItemId { get; set; }

    /// <summary>Gets or sets the chest mod data.</summary>
    public Dictionary<string, string> ModData { get; set; }

    /// <summary>Gets or sets the chest name.</summary>
    public string Name { get; set; }

    /// <summary>Gets or sets the chest color.</summary>
    public Color PlayerChoiceColor { get; set; }

    /// <summary>Tries to create a virtualized chest using the provided virtualized chest data.</summary>
    /// <param name="vChest">
    /// When this method returns, contains the created virtualized chest if successful, or null if the
    /// creation fails.
    /// </param>
    /// <returns>True if the virtualized chest is successfully created; otherwise, false.</returns>
    public bool TryCreate([NotNullWhen(true)] out VirtualizedChest? vChest)
    {
        if (!Game1.player.team.globalInventories.ContainsKey(this.GlobalInventoryId))
        {
            vChest = null;
            return false;
        }

        var chest = new Chest(true, Vector2.Zero, this.ItemId)
        {
            Name = this.Name,
            GlobalInventoryId = this.GlobalInventoryId,
            playerChoiceColor = { Value = this.PlayerChoiceColor },
        };

        foreach (var (key, value) in this.ModData)
        {
            chest.modData[key] = value;
        }

        vChest = new VirtualizedChest(chest);
        return true;
    }
}
