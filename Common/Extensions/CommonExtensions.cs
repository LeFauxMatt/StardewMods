namespace Common.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>Common extension methods.</summary>
    internal static class CommonExtensions
    {
        /// <summary>Invokes all event handlers assigned to an event.</summary>
        /// <param name="eventHandler">The event handler to invoke.</param>
        /// <param name="caller">The calling object.</param>
        public static void InvokeAll(this EventHandler eventHandler, object caller)
        {
            foreach (var @delegate in eventHandler.GetInvocationList())
            {
                @delegate.DynamicInvoke(caller, null);
            }
        }

        /// <summary>Rounds an int up to the next int by an interval.</summary>
        /// <param name="i">The integer to round up from.</param>
        /// <param name="d">The interval to round up to.</param>
        /// <returns>Returns the rounded value.</returns>
        public static int RoundUp(this int i, int d = 1)
        {
            return (int)(d * Math.Ceiling((float)i / d));
        }

        /// <summary>Shuffles a list randomly.</summary>
        /// <param name="source">The list to shuffle.</param>
        /// <typeparam name="T">The list type.</typeparam>
        /// <returns>Returns a shuffled list.</returns>
        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
        {
            return source.Shuffle(new());
        }

        private static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source, Random rng)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (rng is null)
            {
                throw new ArgumentNullException(nameof(rng));
            }

            return source.ShuffleIterator(rng);
        }

        private static IEnumerable<T> ShuffleIterator<T>(this IEnumerable<T> source, Random rng)
        {
            var buffer = source.ToList();
            for (var i = 0; i < buffer.Count; i++)
            {
                var j = rng.Next(i, buffer.Count);
                yield return buffer[j];
                buffer[j] = buffer[i];
            }
        }
    }
}