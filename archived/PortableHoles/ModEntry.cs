namespace StardewMods.PortableHoles;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewMods.Common.Helpers;
using StardewMods.Common.Integrations.GenericModConfigMenu;
using StardewMods.Common.Integrations.ToolbarIcons;
using StardewMods.PortableHoles.Framework;
using StardewValley.Locations;

/// <inheritdoc />
public sealed class ModEntry : Mod
{
    private ModConfig? config;

    private ModConfig Config => this.config ??= CommonHelpers.GetConfig<ModConfig>(this.Helper);

    /// <inheritdoc />
    public override void Entry(IModHelper helper)
    {
        // Init
        I18n.Init(this.Helper.Translation);
        Log.Monitor = this.Monitor;
        ModPatches.Init(this.Helper, this.ModManifest, this.Config);

        // Events
        this.Helper.Events.Content.AssetRequested += this.OnAssetRequested;
        this.Helper.Events.GameLoop.DayStarted += this.OnDayStarted;
        this.Helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
        this.Helper.Events.Input.ButtonPressed += this.OnButtonPressed;
    }

    private static void OnToolbarIconPressed(object? sender, string id)
    {
        switch (id)
        {
            case "PortableHoles.PlaceHole":
                ModEntry.TryToPlaceHole();
                return;
        }
    }

    private static bool TryToPlaceHole()
    {
        if (Game1.currentLocation is not MineShaft { mineLevel: > 120 } mineShaft
            || !mineShaft.shouldCreateLadderOnThisLevel())
        {
            return false;
        }

        var pos = CommonHelpers.GetCursorTile(1);
        mineShaft.createLadderDown((int)pos.X, (int)pos.Y, true);
        return true;
    }

    private void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
    {
        if (e.Name.IsEquivalentTo($"{this.ModManifest.UniqueID}/Icons"))
        {
            e.LoadFromModFile<Texture2D>("assets/icons.png", AssetLoadPriority.Exclusive);
            return;
        }

        if (e.Name.IsEquivalentTo($"{this.ModManifest.UniqueID}/Texture"))
        {
            e.LoadFromModFile<Texture2D>("assets/texture.png", AssetLoadPriority.Exclusive);
            return;
        }

        if (e.Name.IsEquivalentTo("Data/CraftingRecipes"))
        {
            e.Edit(
                asset =>
                {
                    var data = asset.AsDictionary<string, string>().Data;
                    data.Add("Portable Hole", $"769 99/Field/71/true/Mining 10/{I18n.Item_PortableHole_Name()}");
                });
        }
    }

    private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        if (!e.Button.IsUseToolButton()
            || Game1.player.CurrentItem is not SObject { bigCraftable.Value: true, ParentSheetIndex: 71 } obj
            || !obj.modData.ContainsKey($"{this.ModManifest.UniqueID}/PortableHole"))
        {
            return;
        }

        if (!ModEntry.TryToPlaceHole())
        {
            return;
        }

        Game1.player.reduceActiveItemByOne();
        this.Helper.Input.Suppress(e.Button);
    }

    private void OnDayStarted(object? sender, DayStartedEventArgs e)
    {
        if (this.Config.UnlockAutomatically && !Game1.player.craftingRecipes.ContainsKey("Portable Hole"))
        {
            Game1.player.craftingRecipes.Add("Portable Hole", 0);
        }
    }

    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        var toolbarIcons = new ToolbarIconsIntegration(this.Helper.ModRegistry);
        if (toolbarIcons.IsLoaded)
        {
            toolbarIcons.Api.AddToolbarIcon(
                "PortableHoles.PlaceHole",
                $"{this.ModManifest.UniqueID}/Icons",
                new Rectangle(0, 0, 16, 16),
                I18n.Button_PortableHole_Tooltip());

            toolbarIcons.Api.ToolbarIconPressed += ModEntry.OnToolbarIconPressed;
        }

        var gmcm = new GenericModConfigMenuIntegration(this.Helper.ModRegistry);
        if (!gmcm.IsLoaded)
        {
            return;
        }

        // Register mod configuration
        gmcm.Register(this.ModManifest, () => this.config = new(), () => this.Helper.WriteConfig(this.Config));

        // Soft Fall
        gmcm.Api.AddBoolOption(
            this.ModManifest,
            () => this.Config.SoftFall,
            value => this.Config.SoftFall = value,
            I18n.Config_SoftFall_Name,
            I18n.Config_SoftFall_Tooltip);

        // Unlock Automatically
        gmcm.Api.AddBoolOption(
            this.ModManifest,
            () => this.Config.UnlockAutomatically,
            value => this.Config.UnlockAutomatically = value,
            I18n.Config_UnlockAutomatically_Name,
            I18n.Config_UnlockAutomatically_Tooltip);
    }
}