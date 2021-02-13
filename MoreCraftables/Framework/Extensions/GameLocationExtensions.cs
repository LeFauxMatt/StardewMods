using MoreCraftables.API;
using StardewValley;

namespace MoreCraftables.Framework.Extensions
{
    internal static class GameLocationExtensions
    {
        public static void PlaySoundOrDefault(this GameLocation location, IHandledType handledType, string defaultSound)
        {
            if (handledType.Properties.TryGetValue("playSound", out var playSound) && playSound is string playSoundValue)
                location.playSound(playSoundValue);
            else
                location.playSound(defaultSound);
        }
    }
}