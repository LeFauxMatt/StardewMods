namespace StardewMods.Common.Services.Integrations.ProjectFluent;

/// <summary>The Project Fluent API which other mods can access.</summary>
public interface IProjectFluentApi
{
    // #region Locale
    //
    // /// <summary>The default locale of the game (en-US).</summary>
    // IGameLocale DefaultLocale { get; }
    //
    // /// <summary>The current locale of the game.</summary>
    // IGameLocale CurrentLocale { get; }
    //
    // /// <summary>All of the currently known locales.</summary>
    // IEnumerable<IGameLocale> AllKnownLocales { get; }
    //
    // /// <summary>Get a locale built into the vanilla game.</summary>
    // /// <remarks>The <see cref="LocalizedContentManager.LanguageCode.mod"/> is not a valid argument for this method, as it is not a real language code.</remarks>
    // /// <param name="languageCode">A built-in locale language code.</param>
    // IGameLocale GetBuiltInLocale(LocalizedContentManager.LanguageCode languageCode);
    //
    // /// <summary>Get a locale provided by a mod.</summary>
    // /// <param name="language">A mod-provided language.</param>
    // IGameLocale GetModLocale(ModLanguage language);
    //
    // #endregion

    #region Localizations

    /// <summary>
    ///     Get an <see cref="IFluent{}" /> instance that allows retrieving Project Fluent translations for the current
    ///     locale.
    /// </summary>
    /// <remarks>The returned instance's locale will change automatically if the <see cref="CurrentLocale" /> changes.</remarks>
    /// <param name="mod">The mod for which to retrieve the translations.</param>
    /// <param name="file">An optional file name to retrieve the translations from.</param>
    IFluent<string> GetLocalizationsForCurrentLocale(IManifest mod, string? file = null);

    #endregion

    // #region Specialized types
    //
    // /// <summary>Get a specialized <see cref="IEnumFluent{}"/> instance that allows retrieving Project Fluent translations for an <see cref="Enum"/> type.</summary>
    // /// <typeparam name="EnumType">The <see cref="Enum"/> type to retrieve translations for.</typeparam>
    // /// <param name="baseFluent">The underlying <see cref="IFluent{}"/> instance.</param>
    // /// <param name="keyPrefix">The prefix all of the <see cref="Enum"/> values are prefixed with.</param>
    // IEnumFluent<EnumType> GetEnumFluent<EnumType>(IFluent<string> baseFluent, string keyPrefix) where EnumType : struct, Enum;
    //
    // /// <summary>Get a specialized <see cref="IFluent{}"/> instance that allows retrieving Project Fluent translations for values of a given generic type Input.</summary>
    // /// <typeparam name="T">The type to retrieve translations for.</typeparam>
    // /// <param name="baseFluent">The underlying <see cref="IFluent{}"/> instance.</param>
    // /// <param name="mapper">The (Input -> Output) mapper.</param>
    // IFluent<Input> GetMappingFluent<Input, Output>(IFluent<Output> baseFluent, Func<Input, Output> mapper);
    //
    // #endregion

    // #region Custom Fluent functions
    //
    // /// <summary>Create a <see cref="string"/> value usable with Project Fluent functions.</summary>
    // /// <param name="value">The <see cref="string"/> value.</param>
    // IFluentFunctionValue CreateStringValue(string value);
    //
    // /// <summary>Create a <see cref="int"/> value usable with Project Fluent functions.</summary>
    // /// <param name="value">The <see cref="int"/> value.</param>
    // IFluentFunctionValue CreateIntValue(int value);
    //
    // /// <summary>Create a <see cref="long"/> value usable with Project Fluent functions.</summary>
    // /// <param name="value">The <see cref="long"/> value.</param>
    // IFluentFunctionValue CreateLongValue(long value);
    //
    // /// <summary>Create a <see cref="float"/> value usable with Project Fluent functions.</summary>
    // /// <param name="value">The <see cref="float"/> value.</param>
    // IFluentFunctionValue CreateFloatValue(float value);
    //
    // /// <summary>Create a <see cref="double"/> value usable with Project Fluent functions.</summary>
    // /// <param name="value">The <see cref="double"/> value.</param>
    // IFluentFunctionValue CreateDoubleValue(double value);
    //
    // /// <summary>Register a new function for the Project Fluent translations.</summary>
    // /// <remarks>
    // /// The registered function will be available by the translations in the form of:
    // /// <list type="bullet">
    // /// <item><c>CAPITALIZED_UNDERSCORED_MOD_ID_AND_THEN_THE_FUNCTION_NAME</c> in all contexts</item>
    // /// <item><c>FUNCTION_NAME</c> in the <paramref name="mod"/> context.</item>
    // /// </list>
    // /// </remarks>
    // /// <param name="mod">The mod you want to register the function for. Keep in mind other mods can also access the function, if they provide the fully qualified name.</param>
    // /// <param name="name">The name of the function.<br/>Fluent function names can only contain uppercase letters, digits, and the <c>_</c> and <c>-</c> characters. They must also start with an uppercase letter.</param>
    // /// <param name="function">The function to register.</param>
    // void RegisterFunction(IManifest mod, string name, FluentFunction function);
    //
    // /// <summary>Unregister a Project Fluent translation function.</summary>
    // /// <param name="mod">The mod you want to unregister the function for.</param>
    // /// <param name="name">The name of the function.<br/>Fluent function names can only contain uppercase letters, digits, and the <c>_</c> and <c>-</c> characters. They must also start with an uppercase letter.</param>
    // void UnregisterFunction(IManifest mod, string name);
    //
    // #endregion
}
