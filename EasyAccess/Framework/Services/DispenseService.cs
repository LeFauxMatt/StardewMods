namespace StardewMods.EasyAccess.Framework.Services;

using Microsoft.Xna.Framework;
using StardewModdingAPI.Events;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.FuryCore;
using StardewMods.Common.Services.Integrations.ToolbarIcons;
using StardewMods.EasyAccess.Framework.Interfaces;

/// <summary>Handles dispensing items.</summary>
internal sealed class DispenseService : BaseService<DispenseService>
{
    private readonly IInputHelper inputHelper;
    private readonly IModConfig modConfig;

    /// <summary>Initializes a new instance of the <see cref="DispenseService" /> class.</summary>
    /// <param name="assetHandler">Dependency used for handling assets.</param>
    /// <param name="inputHelper">Dependency used for checking and changing input state.</param>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="modConfig">Dependency used for accessing config data.</param>
    /// <param name="modEvents">Dependency used for managing access to events.</param>
    /// <param name="toolbarIconsIntegration">Dependency for Toolbar Icons integration.</param>
    public DispenseService(
        AssetHandler assetHandler,
        IInputHelper inputHelper,
        ILog log,
        IManifest manifest,
        IModConfig modConfig,
        IModEvents modEvents,
        ToolbarIconsIntegration toolbarIconsIntegration)
        : base(log, manifest)
    {
        // Init
        this.inputHelper = inputHelper;
        this.modConfig = modConfig;

        // Events
        modEvents.Input.ButtonsChanged += this.OnButtonsChanged;

        if (!toolbarIconsIntegration.IsLoaded)
        {
            return;
        }

        toolbarIconsIntegration.Api.AddToolbarIcon(
            this.UniqueId,
            assetHandler.IconTexturePath,
            new Rectangle(16, 0, 16, 16),
            I18n.Button_DispenseInputs_Name());

        toolbarIconsIntegration.Api.IconPressed += this.OnIconPressed;
    }

    private void DispenseItems()
    {
        if (Game1.player.CurrentItem is null)
        {
            return;
        }

        var (pX, pY) = Game1.player.Tile;
        for (var tY = (int)(pY - this.modConfig.DispenseInputDistance);
            tY <= (int)(pY + this.modConfig.DispenseInputDistance);
            ++tY)
        {
            for (var tX = (int)(pX - this.modConfig.DispenseInputDistance);
                tX <= (int)(pX + this.modConfig.DispenseInputDistance);
                ++tX)
            {
                if (Math.Abs(tX - pX) + Math.Abs(tY - pY) > this.modConfig.CollectOutputDistance)
                {
                    continue;
                }

                var pos = new Vector2(tX, tY);

                // Big Craftables
                if (!Game1.currentLocation.Objects.TryGetValue(pos, out var obj)
                    || (obj.Type?.Equals("Crafting", StringComparison.OrdinalIgnoreCase) != true
                        && obj.Type?.Equals("interactive", StringComparison.OrdinalIgnoreCase) != true)
                    || !obj.performObjectDropInAction(Game1.player.CurrentItem, false, Game1.player))
                {
                    continue;
                }

                Game1.player.reduceActiveItemByOne();
                this.Log.Info(
                    "Dispensed {0} into producer {1}.",
                    [Game1.player.CurrentItem.DisplayName, obj.DisplayName]);
            }
        }
    }

    private void OnButtonsChanged(object? sender, ButtonsChangedEventArgs e)
    {
        if (!Context.IsPlayerFree || !this.modConfig.ControlScheme.DispenseItems.JustPressed())
        {
            return;
        }

        this.inputHelper.SuppressActiveKeybinds(this.modConfig.ControlScheme.DispenseItems);
        this.DispenseItems();
    }

    private void OnIconPressed(object? sender, IIconPressedEventArgs e)
    {
        if (e.Id == this.UniqueId)
        {
            this.DispenseItems();
        }
    }
}