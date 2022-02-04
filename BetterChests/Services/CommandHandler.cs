namespace StardewMods.BetterChests.Services;

using System;
using System.Linq;
using System.Text;
using Common.Helpers;
using StardewModdingAPI;
using StardewMods.BetterChests.Helpers;
using StardewMods.BetterChests.Interfaces;
using StardewMods.BetterChests.Models;
using StardewMods.FuryCore.Interfaces;
using StardewValley;

/// <inheritdoc />
internal class CommandHandler : IModService
{
    private readonly Lazy<AssetHandler> _assetHandler;
    private readonly Lazy<ManagedChests> _managedChests;

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
        this._managedChests = services.Lazy<ManagedChests>();
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

    private IModHelper Helper { get; }

    private ManagedChests ManagedChests
    {
        get => this._managedChests.Value;
    }

    private void AppendChestData(StringBuilder sb, IChestData data, string chestName)
    {
        var dictData = SerializedChestData.GetData(data);
        if (dictData.Values.All(string.IsNullOrWhiteSpace))
        {
            return;
        }

        this.AppendHeader(sb, chestName);

        foreach (var (key, value) in dictData)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                sb.AppendFormat("{0,25}: {1}\n", key, value);
            }
        }
    }

    private void AppendControls(StringBuilder sb, IControlScheme controls)
    {
        this.AppendHeader(sb, "Controls");

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

    private void AppendHeader(StringBuilder sb, string text)
    {
        sb.AppendFormat($"\n{{0,{(25 + text.Length / 2).ToString()}}}\n", text);
        sb.AppendFormat($"{{0,{(25 + text.Length / 2).ToString()}}}\n", new string('-', text.Length));
    }

    private void DumpConfig(StringBuilder sb)
    {
        // Main Header
        this.AppendHeader(sb, "Mod Config");

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
        this.AppendControls(sb, this.Config.ControlScheme);

        // Default Chest
        this.AppendChestData(sb, this.Config.DefaultChest, "\"Default Chest\" Config");
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
            this.AppendChestData(sb, chestData, $"\"{name}\" Config");
        }

        // Iterate managed chests and features
        foreach (var managedChest in this.ManagedChests.PlayerChests)
        {
            this.AppendChestData(sb, managedChest, $"\nChest {managedChest.QualifiedItemId} with farmer {Game1.player.Name}\n");
        }

        foreach (var ((location, pos), managedChest) in this.ManagedChests.PlacedChests)
        {
            this.AppendChestData(sb, managedChest, $"Chest \"{managedChest.QualifiedItemId}\" at location {location.NameOrUniqueName} at coordinates ({((int)pos.X).ToString()},{((int)pos.Y).ToString()})");
        }

        Log.Info(sb.ToString());
    }
}