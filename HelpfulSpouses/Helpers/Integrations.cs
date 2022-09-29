namespace StardewMods.HelpfulSpouses.Helpers;

using System;
using System.Collections.Generic;
using StardewMods.Common.Integrations.ContentPatcher;

/// <summary>
///     Handles integrations with other mods.
/// </summary>
internal sealed class Integrations
{
    private static Integrations? Instance;

    private readonly ContentPatcherIntegration _contentPatcher;
    private readonly IManifest _manifest;

    private Integrations(IModHelper helper, IManifest manifest)
    {
        this._manifest = manifest;
        this._contentPatcher = new(helper.ModRegistry);

        Integrations.RegisterToken("TermOfEndearment", () => spouse);
    }

    /// <summary>
    ///     Initializes <see cref="Integrations" />.
    /// </summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <param name="manifest">Manifest for the SMAPI mod.</param>
    /// <returns>Returns an instance of the <see cref="Integrations" /> class.</returns>
    public static Integrations Init(IModHelper helper, IManifest manifest)
    {
        return Integrations.Instance ??= new(helper, manifest);
    }

    /// <inheritdoc cref="IContentPatcherApi.RegisterToken(IManifest, string, Func{IEnumerable{string}})" />
    public static void RegisterToken(string name, Func<string?> getValue)
    {
        IEnumerable<string>? GetValue()
        {
            var value = getValue();
            return value is null ? null : new[] { value };
        }

        Integrations.Instance?._contentPatcher.API?.RegisterToken(Integrations.Instance._manifest, name, GetValue);
    }
}