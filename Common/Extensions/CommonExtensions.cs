using System;

namespace Common.Extensions
{
    public static class CommonExtensions
    {
        public static int RoundUp(this int i, int d = 1) => (int) (d * Math.Ceiling((float) i / d));
    }
}