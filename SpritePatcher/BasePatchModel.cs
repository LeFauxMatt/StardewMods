namespace StardewMods.SpritePatcher;

using Microsoft.Xna.Framework;
using StardewMods.SpritePatcher.Framework.Enums;
using StardewMods.SpritePatcher.Framework.Interfaces;

/// <inheritdoc />
public abstract class BasePatchModel : IPatchModel
{
    private readonly IMonitor monitor;

    protected BasePatchModel(
        IMonitor monitor,
        string modId,
        IContentPack contentPack,
        string target,
        Rectangle? sourceArea,
        List<DrawMethod> drawMethods,
        PatchMode patchMode)
    {
        this.monitor = monitor;
        this.ModId = modId;
        this.ContentPack = contentPack;
        this.Target = target;
        this.SourceArea = sourceArea;
        this.DrawMethods = drawMethods;
        this.PatchMode = patchMode;
    }

    /// <inheritdoc />
    public string ModId { get; }

    /// <inheritdoc />
    public IContentPack ContentPack { get; }

    /// <inheritdoc />
    public string Target { get; }

    /// <inheritdoc />
    public Rectangle? SourceArea { get; }

    /// <inheritdoc />
    public List<DrawMethod> DrawMethods { get; }

    /// <inheritdoc />
    public PatchMode PatchMode { get; }

    /// <inheritdoc />
    public string Path { get; protected set; } = string.Empty;

    /// <inheritdoc />
    public Rectangle? Area { get; protected set; }

    /// <inheritdoc />
    public Color? Tint { get; protected set; }

    /// <inheritdoc />
    public abstract bool Run(IHaveModData entity);

    protected void Log(string message) => this.monitor.Log($"{this.ModId}: {message}", LogLevel.Info);
}