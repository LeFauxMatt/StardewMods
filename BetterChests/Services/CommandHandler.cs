namespace StardewMods.BetterChests.Services;

using System;
using System.Linq;
using System.Text;
using Common.Helpers;
using StardewModdingAPI;
using StardewMods.BetterChests.Features;
using StardewMods.BetterChests.Helpers;
using StardewMods.BetterChests.Interfaces.Config;
using StardewMods.BetterChests.Models.ManagedObjects;
using StardewMods.FuryCore.Interfaces;

/// <inheritdoc />
internal class CommandHandler : IModService
{
    private readonly Lazy<AssetHandler> _assetHandler;
    private readonly Lazy<CraftFromChest> _craftFromChest;
    private readonly Lazy<ManagedObjects> _managedObjects;
    private readonly Lazy<StashToChest> _stashToChest;

    /// <summary>
    ///     Initializes a new instance of the <see cref="CommandHandler" /> class.
    /// </summary>
    /// <param name="config">The <see cref="IConfigData" /> for options set by the player.</param>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <param name="services">Provides access to internal and external services.</param>
    public CommandHandler(IConfigData config, IModHelper helper, IModServices services)
    {
        this.Config = config;
        this.Helper = helper;
        this._assetHandler = services.Lazy<AssetHandler>();
        this._craftFromChest = services.Lazy<CraftFromChest>();
        this._managedObjects = services.Lazy<ManagedObjects>();
        this._stashToChest = services.Lazy<StashToChest>();
        this.Helper.ConsoleCommands.Add(
            "better_chests_info",
            "Prints information to the logs about all storages managed by better chests.",
            this.DumpInfo);
    }

    private AssetHandler Assets
    {
        get => this._assetHandler.Value;
    }

    private IConfigData Config { get; }

    private CraftFromChest CraftFromChest
    {
        get => this._craftFromChest.Value;
    }

    private IModHelper Helper { get; }

    private ManagedObjects ManagedObjects
    {
        get => this._managedObjects.Value;
    }

    private StashToChest StashToChest
    {
        get => this._stashToChest.Value;
    }

    private static void AddStorageData(StringBuilder sb, IStorageData data, string storageName)
    {
        var dictData = SerializedStorageData.GetData(data);
        if (dictData.Values.All(string.IsNullOrWhiteSpace))
        {
            return;
        }

        CommandHandler.AppendHeader(sb, storageName);

        foreach (var (key, value) in dictData)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                sb.AppendFormat("{0,25}: {1}\n", key, value);
            }
        }
    }

    private static void AppendControls(StringBuilder sb, IControlScheme controls)
    {
        CommandHandler.AppendHeader(sb, "Controls");

        sb.AppendFormat(
            "{0,25}: {1}\n",
            nameof(controls.OpenCrafting),
            controls.OpenCrafting);

        sb.AppendFormat(
            "{0,25}: {1}\n",
            nameof(controls.StashItems),
            controls.StashItems);

        sb.AppendFormat(
            "{0,25}: {1}\n",
            nameof(controls.ScrollUp),
            controls.ScrollUp);

        sb.AppendFormat(
            "{0,25}: {1}\n",
            nameof(controls.ScrollDown),
            controls.ScrollDown);

        sb.AppendFormat(
            "{0,25}: {1}\n",
            nameof(controls.PreviousTab),
            controls.PreviousTab);

        sb.AppendFormat(
            "{0,25}: {1}\n",
            nameof(controls.NextTab),
            controls.NextTab);

        sb.AppendFormat(
            "{0,25}: {1}\n",
            nameof(controls.LockSlot),
            controls.LockSlot);
    }

    private static void AppendHeader(StringBuilder sb, string text)
    {
        sb.AppendFormat($"\n{{0,{(25 + text.Length / 2).ToString()}}}\n", text);
        sb.AppendFormat($"{{0,{(25 + text.Length / 2).ToString()}}}\n", new string('-', text.Length));
    }

    private void DumpConfig(StringBuilder sb)
    {
        // Main Header
        CommandHandler.AppendHeader(sb, "Mod Config");

        // Features
        sb.AppendFormat(
            "{0,25}: {1}\n",
            nameof(this.Config.CategorizeChest),
            this.Config.CategorizeChest.ToString());

        sb.AppendFormat(
            "{0,25}: {1}\n",
            nameof(this.Config.SlotLock),
            this.Config.SlotLock.ToString());

        // General
        sb.AppendFormat(
            "{0,25}: {1}\n",
            nameof(this.Config.CustomColorPickerArea),
            FormatHelper.GetAreaString(this.Config.CustomColorPickerArea));

        sb.AppendFormat(
            "{0,25}: {1}\n",
            nameof(this.Config.SearchTagSymbol),
            this.Config.SearchTagSymbol.ToString());

        // Control Scheme
        CommandHandler.AppendControls(sb, this.Config.ControlScheme);

        // Default Storage
        CommandHandler.AddStorageData(sb, this.Config.DefaultChest, "\"Default Chest\" Config");
    }

    private void DumpInfo(string command, string[] args)
    {
        var sb = new StringBuilder();

        // Main Header
        sb.AppendLine("Better Chests Info");

        // Log Config
        this.DumpConfig(sb);

        // Iterate known storages and features
        foreach (var (name, storageData) in this.Assets.ChestData)
        {
            CommandHandler.AddStorageData(sb, storageData, $"\"{name}\" Config");
        }

        var eligibleCraftingChests = this.CraftFromChest.EligibleChests.ToDictionary(managedChest => managedChest, _ => string.Empty);
        var eligibleStashingChests = this.StashToChest.EligibleStorages.ToDictionary(managedChest => managedChest, _ => string.Empty);

        // Iterate managed chests and features
        foreach (var ((player, index), managedStorage) in this.ManagedObjects.InventoryStorages)
        {
            CommandHandler.AddStorageData(sb, managedStorage, $"Storage {managedStorage.QualifiedItemId} with farmer {player.Name} at slot {index.ToString()}.\n");

            if (eligibleCraftingChests.Keys.Contains(managedStorage))
            {
                eligibleCraftingChests[managedStorage] = $"Inventory of {player.Name}.";
            }

            if (eligibleStashingChests.Keys.Contains(managedStorage))
            {
                eligibleStashingChests[managedStorage] = $"Inventory of {player.Name}.";
            }
        }

        foreach (var ((location, (x, y)), managedStorage) in this.ManagedObjects.LocationStorages)
        {
            CommandHandler.AddStorageData(sb, managedStorage, $"Storage \"{managedStorage.QualifiedItemId}\" at location {location.NameOrUniqueName} at coordinates ({((int)x).ToString()},{((int)y).ToString()}).");

            if (eligibleCraftingChests.Keys.Contains(managedStorage))
            {
                eligibleCraftingChests[managedStorage] = $"Location {location.NameOrUniqueName} at ({((int)x).ToString()},{((int)y).ToString()}).";
            }

            if (eligibleStashingChests.Keys.Contains(managedStorage))
            {
                eligibleStashingChests[managedStorage] = $"Location {location.NameOrUniqueName} at ({((int)x).ToString()},{((int)y).ToString()}).";
            }
        }

        CommandHandler.AppendHeader(sb, "Craft from Chests Eligible Chests");
        foreach (var (managedChest, description) in eligibleCraftingChests)
        {
            sb.AppendFormat("{0,25}: {1}\n", managedChest.QualifiedItemId, description);
        }

        CommandHandler.AppendHeader(sb, "Stash to Chest Eligible Chests");
        foreach (var (managedChest, description) in eligibleStashingChests)
        {
            sb.AppendFormat("{0,25}: {1}\n", managedChest.QualifiedItemId, description);
        }

        Log.Info(sb.ToString());
    }
}