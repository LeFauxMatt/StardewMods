namespace StardewMods.SpritePatcher.Framework.Models;

using StardewMods.SpritePatcher.Framework.Enums;
using StardewMods.SpritePatcher.Framework.Interfaces;

/// <inheritdoc />
internal sealed class PatchData(
    string id,
    string target,
    string path,
    List<DrawMethod> drawMethods,
    PatchMode patchMode,
    Dictionary<string, TokenDefinition> tokens,
    List<ConditionalTexture> textures) : IPatchData
{
    /// <inheritdoc />
    public string Id { get; set; } = id;

    /// <inheritdoc />
    public string Target { get; set; } = target;

    /// <inheritdoc />
    public string Path { get; set; } = path;

    /// <inheritdoc />
    public List<DrawMethod> DrawMethods { get; set; } = drawMethods;

    /// <inheritdoc />
    public PatchMode PatchMode { get; set; } = patchMode;

    /// <inheritdoc />
    public Dictionary<string, TokenDefinition> Tokens { get; set; } = tokens;

    /// <inheritdoc />
    public List<ConditionalTexture> Textures { get; set; } = textures;
}