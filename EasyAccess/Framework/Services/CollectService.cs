namespace StardewMods.EasyAccess.Framework.Services;

using Microsoft.Xna.Framework;
using StardewModdingAPI.Events;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.FuryCore;
using StardewMods.Common.Services.Integrations.ToolbarIcons;
using StardewMods.EasyAccess.Framework.Interfaces;

/// <summary>Handles collecting items.</summary>
internal sealed class CollectService : BaseService<CollectService>
{
    private readonly IInputHelper inputHelper;
    private readonly IModConfig modConfig;

    /// <summary>Initializes a new instance of the <see cref="CollectService" /> class.</summary>
    /// <param name="assetHandler">Dependency used for handling assets.</param>
    /// <param name="inputHelper">Dependency used for checking and changing input state.</param>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="modConfig">Dependency used for accessing config data.</param>
    /// <param name="modEvents">Dependency used for managing access to events.</param>
    /// <param name="toolbarIconsIntegration">Dependency for Toolbar Icons integration.</param>
    public CollectService(
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
            new Rectangle(0, 0, 16, 16),
            I18n.Button_CollectOutputs_Name());

        toolbarIconsIntegration.Api.ToolbarIconPressed += this.OnToolbarIconPressed;
    }

    private void CollectItems()
    {
        var (pX, pY) = Game1.player.Tile;
        for (var tY = (int)(pY - this.modConfig.CollectOutputDistance);
            tY <= (int)(pY + this.modConfig.CollectOutputDistance);
            ++tY)
        {
            for (var tX = (int)(pX - this.modConfig.CollectOutputDistance);
                tX <= (int)(pX + this.modConfig.CollectOutputDistance);
                ++tX)
            {
                if (Math.Abs(tX - pX) + Math.Abs(tY - pY) > this.modConfig.CollectOutputDistance)
                {
                    continue;
                }

                var pos = new Vector2(tX, tY);

                if (Game1.currentLocation.Objects.TryGetValue(pos, out var obj))
                {
                    // Dig Spot
                    if (this.modConfig.DoDigSpots && obj.ParentSheetIndex == 590)
                    {
                        Game1.currentLocation.digUpArtifactSpot(tX, tY, Game1.player);
                        if (!Game1.currentLocation.terrainFeatures.ContainsKey(pos))
                        {
                            Game1.currentLocation.makeHoeDirt(pos, true);
                        }

                        Game1.currentLocation.Objects.Remove(pos);
                        continue;
                    }

                    // Big Craftables
                    if (this.modConfig.DoForage && obj.IsSpawnedObject && obj.isForage())
                    {
                        // Vanilla Logic
                        var r = new Random(
                            ((int)Game1.uniqueIDForThisGame / 2)
                            + (int)Game1.stats.DaysPlayed
                            + (int)pos.X
                            + ((int)pos.Y * 777));

                        if (Game1.player.professions.Contains(16))
                        {
                            obj.Quality = 4;
                        }
                        else if (r.NextDouble() < Game1.player.ForagingLevel / 30f)
                        {
                            obj.Quality = 2;
                        }
                        else if (r.NextDouble() < Game1.player.ForagingLevel / 15f)
                        {
                            obj.Quality = 1;
                        }

                        ++Game1.stats.ItemsForaged;
                        if (Game1.currentLocation.isFarmBuildingInterior())
                        {
                            Game1.player.gainExperience(0, 5);
                        }
                        else
                        {
                            Game1.player.gainExperience(2, 7);
                        }

                        var direction = tY < pY
                            ? 0
                            : tX > pX
                                ? 1
                                : tY > pY
                                    ? 2
                                    : tX < pX
                                        ? 3
                                        : -1;

                        Game1.createItemDebris(obj, Game1.tileSize * pos, direction, Game1.currentLocation);
                        Game1.currentLocation.Objects.Remove(pos);
                        this.Log.Info("Dropped {0} from forage.", [obj.DisplayName]);
                        continue;
                    }

                    if (this.modConfig.DoMachines)
                    {
                        var item = obj.heldObject.Value;
                        if (item is not null && obj.checkForAction(Game1.player))
                        {
                            this.Log.Info("Collected {0} from producer {1}.", [item.DisplayName, obj.DisplayName]);
                        }
                    }
                }

                if (!this.modConfig.DoTerrain)
                {
                    continue;
                }

                // Terrain Features
                if (Game1.currentLocation.terrainFeatures.TryGetValue(pos, out var terrainFeature))
                {
                    terrainFeature.performUseAction(pos);
                }

                // Large Terrain Features
                terrainFeature = Game1.currentLocation.getLargeTerrainFeatureAt((int)pos.X, (int)pos.Y);
                terrainFeature?.performUseAction(pos);
            }
        }
    }

    private void OnButtonsChanged(object? sender, ButtonsChangedEventArgs e)
    {
        if (!Context.IsPlayerFree || this.modConfig.ControlScheme.CollectItems.JustPressed())
        {
            return;
        }

        this.inputHelper.SuppressActiveKeybinds(this.modConfig.ControlScheme.CollectItems);
        this.CollectItems();
    }

    private void OnToolbarIconPressed(object? sender, string id)
    {
        if (id == this.UniqueId)
        {
            this.CollectItems();
        }
    }
}