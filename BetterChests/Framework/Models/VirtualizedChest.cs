namespace StardewMods.BetterChests.Framework.Models;

using System.Globalization;
using Microsoft.Xna.Framework;
using StardewValley.Objects;

/// <summary>An instance of a chest which has been detached from the world.</summary>
internal sealed class VirtualizedChest
{
    private const string AlphaNumeric = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
    private const string ColorKey = VirtualizedChest.Prefix + "PlayerChoiceColor";
    private const string GlobalInventoryIdKey = VirtualizedChest.Prefix + "GlobalInventoryId";
    private const string Prefix = "furyx639.BetterChests/";

    /// <summary>Initializes a new instance of the <see cref="VirtualizedChest" /> class.</summary>
    /// <param name="chest">The chest to create a virtualized chest from.</param>
    public VirtualizedChest(Chest chest)
    {
        this.Chest = chest;
        this.GlobalInventoryId = VirtualizedChest.GenerateGlobalInventoryId();
    }

    /// <summary>Gets the global inventory id.</summary>
    public string GlobalInventoryId { get; }

    /// <summary>Gets the chest.</summary>
    public Chest Chest { get; }

    /// <summary>Tries to get the global inventory id of the proxy item.</summary>
    /// <param name="item">The item to get the identifier from.</param>
    /// <param name="id">The identifier of the item, if found.</param>
    /// <returns>True if the identifier is successfully retrieved, otherwise false.</returns>
    public static bool TryGetId(ISalable item, [NotNullWhen(true)] out string? id)
    {
        if (item is SObject sourceObject && sourceObject.modData.TryGetValue(VirtualizedChest.GlobalInventoryIdKey, out id) && Game1.player.team.globalInventories.ContainsKey(id))
        {
            return true;
        }

        id = null;
        return false;
    }

    /// <summary>Create a new proxy item for a chest.</summary>
    /// <returns>A new instance of the Item class.</returns>
    public Item CreateProxyItem()
    {
        // Create Proxy Item
        var globalInventoryId = VirtualizedChest.GenerateGlobalInventoryId();
        var newItem = ItemRegistry.Create(this.Chest.QualifiedItemId);
        newItem.Name = this.Chest.Name;
        newItem.modData[VirtualizedChest.GlobalInventoryIdKey] = globalInventoryId;

        // Store Color Data
        if (this.Chest.playerChoiceColor.Value != Color.Black)
        {
            var c = this.Chest.playerChoiceColor.Value;
            var color = (c.R << 0) | (c.G << 8) | (c.B << 16);
            newItem.modData[VirtualizedChest.ColorKey] = color.ToString(CultureInfo.InvariantCulture);
        }

        return newItem;
    }

    /// <summary>Retrieves the data for a virtualized chest.</summary>
    /// <returns>A VirtualizedChestData object containing the chest data.</returns>
    public VirtualizedChestData GetData()
    {
        var modData = new Dictionary<string, string>();
        foreach (var (key, value) in this.Chest.modData.Pairs)
        {
            modData[key] = value;
        }

        return new VirtualizedChestData(this.GlobalInventoryId, this.Chest.ItemId, modData, this.Chest.Name, this.Chest.playerChoiceColor.Value);
    }

    /// <summary>Moves all items from the chest to the global inventory.</summary>
    public void TransferItemsFromChest()
    {
        // Move Items to Global Inventory
        this.Chest.GlobalInventoryId = this.GlobalInventoryId;
        var globalInventory = Game1.player.team.GetOrCreateGlobalInventory(this.GlobalInventoryId);
        globalInventory.OverwriteWith(this.Chest.Items);
        this.Chest.Items.Clear();
    }

    /// <summary>
    ///     Transfers all items from the global inventory associated with this object to the chest object owned by this
    ///     object.
    /// </summary>
    public void TransferItemsToChest()
    {
        // Move Items to Chest
        this.Chest.GlobalInventoryId = null;
        var globalInventory = Game1.player.team.GetOrCreateGlobalInventory(this.GlobalInventoryId);
        this.Chest.Items.OverwriteWith(globalInventory);

        // Clear Global Inventory
        Game1.player.team.globalInventories.Remove(this.GlobalInventoryId);
        Game1.player.team.globalInventoryMutexes.Remove(this.GlobalInventoryId);
    }

    private static string GenerateGlobalInventoryId()
    {
        var globalInventoryId = VirtualizedChest.Prefix + VirtualizedChest.RandomString();
        while (Game1.player.team.globalInventories.ContainsKey(globalInventoryId) || Game1.player.team.globalInventoryMutexes.ContainsKey(globalInventoryId))
        {
            globalInventoryId = VirtualizedChest.Prefix + VirtualizedChest.RandomString();
        }

        return globalInventoryId;
    }

    private static string RandomString()
    {
        var stringChars = new char[16];
        var random = new Random();

        for (var i = 0; i < stringChars.Length; i++)
        {
            stringChars[i] = VirtualizedChest.AlphaNumeric[random.Next(VirtualizedChest.AlphaNumeric.Length)];
        }

        return new string(stringChars);
    }
}
