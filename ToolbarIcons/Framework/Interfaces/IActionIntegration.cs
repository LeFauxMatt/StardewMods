namespace StardewMods.ToolbarIcons.Framework.Interfaces;

/// <summary>Represents an integration with a custom action.</summary>
internal interface IActionIntegration : ICustomIntegration
{
    /// <summary>Gets the unique mod id for the integration.</summary>
    string ModId { get; }

    /// <summary>Retrieves the custom action associated with the integration.</summary>
    /// <param name="mod">Mod info for the integration.</param>
    /// <returns>The custom action associated with the integration.</returns>
    public Action? GetAction(IMod mod);
}