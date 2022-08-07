namespace StardewMods.Common.Helpers;

using Microsoft.Xna.Framework;
using StardewMods.Common.Enums;

/// <summary>
///     Helpers for conversions to/from <see cref="Color" />.
/// </summary>
internal static class ColorHelper
{
    /// <summary>
    ///     Returns a Color representation of an item's context tag.
    /// </summary>
    /// <param name="colorTag">The context tag to return the Color from.</param>
    /// <returns>The Color of the context tag or Gray.</returns>
    public static Color FromTag(string colorTag)
    {
        return colorTag switch
        {
            "color_red" => Color.Red,
            "color_dark_red" => Color.DarkRed,
            "color_pale_violet_red" => Color.PaleVioletRed,
            "color_blue" => Color.Blue,
            "color_green" => Color.Green,
            "color_dark_green" => Color.DarkGreen,
            "color_jade" => Color.Teal,
            "color_brown" => Color.Brown,
            "color_dark_brown" => Color.Maroon,
            "color_yellow" => Color.Yellow,
            "color_dark_yellow" => Color.Goldenrod,
            "color_aquamarine" => Color.Aquamarine,
            "color_purple" => Color.Purple,
            "color_dark_purple" => Color.Indigo,
            "color_cyan" => Color.Cyan,
            "color_pink" => Color.Pink,
            "color_orange" => Color.Orange,
            _ => Color.Gray,
        };
    }

    /// <summary>
    ///     Returns a Color representation of the <see cref="Colors" /> enum..
    /// </summary>
    /// <param name="color">The color to return the Color from.</param>
    /// <returns>The Color of the context tag or Gray.</returns>
    public static Color ToColor(this Colors color)
    {
        return color switch
        {
            Colors.Red => Color.Red,
            Colors.DarkRed => Color.DarkRed,
            Colors.PaleVioletRed => Color.PaleVioletRed,
            Colors.Blue => Color.Blue,
            Colors.Green => Color.Green,
            Colors.DarkGreen => Color.DarkGreen,
            Colors.Jade => Color.Teal,
            Colors.Brown => Color.Brown,
            Colors.DarkBrown => Color.Maroon,
            Colors.Yellow => Color.Yellow,
            Colors.DarkYellow => Color.Goldenrod,
            Colors.Aquamarine => Color.Aquamarine,
            Colors.Purple => Color.Purple,
            Colors.DarkPurple => Color.Indigo,
            Colors.Cyan => Color.Cyan,
            Colors.Pink => Color.Pink,
            Colors.Orange => Color.Orange,
            _ => Color.Gray,
        };
    }
}