#nullable disable

namespace StardewMods.EasyAccess;

using System;
using Common.Helpers;
using Common.Integrations.GenericModConfigMenu;
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
    private ConfigModel Config { get; set; }

    /// <inheritdoc />
    public override void Entry(IModHelper helper)
    {
        I18n.Init(helper.Translation);
        Log.Monitor = this.Monitor;

        // Mod Config
        IConfigData config = null;
        try
        {
            config = this.Helper.ReadConfig<ConfigData>();
        }
        catch (Exception)
        {
            // ignored
        }

        this.Config = new(config ?? new ConfigData(), this.Helper);

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

                // Big Craftables
                if (Game1.currentLocation.Objects.TryGetValue(pos, out var obj))
                {
                    if (obj.IsSpawnedObject && obj.isForage(Game1.currentLocation))
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
                    }

                    var item = obj.heldObject.Value;
                    if (item is not null && obj.checkForAction(Game1.player))
                    {
                        Log.Info($"Collected {item.DisplayName} from producer {obj.DisplayName}.");
                    }
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
                    && (obj.Type.Equals("Crafting") || obj.Type.Equals("interactive"))
                    && obj.performObjectDropInAction(Game1.player.CurrentItem, false, Game1.player))
                {
                    Game1.player.reduceActiveItemByOne();
                    Log.Trace($"Dispensed {Game1.player.CurrentItem.DisplayName} into producer {obj.DisplayName}.");
                }
            }
        }
    }

    private void OnAssetRequested(object sender, AssetRequestedEventArgs e)
    {
        if (e.Name.IsEquivalentTo($"{this.ModManifest.UniqueID}/Icons"))
        {
            e.LoadFromModFile<Texture2D>("assets/icons.png", AssetLoadPriority.Exclusive);
        }
    }

    private void OnButtonsChanged(object sender, ButtonsChangedEventArgs e)
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

    private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
    {
        var gmcm = new GenericModConfigMenuIntegration(this.Helper.ModRegistry);

        if (!gmcm.IsLoaded)
        {
            return;
        }

        // Register mod configuration
        gmcm.Register(
            this.ModManifest,
            () => { this.Config.Reset(); },
            () => { this.Config.Save(); });

        gmcm.API.AddSectionTitle(this.ModManifest, I18n.Section_Features_Name, I18n.Section_Features_Description);

        // Collect Items
        gmcm.API.AddKeybindList(
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
    }
}