namespace StardewMods.Common.Services.Integrations.ToolbarIcons;

using StardewMods.Common.Interfaces;

#pragma warning disable CA1711

/// <summary>Represents the event arguments for a toolbar icon being pressed.</summary>
public interface IIconPressedEventArgs
{
    /// <summary>Gets the id of the icon that was pressed.</summary>
    string Id { get; }

    /// <summary>Gets the button that was pressed.</summary>
    SButton Button { get; }
}