namespace StardewMods.ToolbarIcons.Framework.Services;

using StardewModdingAPI.Events;
using StardewMods.Common.Services;

/// <inheritdoc />
internal sealed class ThemeHelper : BaseThemeHelper
{
    /// <summary>Initializes a new instance of the <see cref="ThemeHelper" /> class.</summary>
    /// <param name="events">Dependency used for managing access to events.</param>
    /// <param name="gameContent">Dependency used for loading game assets.</param>
    public ThemeHelper(IModEvents events, IGameContentHelper gameContent)
        : base(events, gameContent, AssetHandler.IconPath, AssetHandler.ArrowsPath) { }
}
