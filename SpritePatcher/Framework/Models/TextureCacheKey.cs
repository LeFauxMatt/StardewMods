namespace StardewMods.SpritePatcher.Framework.Models;

using System.Text;

/// <summary>Represents the key used to cache textures in the application.</summary>
internal readonly struct TextureCacheKey
{
    private readonly SpriteKey key;

    /// <summary>Gets the list of layer IDs.</summary>
    private readonly List<int> layerIds;

    /// <summary>Initializes a new instance of the <see cref="TextureCacheKey" /> struct.</summary>
    /// <param name="key">A key for the original texture method.</param>
    /// <param name="layerIds">The layer ids.</param>
    public TextureCacheKey(SpriteKey key, List<int> layerIds)
    {
        this.key = key;
        this.layerIds = layerIds;
    }

    public static bool operator ==(TextureCacheKey left, TextureCacheKey right) => left.Equals(right);

    public static bool operator !=(TextureCacheKey left, TextureCacheKey right) => !(left == right);

    /// <inheritdoc />
    public override bool Equals(object? obj) =>
        obj is TextureCacheKey textureCacheKey && this.GetHashCode() == textureCacheKey.GetHashCode();

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var hash = default(HashCode);
        hash.Add(this.key.Target);
        hash.Add(this.key.Area);
        hash.Add(this.key.DrawMethod);
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
        sb.Append(this.key.Target);
        sb.Append('_');
        sb.Append(this.key.Area);
        sb.Append('_');
        sb.Append(this.key.DrawMethod);
        sb.Append('_');
        sb.Append(string.Join('_', this.layerIds));
        return sb.ToString();
    }
}