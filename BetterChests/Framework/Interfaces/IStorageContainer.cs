namespace StardewMods.BetterChests.Framework.Interfaces;

using StardewMods.Common.Services.Integrations.BetterChests.Interfaces;

/// <inheritdoc />
/// <typeparam name="TSource">The source object type.</typeparam>
internal interface IStorageContainer<TSource> : IStorageContainer
    where TSource : class
{
    /// <summary>Gets a value indicating whether the source object is still alive.</summary>
    public bool IsAlive { get; }

    /// <summary>Gets a weak reference to the source object.</summary>
    public WeakReference<TSource> Source { get; }
}