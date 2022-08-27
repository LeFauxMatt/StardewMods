namespace StardewMods.Common.Integrations.ProjectFluent;

/// <summary>A type allowing access to Project Fluent translations.</summary>
/// <typeparam name="TKey">The type of values this instance allows retrieving translations for.</typeparam>
public interface IFluent<in TKey>
{
    /// <summary>Returns a translation for a given key.</summary>
    /// <param name="key">The key to retrieve a translation for.</param>
    string this[TKey key] =>
        this.Get(key, null);

    /// <summary>Returns whether a given key has a translation provided.</summary>
    /// <param name="key">The key to retrieve a translation for.</param>
    bool ContainsKey(TKey key);

    /// <summary>Returns a translation for a given key.</summary>
    /// <param name="key">The key to retrieve a translation for.</param>
    string Get(TKey key)
    {
        return this.Get(key, null);
    }

    /// <summary>Returns a translation for a given key.</summary>
    /// <param name="key">The key to retrieve a translation for.</param>
    /// <param name="tokens">
    ///     An object containing token key/value pairs. This can be an anonymous object (like
    ///     <c>new { value = 42, name = "Cranberries" }</c>), a dictionary, or a class instance.
    /// </param>
    string Get(TKey key, object? tokens);
}