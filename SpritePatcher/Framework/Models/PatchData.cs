namespace StardewMods.SpritePatcher.Framework.Models;

using StardewMods.SpritePatcher.Framework.Enums;
using StardewMods.SpritePatcher.Framework.Interfaces;

/// <inheritdoc />
internal sealed class PatchData(
    string? logName,
    string target,
    string path,
    List<DrawMethod> drawMethods,
    PatchMode patchMode,
    Dictionary<string, TokenDefinition> tokens,
    List<ConditionalTexture> textures,
    int priority = 0) : IPatchData
{
    /// <inheritdoc />
    public string? LogName { get; set; } = logName;

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

    /// <inheritdoc />
    public int Priority { get; set; } = priority;
}