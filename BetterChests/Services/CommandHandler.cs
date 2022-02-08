namespace StardewMods.BetterChests.Services;

using System;
using System.Linq;
using System.Text;
using Common.Helpers;
using StardewModdingAPI;
using StardewMods.BetterChests.Features;
using StardewMods.BetterChests.Helpers;
using StardewMods.BetterChests.Interfaces;
using StardewMods.BetterChests.Models;
using StardewMods.FuryCore.Interfaces;
using StardewValley;

/// <inheritdoc />
internal class CommandHandler : IModService
{
    private readonly Lazy<AssetHandler> _assetHandler;
    private readonly Lazy<CraftFromChest> _craftFromChest;
    private readonly Lazy<ManagedStorages> _managedChests;
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
        this._managedChests = services.Lazy<ManagedStorages>();
        this._stashToChest = services.Lazy<StashToChest>();
        this.Helper.ConsoleCommands.Add(
            "better_chests_info",
            "documentation",
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

    private ManagedStorages ManagedStorages
    {
        get => this._managedChests.Value;
    }

    private StashToChest StashToChest
    {
        get => this._stashToChest.Value;
    }

    private static void AppendChestData(StringBuilder sb, IStorageData data, string chestName)
    {
        var dictData = SerializedStorageData.GetData(data);
        if (dictData.Values.All(string.IsNullOrWhiteSpace))
        {
            return;
        }

        CommandHandler.AppendHeader(sb, chestName);

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

        // Default Chest
        CommandHandler.AppendChestData(sb, this.Config.DefaultChest, "\"Default Chest\" Config");
    }

    private void DumpInfo(string command, string[] args)
    {
        var sb = new StringBuilder();

        // Main Header
        sb.AppendLine("Better Chests Info");

        // Log Config
        this.DumpConfig(sb);

        // Iterate known chests and features
        foreach (var (name, chestData) in this.Assets.ChestData)
        {
            CommandHandler.AppendChestData(sb, chestData, $"\"{name}\" Config");
        }

        var eligibleCraftingChests = this.CraftFromChest.EligibleChests.ToDictionary(managedChest => managedChest, _ => string.Empty);
        var eligibleStashingChests = this.StashToChest.EligibleStorages.ToDictionary(managedChest => managedChest, _ => string.Empty);

        // Iterate managed chests and features
        foreach (var managedChest in this.ManagedStorages.PlayerStorages)
        {
            CommandHandler.AppendChestData(sb, managedChest, $"\nChest {managedChest.QualifiedItemId} with farmer {Game1.player.Name}.\n");

            if (eligibleCraftingChests.Keys.Contains(managedChest))
            {
                eligibleCraftingChests[managedChest] = $"Inventory of {Game1.player.Name}.";
            }

            if (eligibleStashingChests.Keys.Contains(managedChest))
            {
                eligibleStashingChests[managedChest] = $"Inventory of {Game1.player.Name}.";
            }
        }

        foreach (var ((location, (x, y)), managedChest) in this.ManagedStorages.LocationStorages)
        {
            CommandHandler.AppendChestData(sb, managedChest, $"Chest \"{managedChest.QualifiedItemId}\" at location {location.NameOrUniqueName} at coordinates ({((int)x).ToString()},{((int)y).ToString()}).");

            if (eligibleCraftingChests.Keys.Contains(managedChest))
            {
                eligibleCraftingChests[managedChest] = $"Location {location.NameOrUniqueName} at ({((int)x).ToString()},{((int)y).ToString()}).";
            }

            if (eligibleStashingChests.Keys.Contains(managedChest))
            {
                eligibleStashingChests[managedChest] = $"Location {location.NameOrUniqueName} at ({((int)x).ToString()},{((int)y).ToString()}).";
            }
        }

        // Craft from Chest Eligible Chests
        CommandHandler.AppendHeader(sb, "Craft from Chests Eligible Chests");
        foreach (var (managedChest, description) in eligibleCraftingChests)
        {
            sb.AppendFormat("{0,25}: {1}\n", managedChest.QualifiedItemId, description);
        }

        // Stash to Chest Eligible Chests
        CommandHandler.AppendHeader(sb, "Stash to Chest Eligible Chests");
        foreach (var (managedChest, description) in eligibleStashingChests)
        {
            sb.AppendFormat("{0,25}: {1}\n", managedChest.QualifiedItemId, description);
        }

        Log.Info(sb.ToString());
    }
}