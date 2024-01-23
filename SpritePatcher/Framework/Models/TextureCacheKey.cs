namespace StardewMods.SpritePatcher.Framework.Models;

using System.Text;

/// <summary>Represents the key used to cache textures in the application.</summary>
internal readonly struct TextureCacheKey
{
    /// <summary>Gets the list of layer IDs.</summary>
    private readonly List<int> layerIds;

    /// <summary>Initializes a new instance of the <see cref="TextureCacheKey" /> struct.</summary>
    /// <param name="layerIds">The layer ids.</param>
    public TextureCacheKey(List<int> layerIds) => this.layerIds = layerIds;

    public static bool operator ==(TextureCacheKey left, TextureCacheKey right) => left.Equals(right);

    public static bool operator !=(TextureCacheKey left, TextureCacheKey right) => !(left == right);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is TextureCacheKey key && this.layerIds.SequenceEqual(key.layerIds);

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var hash = default(HashCode);
        foreach (var layerId in this.layerIds)
        {
            hash.Add(layerId);
        }

        return hash.ToHashCode();
    }

    /// <inheritdoc />
    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append(string.Join('_', this.layerIds));
        return sb.ToString();
    }
}