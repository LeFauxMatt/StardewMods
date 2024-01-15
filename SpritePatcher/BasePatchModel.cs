namespace StardewMods.SpritePatcher;

using Microsoft.Xna.Framework;
using StardewMods.SpritePatcher.Framework.Enums;
using StardewMods.SpritePatcher.Framework.Interfaces;

/// <inheritdoc />
public abstract class BasePatchModel : IPatchModel
{
    private readonly IMonitor monitor;

    /// <summary>Initializes a new instance of the <see cref="BasePatchModel" /> class.</summary>
    /// <param name="monitor">Dependency used for monitoring and logging.</param>
    /// <param name="modId">The unique id of the mod.</param>
    /// <param name="contentPack">The content pack of the mod.</param>
    /// <param name="target">The target sprite sheet of the patch.</param>
    /// <param name="sourceArea">The area of the patch.</param>
    /// <param name="drawMethods">The draw method of the patch.</param>
    /// <param name="patchMode">The patch mode.</param>
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
        this.Helper = new PatchHelper(this);
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
    public IRawTextureData? Texture { get; protected set; }

    /// <inheritdoc />
    public Rectangle? Area { get; protected set; }

    /// <inheritdoc />
    public Color? Tint { get; protected set; }

    /// <summary>Gets a helper that provides useful methods for performing common operations.</summary>
    protected PatchHelper Helper { get; }

    /// <inheritdoc />
    public abstract bool Run(IHaveModData entity);

    /// <summary>The Helper class provides useful methods for performing common operations.</summary>
    protected class PatchHelper(BasePatchModel patchModel)
    {
        /// <summary>Logs a message with the specified information.</summary>
        /// <param name="message">The message to be logged.</param>
        protected void Log(string message) => patchModel.monitor.Log($"{patchModel.ModId}: {message}", LogLevel.Info);

        /// <summary>Sets the Area representing the icon to the specified index within a texture.</summary>
        /// <param name="index">The index of the icon within the texture.</param>
        /// <param name="width">The width of each icon within the texture. Default value is 16.</param>
        /// <param name="height">The height of each icon within the texture. Default value is 16.</param>
        public void SetIcon(int index, int width = 16, int height = 16)
        {
            if (patchModel.Texture is not null && index != -1)
            {
                patchModel.Area = new Rectangle(
                    width * (index % (patchModel.Texture.Width / width)),
                    height * (index / (patchModel.Texture.Width / width)),
                    width,
                    height);
            }
        }

        /// <summary>Sets the texture of an object using the specified path.</summary>
        /// <param name="path">The path of the texture.</param>
        public void SetTexture(string path) =>
            patchModel.Texture = patchModel.ContentPack.ModContent.Load<IRawTextureData>(path);

        /// <summary>Returns the index of the first occurrence of the specified value in the given array of strings.</summary>
        /// <param name="input">The input string to split.</param>
        /// <param name="value">The value to locate.</param>
        /// <param name="separator">The character used to separate the substrings. The default value is ','.</param>
        /// <returns>The index of the first occurrence of the specified value in the array, if found; otherwise, -1.</returns>
        public int GetIndexFromString(string input, string value, char separator = ',')
        {
            var values = input.Split(separator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            return Array.FindIndex(values, v => v.Equals(value, StringComparison.OrdinalIgnoreCase));
        }
    }
}