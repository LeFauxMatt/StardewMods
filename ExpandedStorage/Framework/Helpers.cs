namespace StardewMods.ExpandedStorage.Framework;

/// <summary>
///     Common helpers for Expanded Storage.
/// </summary>
internal sealed class Helpers
{
#nullable disable
    private static Helpers Instance;
#nullable enable

    private Helpers() { }

    public static Helpers Init()
    {
        return Helpers.Instance ??= new();
    }
}