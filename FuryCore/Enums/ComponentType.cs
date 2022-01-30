namespace FuryCore.Enums;

using StardewValley.Menus;

/// <summary>
///     <see cref="ClickableTextureComponent" /> that are added to the <see cref="ItemGrabMenu" />
/// </summary>
public enum ComponentType
{
    /// <summary>A custom component.</summary>
    Custom,

    /// <summary>The Organize Button.</summary>
    OrganizeButton,

    /// <summary>The Fill Stacks Button.</summary>
    FillStacksButton,

    /// <summary>The Color Picker Toggle Button.</summary>
    ColorPickerToggleButton,

    /// <summary>The Special Button.</summary>
    SpecialButton,

    /// <summary>The Junimo Note Icon.</summary>
    JunimoNoteIcon,
}