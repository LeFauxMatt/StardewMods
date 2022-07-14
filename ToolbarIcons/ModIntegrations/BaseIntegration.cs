namespace StardewMods.ToolbarIcons.ModIntegrations;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using StardewModdingAPI;
using StardewMods.Common.Integrations.ToolbarIcons;

/// <summary>
///     Base class for adding toolbar icons for integrated mods.
/// </summary>
internal abstract class BaseIntegration
{
    private const string IconPath = "furyx639.ToolbarIcons/Icons";

    /// <summary>
    ///     Initializes a new instance of the <see cref="BaseIntegration" /> class.
    /// </summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <param name="api">API to add icons above or below the toolbar.</param>
    protected BaseIntegration(IModHelper helper, IToolbarIconsApi api)
    {
        this.Helper = helper;
        this.API = api;
        this.API.ToolbarIconPressed += this.OnToolbarIconPressed;
    }

    /// <summary>
    ///     Gets the SMAPI helper for events, input, and content.
    /// </summary>
    protected IModHelper Helper { get; }

    private IToolbarIconsApi API { get; }

    private Dictionary<string, Action> Icons { get; } = new();

    /// <summary>
    ///     Adds a toolbar icon for an integrated mod.
    /// </summary>
    /// <param name="modId">The id of the mod.</param>
    /// <param name="index">The index of the mod icon.</param>
    /// <param name="hoverText">The text to display.</param>
    /// <param name="action">The action to perform for this icon.</param>
    /// <param name="texturePath">The texture path of the icon.</param>
    /// <returns>Returns true if the icon was added.</returns>
    protected bool AddIntegration(string modId, int index, string hoverText, Action action, string? texturePath = null)
    {
        this.API.AddToolbarIcon(
            $"{modId}.{index.ToString(CultureInfo.InvariantCulture)}",
            texturePath ?? BaseIntegration.IconPath,
            new(16 * index, 0, 16, 16),
            hoverText);
        this.Icons.Add($"{modId}.{index.ToString(CultureInfo.InvariantCulture)}", action);
        return true;
    }

    /// <summary>
    ///     Tries to get the instance of a mod based on the mod id.
    /// </summary>
    /// <param name="modId">The unique id of the mod.</param>
    /// <param name="mod">The mod instance.</param>
    /// <returns>Returns true if the mod instance could be obtained.</returns>
    protected bool TryGetMod(
        string modId,
        [NotNullWhen(true)] out IMod? mod)
    {
        if (!this.Helper.ModRegistry.IsLoaded(modId))
        {
            mod = null;
            return false;
        }

        var modInfo = this.Helper.ModRegistry.Get(modId);
        mod = (IMod?)modInfo?.GetType().GetProperty("Mod")?.GetValue(modInfo);
        return mod is not null;
    }

    private void OnToolbarIconPressed(object? sender, string id)
    {
        if (this.Icons.TryGetValue(id, out var action))
        {
            action.Invoke();
        }
    }
}