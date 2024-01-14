namespace StardewMods.SpritePatcher.Framework.Models;

using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using StardewMods.SpritePatcher.Framework.Enums;
using StardewMods.SpritePatcher.Framework.Interfaces;

/// <inheritdoc />
internal sealed class PatchData(
    string id,
    string target,
    List<DrawMethod> drawMethods,
    PatchMode patchMode,
    Dictionary<string, TokenDefinition> tokens,
    List<ConditionalTexture> textures,
    int priority = 0) : IPatchData
{
    private static readonly Regex Regex = new(
        @"^(.+?)\{(\d+,\d+,\d+.,\d+)\}?$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    private string? baseTarget;
    private Rectangle? sourceRect;

    /// <inheritdoc />
    public string BaseTarget => this.baseTarget ??= this.GetBaseTarget();

    /// <inheritdoc />
    public Rectangle? SourceRect
    {
        get
        {
            this.baseTarget ??= this.GetBaseTarget();
            return this.sourceRect;
        }
    }

    /// <inheritdoc />
    public string Id { get; set; } = id;

    /// <inheritdoc />
    public string Target { get; set; } = target;

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

    private string GetBaseTarget()
    {
        var match = PatchData.Regex.Match(this.Target);
        if (!match.Success)
        {
            return this.Target;
        }

        var parts = match.Groups[2].Value.Split(',', StringSplitOptions.TrimEntries);
        if (parts.Length == 4
            && int.TryParse(parts[0], out var x)
            && int.TryParse(parts[1], out var y)
            && int.TryParse(parts[2], out var width)
            && int.TryParse(parts[3], out var height))
        {
            this.sourceRect = new Rectangle(x, y, width, height);
        }

        return !match.Success ? this.Target : match.Groups[1].Value;
    }
}