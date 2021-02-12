using Common.Integration.MoreCraftables;
using StardewValley;

namespace Common.Extensions
{
    public static class GameLocationExtensions
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