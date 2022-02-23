namespace StardewMods.FuryCore.Services;

using System;
using StardewModdingAPI;
using StardewMods.FuryCore.Interfaces;
using StardewValley;

/// <inheritdoc />
internal class CommandHandler : IModService
{
    private const string ConfigToolCommand = "furycore_config_tool";
    private readonly Lazy<ConfigureGameObject> _configureGameObject;

    /// <summary>
    ///     Initializes a new instance of the <see cref="CommandHandler" /> class.
    /// </summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <param name="services">Provides access to internal and external services.</param>
    public CommandHandler(IModHelper helper, IModServices services)
    {
        this.Helper = helper;
        this._configureGameObject = services.Lazy<ConfigureGameObject>();
        this.Helper.ConsoleCommands.Add(
            CommandHandler.ConfigToolCommand,
            I18n.Command_ConfigTool_Documentation(),
            this.AddConfigTool);
    }

    private ConfigureGameObject ConfigureGameObject
    {
        get => this._configureGameObject.Value;
    }

    private IModHelper Helper { get; }

    private void AddConfigTool(string command, string[] args)
    {
        var tool = this.ConfigureGameObject.GetConfigTool();
        Game1.player.addItemToInventory(tool);
    }
}