namespace StardewMods.EasyAccess;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewMods.Common.Services.Integrations.GenericModConfigMenu;
using StardewMods.Common.Services.Integrations.ToolbarIcons;

/// <inheritdoc />
public sealed class ModEntry : Mod
{
#nullable disable
    private ModConfig config;
#nullable enable

    /// <inheritdoc />
    public override void Entry(IModHelper helper)
    {
        I18n.Init(this.Helper.Translation);
        this.config = this.Helper.ReadConfig<ModConfig>();

        // Events
        this.Helper.Events.Content.AssetRequested += this.OnAssetRequested;
        this.Helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
        this.Helper.Events.Input.ButtonsChanged += this.OnButtonsChanged;
    }

    private void CollectItems()
    {
        var (pX, pY) = Game1.player.Tile;
        for (var tY = (int)(pY - this.config.CollectOutputDistance); tY <= (int)(pY + this.config.CollectOutputDistance); ++tY)
        {
            for (var tX = (int)(pX - this.config.CollectOutputDistance); tX <= (int)(pX + this.config.CollectOutputDistance); ++tX)
            {
                if (Math.Abs(tX - pX) + Math.Abs(tY - pY) > this.config.CollectOutputDistance)
                {
                    continue;
                }

                var pos = new Vector2(tX, tY);

                if (Game1.currentLocation.Objects.TryGetValue(pos, out var obj))
                {
                    // Dig Spot
                    if (this.config.DoDigSpots && obj.ParentSheetIndex == 590)
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
                    if (this.config.DoForage && obj.IsSpawnedObject && obj.isForage())
                    {
                        // Vanilla Logic
                        var r = new Random(((int)Game1.uniqueIDForThisGame / 2) + (int)Game1.stats.DaysPlayed + (int)pos.X + ((int)pos.Y * 777));
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
                        this.Monitor.Log($"Dropped {obj.DisplayName} from forage.", LogLevel.Info);
                        continue;
                    }

                    if (this.config.DoMachines)
                    {
                        var item = obj.heldObject.Value;
                        if (item is not null && obj.checkForAction(Game1.player))
                        {
                            this.Monitor.Log($"Collected {item.DisplayName} from producer {obj.DisplayName}.", LogLevel.Info);
                        }
                    }
                }

                if (!this.config.DoTerrain)
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

    private void DispenseInputs()
    {
        if (Game1.player.CurrentItem is null)
        {
            return;
        }

        var (pX, pY) = Game1.player.Tile;
        for (var tY = (int)(pY - this.config.DispenseInputDistance); tY <= (int)(pY + this.config.DispenseInputDistance); ++tY)
        {
            for (var tX = (int)(pX - this.config.DispenseInputDistance); tX <= (int)(pX + this.config.DispenseInputDistance); ++tX)
            {
                if (Math.Abs(tX - pX) + Math.Abs(tY - pY) > this.config.CollectOutputDistance)
                {
                    continue;
                }

                var pos = new Vector2(tX, tY);

                // Big Craftables
                if (!Game1.currentLocation.Objects.TryGetValue(pos, out var obj)
                    || (obj.Type?.Equals("Crafting", StringComparison.OrdinalIgnoreCase) != true && obj.Type?.Equals("interactive", StringComparison.OrdinalIgnoreCase) != true)
                    || !obj.performObjectDropInAction(Game1.player.CurrentItem, false, Game1.player))
                {
                    continue;
                }

                Game1.player.reduceActiveItemByOne();
                this.Monitor.Log($"Dispensed {Game1.player.CurrentItem.DisplayName} into producer {obj.DisplayName}.");
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
        if (!Context.IsPlayerFree)
        {
            return;
        }

        if (this.config.ControlScheme.CollectItems.JustPressed())
        {
            this.CollectItems();
            this.Helper.Input.SuppressActiveKeybinds(this.config.ControlScheme.CollectItems);
            return;
        }

        if (!this.config.ControlScheme.DispenseItems.JustPressed())
        {
            return;
        }

        this.DispenseInputs();
        this.Helper.Input.SuppressActiveKeybinds(this.config.ControlScheme.DispenseItems);
    }

    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        var gmcm = new GenericModConfigMenuIntegration(this.Helper.ModRegistry);
        var toolbarIcons = new ToolbarIconsIntegration(this.Helper.ModRegistry);

        if (gmcm.IsLoaded)
        {
            // Register mod configuration
            gmcm.Api.Register(this.ModManifest, () => this.config = new ModConfig(), () => this.Helper.WriteConfig(this.config));

            // Collect Items
            gmcm.Api.AddKeybindList(
                this.ModManifest,
                () => this.config.ControlScheme.CollectItems,
                value => this.config.ControlScheme.CollectItems = value,
                I18n.Config_CollectItems_Name,
                I18n.Config_CollectItems_Tooltip,
                nameof(Controls.CollectItems));

            // Dispense Items
            gmcm.Api.AddKeybindList(
                this.ModManifest,
                () => this.config.ControlScheme.DispenseItems,
                value => this.config.ControlScheme.DispenseItems = value,
                I18n.Config_DispenseItems_Name,
                I18n.Config_DispenseItems_Tooltip,
                nameof(Controls.DispenseItems));

            // Collect Output Distance
            gmcm.Api.AddNumberOption(
                this.ModManifest,
                () => this.config.CollectOutputDistance,
                value => this.config.CollectOutputDistance = value,
                I18n.Config_CollectOutputsDistance_Name,
                I18n.Config_CollectOutputsDistance_Tooltip,
                1,
                16,
                1,
                fieldId: nameof(ModConfig.CollectOutputDistance));

            // Dispense Input Distance
            gmcm.Api.AddNumberOption(
                this.ModManifest,
                () => this.config.DispenseInputDistance,
                value => this.config.DispenseInputDistance = value,
                I18n.Config_DispenseInputsDistance_Name,
                I18n.Config_DispenseInputsDistance_Tooltip,
                1,
                16,
                1,
                fieldId: nameof(ModConfig.DispenseInputDistance));

            // Do Dig Spots
            gmcm.Api.AddBoolOption(this.ModManifest, () => this.config.DoDigSpots, value => this.config.DoDigSpots = value, I18n.Config_DoDigSpots_Name, I18n.Config_DoDigSpots_Tooltip, nameof(ModConfig.DoDigSpots));

            // Do Forage
            gmcm.Api.AddBoolOption(this.ModManifest, () => this.config.DoForage, value => this.config.DoForage = value, I18n.Config_DoForage_Name, I18n.Config_DoForage_Tooltip, nameof(ModConfig.DoForage));

            // Do Machines
            gmcm.Api.AddBoolOption(this.ModManifest, () => this.config.DoMachines, value => this.config.DoMachines = value, I18n.Config_DoMachines_Name, I18n.Config_DoMachines_Tooltip, nameof(ModConfig.DoMachines));

            // Do Terrain
            gmcm.Api.AddBoolOption(this.ModManifest, () => this.config.DoTerrain, value => this.config.DoTerrain = value, I18n.Config_DoTerrain_Name, I18n.Config_DoTerrain_Tooltip, nameof(ModConfig.DoTerrain));
        }

        if (!toolbarIcons.IsLoaded)
        {
            return;
        }

        toolbarIcons.Api.AddToolbarIcon("EasyAccess.CollectItems", $"{this.ModManifest.UniqueID}/Icons", new Rectangle(0, 0, 16, 16), I18n.Button_CollectOutputs_Name());

        toolbarIcons.Api.AddToolbarIcon("EasyAccess.DispenseInputs", $"{this.ModManifest.UniqueID}/Icons", new Rectangle(16, 0, 16, 16), I18n.Button_DispenseInputs_Name());

        toolbarIcons.Api.ToolbarIconPressed += this.OnToolbarIconPressed;
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
