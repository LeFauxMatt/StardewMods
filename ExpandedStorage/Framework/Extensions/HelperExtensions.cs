using System;

namespace ExpandedStorage.Framework.Extensions
{
    public static class HelperExtensions
    {
        public static int RoundUp(this int i, int d = 1)
        {
            return (int) (d * Math.Ceiling((float) i / d));
        }
    }
}