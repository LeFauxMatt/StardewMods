namespace StardewMods.SpritePatcher.Framework.Services;

using StardewMods.Common.Interfaces;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.FuryCore;
using StardewMods.Common.Services.Integrations.GenericModConfigMenu;
using StardewMods.SpritePatcher.Framework.Enums.Patches;
using StardewMods.SpritePatcher.Framework.Interfaces;
using StardewMods.SpritePatcher.Framework.Models;

/// <inheritdoc cref="StardewMods.SpritePatcher.Framework.Interfaces.IModConfig" />
internal sealed class ConfigManager : ConfigManager<DefaultConfig>, IModConfig
{
    private readonly GenericModConfigMenuIntegration genericModConfigMenuIntegration;
    private readonly ILog log;
    private readonly IManifest manifest;
    private readonly ITranslationHelper translationHelper;

    /// <summary>Initializes a new instance of the <see cref="ConfigManager" /> class.</summary>
    /// <param name="eventPublisher">Dependency used for publishing events.</param>
    /// <param name="genericModConfigMenuIntegration">Dependency for Generic Mod Config Menu integration.</param>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="modHelper">Dependency for events, input, and content.</param>
    /// <param name="translationHelper">Dependency used for managing translations.</param>
    public ConfigManager(
        IEventPublisher eventPublisher,
        GenericModConfigMenuIntegration genericModConfigMenuIntegration,
        ILog log,
        IManifest manifest,
        IModHelper modHelper,
        ITranslationHelper translationHelper)
        : base(eventPublisher, modHelper)
    {
        this.genericModConfigMenuIntegration = genericModConfigMenuIntegration;
        this.log = log;
        this.manifest = manifest;
        this.translationHelper = translationHelper;
        this.InitializeConfig();

        if (this.genericModConfigMenuIntegration.IsLoaded)
        {
            this.SetupMainConfig();
        }
    }

    /// <inheritdoc />
    public bool DeveloperMode => this.Config.DeveloperMode;

    /// <inheritdoc />
    public Dictionary<AllPatches, bool> PatchedObjects => this.Config.PatchedObjects;

    /// <summary>Retrieves the value associated with the specified patch.</summary>
    /// <param name="patch">The patch to retrieve the value for.</param>
    /// <returns>Whether the patch is enabled or <c>true</c> by default.</returns>
    public bool GetValue(AllPatches patch)
    {
        if (this.PatchedObjects.TryGetValue(patch, out var value))
        {
            return value;
        }

        this.PatchedObjects[patch] = true;
        return true;
    }

    private void InitializeConfig()
    {
        foreach (var anyPatch in AllPatchesExtensions.GetValues())
        {
            this.PatchedObjects.TryAdd(anyPatch, true);
        }
    }

    private bool GetValue(string patchType)
    {
        if (!AllPatchesExtensions.TryParse(patchType, out var patch))
        {
            return false;
        }

        if (this.PatchedObjects.TryGetValue(patch, out var value))
        {
            return value;
        }

        this.PatchedObjects[patch] = true;
        return true;
    }

    private void SetValue(string patchType, bool value)
    {
        if (!AllPatchesExtensions.TryParse(patchType, out var patch))
        {
            return;
        }

        this.PatchedObjects[patch] = value;
    }

    private void SetupMainConfig()
    {
        if (!this.genericModConfigMenuIntegration.IsLoaded)
        {
            return;
        }

        var gmcm = this.genericModConfigMenuIntegration.Api;
        var config = this.GetNew();
        this.genericModConfigMenuIntegration.Register(this.Reset, () => this.Save(config));

        gmcm.AddBoolOption(
            this.manifest,
            () => this.Config.DeveloperMode,
            value => this.Config.DeveloperMode = value,
            I18n.Config_DeveloperMode_Name,
            I18n.Config_DeveloperMode_Tooltip);

        gmcm.AddPageLink(this.manifest, "Buildings", I18n.Section_Buildings_Name, I18n.Section_Buildings_Description);
        gmcm.AddParagraph(this.manifest, I18n.Section_Buildings_Description);

        gmcm.AddPageLink(
            this.manifest,
            "Characters",
            I18n.Section_Characters_Name,
            I18n.Section_Characters_Description);

        gmcm.AddParagraph(this.manifest, I18n.Section_Characters_Description);

        gmcm.AddPageLink(this.manifest, "Objects", I18n.Section_Items_Name, I18n.Section_Items_Description);
        gmcm.AddParagraph(this.manifest, I18n.Section_Items_Description);

        gmcm.AddPageLink(
            this.manifest,
            "TerrainFeatures",
            I18n.Section_TerrainFeatures_Name,
            I18n.Section_TerrainFeatures_Description);

        gmcm.AddParagraph(this.manifest, I18n.Section_TerrainFeatures_Description);

        gmcm.AddPageLink(this.manifest, "Tools", I18n.Section_Tools_Name, I18n.Section_Tools_Description);
        gmcm.AddParagraph(this.manifest, I18n.Section_Tools_Description);

        gmcm.AddPage(this.manifest, "Buildings", I18n.Section_Buildings_Name);

        foreach (var buildingPatch in BuildingPatchesExtensions.GetValues())
        {
            gmcm.AddBoolOption(
                this.manifest,
                () => this.GetValue(buildingPatch.ToStringFast()),
                value => this.SetValue(buildingPatch.ToStringFast(), value),
                () => this.translationHelper.Get($"config.{buildingPatch.ToStringFast()}.name"),
                () => this.translationHelper.Get($"config.{buildingPatch.ToStringFast()}.tooltip"));
        }

        gmcm.AddPage(this.manifest, "Characters", I18n.Section_Characters_Name);

        foreach (var characterPatch in CharacterPatchesExtensions.GetValues())
        {
            gmcm.AddBoolOption(
                this.manifest,
                () => this.GetValue(characterPatch.ToStringFast()),
                value => this.SetValue(characterPatch.ToStringFast(), value),
                () => this.translationHelper.Get($"config.{characterPatch.ToStringFast()}.name"),
                () => this.translationHelper.Get($"config.{characterPatch.ToStringFast()}.tooltip"));
        }

        gmcm.AddPage(this.manifest, "Objects", I18n.Section_Items_Name);

        foreach (var itemPatch in ItemPatchesExtensions.GetValues())
        {
            gmcm.AddBoolOption(
                this.manifest,
                () => this.GetValue(itemPatch.ToStringFast()),
                value => this.SetValue(itemPatch.ToStringFast(), value),
                () => this.translationHelper.Get($"config.{itemPatch.ToStringFast()}.name"),
                () => this.translationHelper.Get($"config.{itemPatch.ToStringFast()}.tooltip"));
        }

        gmcm.AddPage(this.manifest, "TerrainFeatures", I18n.Section_TerrainFeatures_Name);

        foreach (var terrainFeaturePatches in TerrainFeaturePatchesExtensions.GetValues())
        {
            gmcm.AddBoolOption(
                this.manifest,
                () => this.GetValue(terrainFeaturePatches.ToStringFast()),
                value => this.SetValue(terrainFeaturePatches.ToStringFast(), value),
                () => this.translationHelper.Get($"config.{terrainFeaturePatches.ToStringFast()}.name"),
                () => this.translationHelper.Get($"config.{terrainFeaturePatches.ToStringFast()}.tooltip"));
        }

        gmcm.AddPage(this.manifest, "Tools", I18n.Section_Tools_Name);

        foreach (var toolPatches in ToolPatchesExtensions.GetValues())
        {
            gmcm.AddBoolOption(
                this.manifest,
                () => this.GetValue(toolPatches.ToStringFast()),
                value => this.SetValue(toolPatches.ToStringFast(), value),
                () => this.translationHelper.Get($"config.{toolPatches.ToStringFast()}.name"),
                () => this.translationHelper.Get($"config.{toolPatches.ToStringFast()}.tooltip"));
        }
    }
}