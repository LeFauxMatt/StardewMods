using System;

namespace ImJustMatt.Common.Extensions
{
    internal static class CommonExtensions
    {
        public static int RoundUp(this int i, int d = 1)
        {
            return (int) (d * Math.Ceiling((float) i / d));
        }
    }
}