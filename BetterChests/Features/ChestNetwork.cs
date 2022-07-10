namespace StardewMods.BetterChests.Features;

using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.BetterChests.Helpers;
using StardewMods.BetterChests.Storages;

/// <summary>
///     Access all chests from a single menu.
/// </summary>
internal class ChestNetwork : IFeature
{
    private const string Id = "furyx639.BetterChests/ChestNetwork";

    private readonly PerScreen<VirtualStorage> _virtualStorage = new();

    private ChestNetwork(IModHelper helper, ModConfig config)
    {
        this.Helper = helper;
        this.Config = config;
    }

    private static ChestNetwork? Instance { get; set; }

    private ModConfig Config { get; }

    private IModHelper Helper { get; }

    private bool IsActivated { get; set; }

    private VirtualStorage VirtualStorage
    {
        get => this._virtualStorage.Value;
        set => this._virtualStorage.Value = value;
    }

    /// <summary>
    ///     Initializes <see cref="ChestNetwork" />.
    /// </summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <param name="config">Mod config data.</param>
    /// <returns>Returns an instance of the <see cref="ChestNetwork" /> class.</returns>
    public static ChestNetwork Init(IModHelper helper, ModConfig config)
    {
        return ChestNetwork.Instance ??= new(helper, config);
    }

    /// <inheritdoc />
    public void Activate()
    {
        if (!this.IsActivated)
        {
            this.IsActivated = true;
            this.Helper.Events.Input.ButtonsChanged += this.OnButtonsChanged;

            if (IntegrationHelper.ToolbarIcons.IsLoaded)
            {
                IntegrationHelper.ToolbarIcons.API.AddToolbarIcon(
                    "BetterChests.ChestNetwork",
                    "furyx639.BetterChests/Icons",
                    new(64, 0, 16, 16),
                    I18n.Button_ChestNetwork_Name());
                IntegrationHelper.ToolbarIcons.API.ToolbarIconPressed += this.OnToolbarIconPressed;
            }
        }
    }

    /// <inheritdoc />
    public void Deactivate()
    {
        if (this.IsActivated)
        {
            this.IsActivated = false;
            this.Helper.Events.Input.ButtonsChanged -= this.OnButtonsChanged;

            if (IntegrationHelper.ToolbarIcons.IsLoaded)
            {
                IntegrationHelper.ToolbarIcons.API.RemoveToolbarIcon("BetterChests.ChestNetwork");
                IntegrationHelper.ToolbarIcons.API.ToolbarIconPressed -= this.OnToolbarIconPressed;
            }
        }
    }

    private void OnButtonsChanged(object? sender, ButtonsChangedEventArgs e)
    {
        if (!Context.IsPlayerFree || !this.Config.ControlScheme.OpenChestNetwork.JustPressed())
        {
            return;
        }

        this.Helper.Input.SuppressActiveKeybinds(this.Config.ControlScheme.OpenChestNetwork);
        this.OpenChestNetwork();
    }

    private void OnToolbarIconPressed(object? sender, string id)
    {
        if (id == "BetterChests.ChestNetwork")
        {
            this.OpenChestNetwork();
        }
    }

    private void OpenChestNetwork()
    {
        this.VirtualStorage = new(this.Config.DefaultChest);
        this.VirtualStorage.ShowMenu();
    }
}