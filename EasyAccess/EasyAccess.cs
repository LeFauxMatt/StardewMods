namespace StardewMods.EasyAccess;

using System;
using Common.Helpers;
using Common.Integrations.GenericModConfigMenu;
using Common.Integrations.ToolbarIcons;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewMods.EasyAccess.Interfaces.Config;
using StardewMods.EasyAccess.Models.Config;
using StardewValley;

/// <inheritdoc />
public class EasyAccess : Mod
{
    private ConfigModel? _config;

    private ConfigModel Config
    {
        get
        {
            if (this._config is not null)
            {
                return this._config;
            }

            // Mod Config
            IConfigData? config = null;
            try
            {
                config = this.Helper.ReadConfig<ConfigData>();
            }
            catch (Exception)
            {
                // ignored
            }

            this._config = new(config ?? new ConfigData(), this.Helper);
            return this._config;
        }
    }

    /// <inheritdoc />
    public override void Entry(IModHelper helper)
    {
        I18n.Init(helper.Translation);
        Log.Monitor = this.Monitor;

        // Events
        this.Helper.Events.Content.AssetRequested += this.OnAssetRequested;
        this.Helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
        this.Helper.Events.Input.ButtonsChanged += this.OnButtonsChanged;
    }

    private void CollectItems()
    {
        var (pX, pY) = Game1.player.getTileLocation();
        for (var tY = (int)(pY - this.Config.CollectOutputDistance); tY <= (int)(pY + this.Config.CollectOutputDistance); tY++)
        {
            for (var tX = (int)(pX - this.Config.CollectOutputDistance); tX <= (int)(pX + this.Config.CollectOutputDistance); tX++)
            {
                if (Math.Abs(tX - pX) + Math.Abs(tY - pY) > this.Config.CollectOutputDistance)
                {
                    continue;
                }

                var pos = new Vector2(tX, tY);

                if (Game1.currentLocation.Objects.TryGetValue(pos, out var obj))
                {
                    // Dig Spot
                    if (this.Config.DoDigSpots && obj.ParentSheetIndex == 590)
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
                    if (this.Config.DoForage && obj.IsSpawnedObject && obj.isForage(Game1.currentLocation))
                    {
                        // Vanilla Logic
                        var r = new Random((int)Game1.uniqueIDForThisGame / 2 + (int)Game1.stats.DaysPlayed + (int)pos.X + (int)pos.Y * 777);
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

                        Game1.stats.ItemsForaged++;
                        if (Game1.currentLocation.isFarmBuildingInterior())
                        {
                            Game1.player.gainExperience(0, 5);
                        }
                        else
                        {
                            Game1.player.gainExperience(2, 7);
                        }

                        Game1.createItemDebris(obj, 64 * pos, tY < pY ? 0 : tX > pX ? 1 : tY > pY ? 2 : tX < pX ? 3 : -1, Game1.currentLocation);
                        Game1.currentLocation.Objects.Remove(pos);
                        Log.Info($"Dropped {obj.DisplayName} from forage.");
                        continue;
                    }

                    if (this.Config.DoMachines)
                    {
                        var item = obj.heldObject.Value;
                        if (item is not null && obj.checkForAction(Game1.player))
                        {
                            Log.Info($"Collected {item.DisplayName} from producer {obj.DisplayName}.");
                        }
                    }
                }

                if (!this.Config.DoTerrain)
                {
                    continue;
                }

                // Terrain Features
                if (Game1.currentLocation.terrainFeatures.TryGetValue(pos, out var terrainFeature))
                {
                    terrainFeature.performUseAction(pos, Game1.currentLocation);
                }

                // Large Terrain Features
                terrainFeature = Game1.currentLocation.getLargeTerrainFeatureAt((int)pos.X, (int)pos.Y);
                terrainFeature?.performUseAction(pos, Game1.currentLocation);
            }
        }
    }

    private void DispenseInputs()
    {
        if (Game1.player.CurrentItem is null)
        {
            return;
        }

        var (pX, pY) = Game1.player.getTileLocation();
        for (var tY = (int)(pY - this.Config.DispenseInputDistance); tY <= (int)(pY + this.Config.DispenseInputDistance); tY++)
        {
            for (var tX = (int)(pX - this.Config.DispenseInputDistance); tX <= (int)(pX + this.Config.DispenseInputDistance); tX++)
            {
                if (Math.Abs(tX - pX) + Math.Abs(tY - pY) > this.Config.CollectOutputDistance)
                {
                    continue;
                }

                var pos = new Vector2(tX, tY);

                // Big Craftables
                if (Game1.currentLocation.Objects.TryGetValue(pos, out var obj)
                    && (obj.Type?.Equals("Crafting") == true || obj.Type?.Equals("interactive") == true)
                    && obj.performObjectDropInAction(Game1.player.CurrentItem, false, Game1.player))
                {
                    Game1.player.reduceActiveItemByOne();
                    Log.Trace($"Dispensed {Game1.player.CurrentItem.DisplayName} into producer {obj.DisplayName}.");
                }
            }
        }
    }

    private void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
    {
        if (e.Name.IsEquivalentTo($"{this.ModManifest.UniqueID}/Icons"))
        {
            e.LoadFromModFile<Texture2D>("assets/icons.png", AssetLoadPriority.Exclusive);
        }
    }

    private void OnButtonsChanged(object? sender, ButtonsChangedEventArgs e)
    {
        if (this.Config.ControlScheme.CollectItems.JustPressed())
        {
            this.CollectItems();
            this.Helper.Input.SuppressActiveKeybinds(this.Config.ControlScheme.CollectItems);
            return;
        }

        if (this.Config.ControlScheme.DispenseItems.JustPressed())
        {
            this.DispenseInputs();
            this.Helper.Input.SuppressActiveKeybinds(this.Config.ControlScheme.DispenseItems);
        }
    }

    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        var gmcm = new GenericModConfigMenuIntegration(this.Helper.ModRegistry);
        var toolbarIcons = new ToolbarIconsIntegration(this.Helper.ModRegistry);

        if (gmcm.IsLoaded)
        {
            // Register mod configuration
            gmcm.Register(
                this.ModManifest,
                () => { this.Config.Reset(); },
                () => { this.Config.Save(); });

            // Collect Items
            gmcm.API!.AddKeybindList(
                this.ModManifest,
                () => this.Config.ControlScheme.CollectItems,
                value => this.Config.ControlScheme.CollectItems = value,
                I18n.Config_CollectItems_Name,
                I18n.Config_CollectItems_Tooltip,
                nameof(IControlScheme.CollectItems));

            // Dispense Items
            gmcm.API.AddKeybindList(
                this.ModManifest,
                () => this.Config.ControlScheme.DispenseItems,
                value => this.Config.ControlScheme.DispenseItems = value,
                I18n.Config_DispenseItems_Name,
                I18n.Config_DispenseItems_Tooltip,
                nameof(IControlScheme.DispenseItems));

            // Collect Output Distance
            gmcm.API.AddNumberOption(
                this.ModManifest,
                () => this.Config.CollectOutputDistance,
                value => this.Config.CollectOutputDistance = value,
                I18n.Config_CollectOutputsDistance_Name,
                I18n.Config_CollectOutputsDistance_Tooltip,
                1,
                16,
                1,
                fieldId: nameof(IConfigData.CollectOutputDistance));

            // Dispense Input Distance
            gmcm.API.AddNumberOption(
                this.ModManifest,
                () => this.Config.DispenseInputDistance,
                value => this.Config.DispenseInputDistance = value,
                I18n.Config_DispenseInputsDistance_Name,
                I18n.Config_DispenseInputsDistance_Tooltip,
                1,
                16,
                1,
                fieldId: nameof(IConfigData.DispenseInputDistance));

            // Do Dig Spots
            gmcm.API.AddBoolOption(
                this.ModManifest,
                () => this.Config.DoDigSpots,
                value => this.Config.DoDigSpots = value,
                I18n.Config_DoDigSpots_Name,
                I18n.Config_DoDigSpots_Tooltip,
                nameof(IConfigData.DoDigSpots));

            // Do Forage
            gmcm.API.AddBoolOption(
                this.ModManifest,
                () => this.Config.DoForage,
                value => this.Config.DoForage = value,
                I18n.Config_DoForage_Name,
                I18n.Config_DoForage_Tooltip,
                nameof(IConfigData.DoForage));

            // Do Machines
            gmcm.API.AddBoolOption(
                this.ModManifest,
                () => this.Config.DoMachines,
                value => this.Config.DoMachines = value,
                I18n.Config_DoMachines_Name,
                I18n.Config_DoMachines_Tooltip,
                nameof(IConfigData.DoMachines));

            // Do Terrain
            gmcm.API.AddBoolOption(
                this.ModManifest,
                () => this.Config.DoTerrain,
                value => this.Config.DoTerrain = value,
                I18n.Config_DoTerrain_Name,
                I18n.Config_DoTerrain_Tooltip,
                nameof(IConfigData.DoTerrain));
        }

        if (toolbarIcons.IsLoaded)
        {
            toolbarIcons.API!.AddToolbarIcon(
                "EasyAccess.CollectItems",
                $"{this.ModManifest.UniqueID}/Icons",
                new Rectangle(0, 0, 16, 16),
                I18n.Button_CollectOutputs_Name());

            toolbarIcons.API.AddToolbarIcon(
                "EasyAccess.DispenseInputs",
                $"{this.ModManifest.UniqueID}/Icons",
                new Rectangle(16, 0, 16, 16),
                I18n.Button_DispenseInputs_Name());

            toolbarIcons.API.ToolbarIconPressed += this.OnToolbarIconPressed;
        }
    }

    private void OnToolbarIconPressed(object? sender, string id)
    {
        switch (id)
        {
            case "EasyAccess.CollectItems":
                this.CollectItems();
                return;
            case "EasyAccess.DispenseInputs":
                this.DispenseInputs();
                return;
        }
    }
}