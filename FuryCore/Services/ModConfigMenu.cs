namespace StardewMods.FuryCore.Services;

using System;
using Common.Integrations.GenericModConfigMenu;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewMods.FuryCore.Enums;
using StardewMods.FuryCore.Interfaces;
using StardewMods.FuryCore.Models;

/// <inheritdoc />
internal class ModConfigMenu : IModService
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ModConfigMenu" /> class.
    /// </summary>
    /// <param name="config">The data for player configured mod options.</param>
    /// <param name="helper">SMAPI helper to read/save config data and for events.</param>
    /// <param name="manifest">The mod manifest to subscribe to GMCM with.</param>
    public ModConfigMenu(ConfigData config, IModHelper helper, IManifest manifest)
    {
        this.Config = config;
        this.Helper = helper;
        this.Manifest = manifest;
        this.GMCM = new(this.Helper.ModRegistry);
        this.Helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
    }

    private ConfigData Config { get; }

    private GenericModConfigMenuIntegration GMCM { get; }

    private IModHelper Helper { get; }

    private IManifest Manifest { get; }

    private static Func<string> GetTagName(CustomTag customTag)
    {
        return customTag switch
        {
            CustomTag.CategoryArtifact => I18n.Tag_CategoryArtifact_Name,
            CustomTag.CategoryFurniture => I18n.Tag_CategoryFurniture_Name,
            CustomTag.DonateBundle => I18n.Tag_DonateBundle_Name,
            CustomTag.DonateMuseum => I18n.Tag_DonateMuseum_Name,
            _ => throw new ArgumentOutOfRangeException(nameof(customTag), customTag, null),
        };
    }

    private static Func<string> GetTagTooltip(CustomTag customTag)
    {
        return customTag switch
        {
            CustomTag.CategoryArtifact => I18n.Tag_CategoryArtifact_Tooltip,
            CustomTag.CategoryFurniture => I18n.Tag_CategoryFurniture_Tooltip,
            CustomTag.DonateBundle => I18n.Tag_DonateBundle_Tooltip,
            CustomTag.DonateMuseum => I18n.Tag_DonateMuseum_Tooltip,
            _ => throw new ArgumentOutOfRangeException(nameof(customTag), customTag, null),
        };
    }

    private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
    {
        if (!this.GMCM.IsLoaded)
        {
            return;
        }

        this.GMCM.Register(
            this.Manifest,
            this.Reset,
            this.Save);

        this.GMCM.API.AddSectionTitle(
            this.Manifest,
            I18n.Section_General_Name,
            I18n.Section_General_Description);

        this.GMCM.API.AddBoolOption(
            this.Manifest,
            () => this.Config.ToolbarIcons,
            value => this.Config.ToolbarIcons = value,
            I18n.Config_ToolbarIcons_Name,
            I18n.Config_ToolbarIcons_Tooltip,
            nameof(ConfigData.ToolbarIcons));

        this.GMCM.API.AddSectionTitle(
            this.Manifest,
            I18n.Section_CustomTags_Name,
            I18n.Section_CustomTags_Tooltip);

        foreach (var customTag in Enum.GetValues<CustomTag>())
        {
            this.GMCM.API.AddBoolOption(
                this.Manifest,
                () => this.Config.CustomTags.Contains(customTag),
                value =>
                {
                    if (value)
                    {
                        this.Config.CustomTags.Add(customTag);
                        return;
                    }

                    this.Config.CustomTags.Remove(customTag);
                },
                ModConfigMenu.GetTagName(customTag),
                ModConfigMenu.GetTagTooltip(customTag));
        }
    }

    private void Reset()
    {
        new ConfigData().CopyTo(this.Config);
    }

    private void Save()
    {
        this.Helper.WriteConfig(this.Config);
    }
}