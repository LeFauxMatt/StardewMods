namespace StardewMods.SpritePatcher.Framework.Services.Transient;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.SpritePatcher.Framework.Enums;

/// <summary>Represents an object being managed by the mod.</summary>
internal sealed partial class ManagedObject
{
    private class CachedTextures(ManagedObject managedObject)
    {
        private readonly IDictionary<string, IDictionary<(Rectangle? Area, DrawMethod Method), Texture2D>>
            cachedTextures =
                new Dictionary<string, IDictionary<(Rectangle? Area, DrawMethod Method), Texture2D>>(
                    StringComparer.OrdinalIgnoreCase);

        private readonly IDictionary<string, HashSet<(Rectangle? Area, DrawMethod Method)>> disabledTextures =
            new Dictionary<string, HashSet<(Rectangle? Area, DrawMethod Method)>>(StringComparer.OrdinalIgnoreCase);

        public void AddOrUpdate(string target, Rectangle? area, DrawMethod method, Texture2D texture)
        {
            if (!this.cachedTextures.TryGetValue(target, out var textures))
            {
                textures = new Dictionary<(Rectangle? Area, DrawMethod Method), Texture2D>();
                this.cachedTextures[target] = textures;
            }

            textures[(area, method)] = texture;
        }

        public void ClearCache(IEnumerable<string> targets)
        {
            foreach (var target in targets)
            {
                this.cachedTextures.Remove(target);
                this.disabledTextures.Remove(target);
            }
        }

        public void Disable(string target, Rectangle? area, DrawMethod method)
        {
            if (!this.disabledTextures.TryGetValue(target, out var textures))
            {
                textures = new HashSet<(Rectangle? Area, DrawMethod Method)>();
                this.disabledTextures[target] = textures;
            }

            textures.Add((area, method));
        }

        public bool TryGet(string target, Rectangle? area, DrawMethod method, out Texture2D? texture)
        {
            texture = null;
            return (this.cachedTextures.TryGetValue(target, out var enabled)
                    && enabled.TryGetValue((area, method), out texture))
                || (this.disabledTextures.TryGetValue(target, out var disabled) && disabled.Contains((area, method)));
        }
    }
}