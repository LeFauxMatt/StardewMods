namespace StardewMods.TeaTime.Framework;

using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;

/// <summary>Responsible for handling assets provided by this mod.</summary>
internal sealed class AssetHandler
{
    /// <summary>Game path to data assets used by TeaTime.</summary>
    private const string DataPath = "furyx639.TeaTime/Data";

    private readonly IGameContentHelper gameContent;

    /// <summary>
    /// Initializes a new instance of the <see cref="AssetHandler"/> class.
    /// </summary>
    /// <param name="events">Dependency used for monitoring and logging.</param>
    /// <param name="gameContent">Dependency used for loading game assets.</param>
    public AssetHandler(IModEvents events, IGameContentHelper gameContent)
    {
        this.gameContent = gameContent;
        events.Content.AssetRequested += AssetHandler.OnAssetRequested;
    }

    /// <summary>Gets TeaTime data.</summary>
    public Dictionary<string, TeaModel> TeaData =>
        this.gameContent.Load<Dictionary<string, TeaModel>>(AssetHandler.DataPath);

    /// <summary>Loads a game texture.</summary>
    /// <param name="path">The path to the game texture.</param>
    /// <returns>Returns the game texture to load.</returns>
    public Texture2D GetTexture(string path) => this.gameContent.Load<Texture2D>(path);

    private static void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
    {
        if (e.Name.IsEquivalentTo(AssetHandler.DataPath))
        {
            e.LoadFrom(() => new Dictionary<string, TeaModel>(), AssetLoadPriority.Exclusive);
        }
    }
}
