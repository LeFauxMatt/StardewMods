namespace StardewMods.BetterChests.Framework.Models.StorageOptions;

using StardewValley.Mods;

/// <inheritdoc />
internal sealed class ModDataStorageOptions : DictionaryStorageOptions
{
    private readonly ModDataDictionary modData;

    /// <summary>Initializes a new instance of the <see cref="ModDataStorageOptions" /> class.</summary>
    /// <param name="modData">The mod data dictionary.</param>
    public ModDataStorageOptions(ModDataDictionary modData) => this.modData = modData;

    /// <inheritdoc />
    protected override bool TryGetValue(string key, [NotNullWhen(true)] out string? value) =>
        this.modData.TryGetValue(key, out value);

    /// <inheritdoc />
    protected override void SetValue(string key, string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            this.modData.Remove(key);
            return;
        }

        this.modData[key] = value;
    }
}