using System;

// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global

namespace Common.Extensions
{
    public static class CommonExtensions
    {
        public static int RoundUp(this int i, int d = 1)
        {
            return (int) (d * Math.Ceiling((float) i / d));
        }
    }
}