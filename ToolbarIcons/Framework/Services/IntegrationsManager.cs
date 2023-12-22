namespace StardewMods.ToolbarIcons.Framework.Services;

using System.Globalization;
using StardewModdingAPI.Events;
using StardewMods.ToolbarIcons.Framework.Integrations;

/// <summary>Handles integrations with other mods.</summary>
internal sealed class IntegrationsManager
{
    private readonly EventsManager customEvents;
    private readonly IGameContentHelper gameContent;
    private readonly SimpleIntegration simple;

    private bool isLoaded;

    /// <summary>Initializes a new instance of the <see cref="IntegrationsManager" /> class.</summary>
    /// <param name="customEvents">Dependency used for custom events.</param>
    /// <param name="events">Dependency used for managing access to events.</param>
    /// <param name="gameContent">Dependency used for loading game assets.</param>
    /// <param name="simple">Dependency used for adding a simple mod integration.</param>
    public IntegrationsManager(EventsManager customEvents, IModEvents events, IGameContentHelper gameContent, SimpleIntegration simple)
    {
        // Init
        this.customEvents = customEvents;
        this.gameContent = gameContent;
        this.simple = simple;

        // Events
        events.GameLoop.SaveLoaded += this.OnSaveLoaded;
    }

    private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
    {
        if (this.isLoaded)
        {
            return;
        }

        foreach (var (key, data) in this.gameContent.Load<Dictionary<string, string>>(AssetHandler.ToolbarPath))
        {
            var info = data.Split('/');
            var modId = key.Split('/')[0];
            var index = int.Parse(info[2], CultureInfo.InvariantCulture);
            switch (info[3])
            {
                case "menu":
                    this.simple.AddMenu(modId, index, info[0], info[4], info[1]);
                    break;
                case "method":
                    this.simple.AddMethod(modId, index, info[0], info[4], info[1]);
                    break;
                case "keybind":
                    this.simple.AddKeybind(modId, index, info[0], info[4], info[1]);
                    break;
            }
        }

        this.isLoaded = true;
        this.customEvents.InvokeToolbarIconsLoaded();
    }
}
