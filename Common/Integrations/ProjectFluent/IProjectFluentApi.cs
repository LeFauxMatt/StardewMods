namespace StardewMods.Common.Integrations.ProjectFluent;

/// <summary>The Project Fluent API which other mods can access.</summary>
public interface IProjectFluentApi
{
    /// <summary>
    ///     Get an <see cref="IFluent{}" /> instance that allows retrieving Project Fluent translations for the current
    ///     locale.
    /// </summary>
    /// <remarks>The returned instance's locale will change automatically if the <see cref="CurrentLocale" /> changes.</remarks>
    /// <param name="mod">The mod for which to retrieve the translations.</param>
    /// <param name="file">An optional file name to retrieve the translations from.</param>
    IFluent<string> GetLocalizationsForCurrentLocale(IManifest mod, string? file = null);
}