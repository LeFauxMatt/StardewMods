namespace StardewMods.ToolbarIcons.Helpers;

using System;
using System.Collections.Generic;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewMods.Common.Integrations.ToolbarIcons;

/// <summary>
///     Handles integrations with other mods.
/// </summary>
internal class IntegrationHelper
{
    private const string StardewAquariumId = "Cherry.StardewAquarium";

    private IReflectedMethod? _openCollectionsMenu;

    private IntegrationHelper(IModHelper helper, IToolbarIconsApi api)
    {
        this.Helper = helper;
        this.API = api;

        // Events
        this.Helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
        this.API.ToolbarIconPressed += this.OnToolbarIconPressed;
    }

    private static IntegrationHelper? Instance { get; set; }

    private IToolbarIconsApi API { get; }

    private IModHelper Helper { get; }

    private Dictionary<string, Action> Icons { get; } = new();

    /// <summary>
    ///     Initializes <see cref="IntegrationHelper" />.
    /// </summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <param name="api">API to add icons above or below the toolbar.</param>
    /// <returns>Returns an instance of the <see cref="IntegrationHelper" /> class.</returns>
    public static IntegrationHelper Init(IModHelper helper, IToolbarIconsApi api)
    {
        return IntegrationHelper.Instance ??= new(helper, api);
    }

    private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
    {
        if (this.Helper.ModRegistry.IsLoaded(IntegrationHelper.StardewAquariumId))
        {
            this.API.AddToolbarIcon(
                IntegrationHelper.StardewAquariumId,
                "furyx639.ToolbarIcons/Icons",
                new(16, 0, 16, 16),
                this.Helper.Translation.Get($"{IntegrationHelper.StardewAquariumId}.Button"));
            this.Icons.Add(IntegrationHelper.StardewAquariumId, this.StardewAquarium_OpenCollectionsMenu);
        }
    }

    private void OnToolbarIconPressed(object? sender, string id)
    {
        if (this.Icons.TryGetValue(id, out var action))
        {
            try
            {
                action.Invoke();
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }

    private void StardewAquarium_OpenCollectionsMenu()
    {
        if (this._openCollectionsMenu is not null)
        {
            this._openCollectionsMenu.Invoke(null, null);
            return;
        }

        var modInfo = this.Helper.ModRegistry.Get(IntegrationHelper.StardewAquariumId);
        var modEntry = (IMod?)modInfo?.GetType().GetProperty("Mod")?.GetValue(modInfo);
        if (modEntry is null)
        {
            return;
        }

        this._openCollectionsMenu = this.Helper.Reflection.GetMethod(modEntry, "OpenAquariumCollectionMenu");
        this._openCollectionsMenu.Invoke(null, null);
    }
}