namespace StardewMods.SpritePatcher.Framework.Services.Transient;

using System.Text;
using StardewMods.SpritePatcher.Framework.Interfaces;

/// <summary>Represents an object being managed by the mod.</summary>
internal sealed partial class ManagedObject
{
    private class CachedTokens(ManagedObject managedObject)
    {
        private readonly Dictionary<string, Dictionary<string, CachedToken>> cachedTokens = new();
        private readonly HashSet<string> unsupportedPatches = [];

        public bool Init(IPatchData patch)
        {
            if (this.unsupportedPatches.Contains(patch.Id))
            {
                return false;
            }

            if (this.cachedTokens.TryGetValue(patch.Id, out var tokens))
            {
                return true;
            }

            foreach (var (key, token) in patch.Tokens)
            {
                if (!managedObject.delegateManager.TryGetValue(
                    managedObject.entity,
                    token.RefersTo,
                    out var comparableValue))
                {
                    this.unsupportedPatches.Add(patch.Id);
                    return false;
                }

                tokens ??= new Dictionary<string, CachedToken>();
                tokens[key] = new CachedToken(comparableValue, token.Map);
            }

            this.cachedTokens[patch.Id] = tokens ?? new Dictionary<string, CachedToken>();
            return true;
        }

        public string Parse(IPatchData patch, string value)
        {
            // Return if tokens do not exist for this patch.
            if (!this.cachedTokens.TryGetValue(patch.Id, out var tokens))
            {
                return value;
            }

            var sb = new StringBuilder(value);
            foreach (var (find, replace) in tokens)
            {
                sb.Replace($"{{{find}}}", replace.LastValue);
            }

            return sb.ToString();
        }

        public bool Refresh(IPatchData patch) =>
            this.cachedTokens.TryGetValue(patch.Id, out var tokens)
            && tokens.Values.Aggregate(false, (current, token) => current | token.Refresh());

        public bool TryFindMatch(IPatchData patch, [NotNullWhen(true)] out IConditionalTexture? conditionalTexture)
        {
            // Return if tokens do not exist for this patch.
            if (!this.cachedTokens.TryGetValue(patch.Id, out var tokens))
            {
                conditionalTexture = null;
                return false;
            }

            // Try to get the first conditional texture where all of its conditions are met.
            conditionalTexture = patch.Textures.FirstOrDefault(TestConditions);
            return conditionalTexture is not null;

            // Verify that all conditions are met.
            bool TestConditions(IConditionalTexture conditionalTexture) =>
                conditionalTexture.Conditions.All(
                    condition =>
                        tokens.TryGetValue(condition.Key, out var token) && token.LastValue == condition.Value);
        }

        private sealed class CachedToken(IEquatable<string> comparableValue, Dictionary<string, string> map)
        {
            public string LastValue { get; private set; } = string.Empty;

            public bool Refresh()
            {
                foreach (var (key, value) in map)
                {
                    if (!comparableValue.Equals(value))
                    {
                        continue;
                    }

                    if (this.LastValue == key)
                    {
                        return false;
                    }

                    this.LastValue = key;
                    return true;
                }

                var newValue = comparableValue.ToString();
                if (newValue is null || this.LastValue == newValue)
                {
                    return false;
                }

                this.LastValue = newValue;
                return false;
            }
        }
    }
}