namespace StardewMods.SpritePatcher.Framework.Services;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Models;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.FuryCore;
using StardewMods.SpritePatcher.Framework.Enums;
using StardewMods.SpritePatcher.Framework.Interfaces;
using StardewMods.SpritePatcher.Framework.Models;
using StardewMods.SpritePatcher.Framework.Models.Events;

/// <summary>Helps build a texture object from patches.</summary>
internal sealed class TextureBuilder : BaseService
{
    private readonly AssetHandler assetHandler;
    private readonly Dictionary<string, Texture2D> cachedTextures = [];
    private readonly Dictionary<string, HashSet<string>> cachedTextureKeys = [];
    private readonly DelegateManager delegateManager;
    private readonly IGameContentHelper gameContentHelper;

    /// <summary>Initializes a new instance of the <see cref="TextureBuilder" /> class.</summary>
    /// <param name="assetHandler">Dependency used for managing icons.</param>
    /// <param name="delegateManager">Dependency used for getting item properties.</param>
    /// <param name="eventSubscriber">Dependency used for subscribing to events.</param>
    /// <param name="gameContentHelper">Dependency used for loading game assets.</param>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    public TextureBuilder(
        AssetHandler assetHandler,
        DelegateManager delegateManager,
        IEventSubscriber eventSubscriber,
        IGameContentHelper gameContentHelper,
        ILog log,
        IManifest manifest)
        : base(log, manifest)
    {
        this.assetHandler = assetHandler;
        this.delegateManager = delegateManager;
        this.gameContentHelper = gameContentHelper;
        eventSubscriber.Subscribe<PatchesChangedEventArgs>(this.OnPatchesChanged);
    }

    /// <summary>Tries to get a modified texture for the given entity using patches and conditions.</summary>
    /// <param name="entity">The entity to get the texture for.</param>
    /// <param name="baseTexture">The base texture to modify.</param>
    /// <param name="sourceRect">The rectangle defining the area of the texture to modify.</param>
    /// <param name="drawMethod">The draw method to apply the texture to.</param>
    /// <param name="texture">The modified texture, if successful; otherwise, null.</param>
    /// <returns>True if a modified texture was found and applied; otherwise, false.</returns>
    public bool TryGetTexture(
        IHaveModData entity,
        Texture2D baseTexture,
        Rectangle sourceRect,
        DrawMethod drawMethod,
        [NotNullWhen(true)] out Texture2D? texture)
    {
        // Check if any patches may apply to this texture
        if (!this.assetHandler.TryGetData(baseTexture.Name, out var patches))
        {
            texture = null;
            return false;
        }

        // Check if the texture is in cache
        var modDataKey = this.ModId + "/" + baseTexture.Name + "/" + sourceRect + "/" + drawMethod;
        if (entity.modData.TryGetValue(modDataKey, out var cachedTextureName))
        {
            if (this.cachedTextures.TryGetValue(cachedTextureName, out texture))
            {
                return true;
            }

            entity.modData.Remove(modDataKey);
        }

        // Attempt to build the texture
        Color[]? data = null;
        var initialized = false;
        cachedTextureName = string.Empty;
        foreach (var patch in patches.Values)
        {
            // Check if the patch applies to this draw method
            if (!patch.DrawMethods.Contains(drawMethod))
            {
                continue;
            }

            // Check if specific target is applicable
            if (patch.SourceRect is not null && patch.SourceRect != sourceRect)
            {
                continue;
            }

            // Check if all tokens are applicable to this entity
            if (!this.TryGetTokens(entity, patch.Tokens, out var tokens))
            {
                continue;
            }

            // Find first texture that matches all conditions
            var patchTexture = patch.Textures.FirstOrDefault(TextureBuilder.TestConditions(tokens));
            if (patchTexture is null)
            {
                continue;
            }

            // Get path to texture patch
            var path = TextureBuilder.ParseTokens(patch.Path, tokens);

            // Apply texture patch
            texture = this.gameContentHelper.Load<Texture2D>(path);
            var patchData = new Color[sourceRect.Width * sourceRect.Height];
            var patchArea = patchTexture.FromArea ?? new Rectangle(0, 0, sourceRect.Width, sourceRect.Height);
            texture.GetData(0, patchArea, patchData, 0, patchData.Length);

            // Apply tinting
            if (patchTexture.Tint is not null)
            {
                var tint = HslColor.FromColor(patchTexture.Tint.Value);
                var blendColor = new HslColor(tint.H, 2f * tint.S, 2f * tint.L).ToRgbColor();
                for (var i = 0; i < patchData.Length; ++i)
                {
                    if (patchData[i].A <= 0)
                    {
                        continue;
                    }

                    var multiplyColor = new Color(
                        patchData[i].R / 255f * patchTexture.Tint.Value.R / 255f,
                        patchData[i].G / 255f * patchTexture.Tint.Value.G / 255f,
                        patchData[i].B / 255f * patchTexture.Tint.Value.B / 255f);

                    patchData[i] = Color.Lerp(multiplyColor, blendColor, 0.3f);
                }
            }

            switch (patch.PatchMode)
            {
                case PatchMode.Replace:
                    initialized = true;
                    data = patchData;
                    break;

                default:
                    data ??= new Color[sourceRect.Width * sourceRect.Height];
                    if (!initialized)
                    {
                        initialized = true;
                        baseTexture.GetData(0, sourceRect, data, 0, data.Length);
                    }

                    for (var i = 0; i < data.Length; ++i)
                    {
                        if (patchData[i].A > 0)
                        {
                            data[i] = patchData[i];
                        }
                    }

                    break;
            }

            cachedTextureName += path + patchArea + patchTexture.Tint + ",";
        }

        if (!initialized)
        {
            entity.modData[modDataKey] = "Disabled";
            texture = null;
            return false;
        }

        texture = new Texture2D(baseTexture.GraphicsDevice, sourceRect.Width, sourceRect.Height);
        texture.SetData(data);
        this.cachedTextures[cachedTextureName] = texture;
        entity.modData[modDataKey] = cachedTextureName;
        if (!this.cachedTextureKeys.TryGetValue(baseTexture.Name, out var cachedTextureNames))
        {
            cachedTextureNames = new HashSet<string>();
            this.cachedTextureKeys[baseTexture.Name] = cachedTextureNames;
        }

        cachedTextureNames.Add(cachedTextureName);
        return true;
    }

    private static string ParseTokens(string value, IReadOnlyDictionary<string, Token> tokens)
    {
        var result = value;
        foreach (var (key, token) in tokens)
        {
            result = result.Replace($"{{{key}}}", token.ToString());
        }

        return result;
    }

    private static Func<IConditionalTexture, bool> TestConditions(IReadOnlyDictionary<string, Token> tokens) =>
        texture =>
        {
            foreach (var (key, aliasValue) in texture.Conditions)
            {
                if (!tokens.TryGetValue(key, out var token))
                {
                    return false;
                }

                var originalValue = token.Map.GetValueOrDefault(aliasValue, aliasValue);
                if (!token.Value.Equals(originalValue))
                {
                    return false;
                }
            }

            return true;
        };

    private void OnPatchesChanged(PatchesChangedEventArgs e)
    {
        foreach (var target in e.ChangedTargets)
        {
            if (!this.cachedTextureKeys.TryGetValue(target, out var cachedTextureNames))
            {
                continue;
            }

            foreach (var cachedTextureName in cachedTextureNames)
            {
                this.cachedTextures.Remove(cachedTextureName);
            }

            this.cachedTextureKeys.Remove(target);
        }
    }

    private bool TryGetTokens(
        IHaveModData entity,
        Dictionary<string, TokenDefinition> tokenDefinitions,
        [NotNullWhen(true)] out Dictionary<string, Token>? tokens)
    {
        tokens = [];
        foreach (var (key, tokenDefinition) in tokenDefinitions)
        {
            if (!this.delegateManager.TryGetValue(entity, tokenDefinition.RefersTo, out var comparableValue))
            {
                return false;
            }

            var originalValue = comparableValue.ToString() ?? string.Empty;
            var aliasValue = tokenDefinition.Map.FirstOrDefault(kvp => kvp.Value == originalValue).Key;
            tokens[key] = new Token(comparableValue, tokenDefinition.Map, aliasValue ?? originalValue);
        }

        return true;
    }
}