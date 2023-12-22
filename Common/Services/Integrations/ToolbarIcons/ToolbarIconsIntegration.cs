﻿namespace StardewMods.Common.Services.Integrations.ToolbarIcons;

/// <inheritdoc />
internal sealed class ToolbarIconsIntegration : ModIntegration<IToolbarIconsApi>
{
    private const string ModUniqueId = "furyx639.ToolbarIcons";

    /// <summary>Initializes a new instance of the <see cref="ToolbarIconsIntegration" /> class.</summary>
    /// <param name="modRegistry">SMAPI's mod registry.</param>
    public ToolbarIconsIntegration(IModRegistry modRegistry)
        : base(modRegistry, ToolbarIconsIntegration.ModUniqueId)
    {
        // Nothing
    }
}
