namespace Common.Extensions;

using StardewModdingAPI;
using StardewModdingAPI.Utilities;

/// <summary>
/// 
/// </summary>
internal static class KeybindListExtensions
{
    /// <inheritdoc cref="IInputHelper" />
    public static IInputHelper InputHelper { get; set; }

    /// <inheritdoc cref="IInputHelper.SuppressActiveKeybinds" />
    public static void Suppress(this KeybindList keybindList)
    {
        KeybindListExtensions.InputHelper.SuppressActiveKeybinds(keybindList);
    }
}