namespace StardewMods.ToolbarIcons.Framework.Services;

using System.Globalization;
using StardewModdingAPI.Events;
using StardewMods.ToolbarIcons.Framework.Interfaces;
using StardewMods.ToolbarIcons.Framework.Services.Integrations;

/// <summary>Handles integrations with other mods.</summary>
internal sealed class IntegrationsManager
{
    private readonly IEnumerable<ICustomIntegration> customIntegrations;
    private readonly EventsManager eventsManager;
    private readonly IGameContentHelper gameContentHelper;
    private readonly SimpleIntegration simpleIntegration;

    private bool isLoaded;

    /// <summary>Initializes a new instance of the <see cref="IntegrationsManager" /> class.</summary>
    /// <param name="customIntegrations">Integrations directly supported by the mod.</param>
    /// <param name="eventsManager">Dependency used for custom events.</param>
    /// <param name="gameContentHelper">Dependency used for loading game assets.</param>
    /// <param name="modEvents">Dependency used for managing access to events.</param>
    /// <param name="simpleIntegration">Dependency used for adding a simple mod integration.</param>
    public IntegrationsManager(
        IEnumerable<ICustomIntegration> customIntegrations,
        EventsManager eventsManager,
        IGameContentHelper gameContentHelper,
        IModEvents modEvents,
        SimpleIntegration simpleIntegration)
    {
        // Init
        this.customIntegrations = customIntegrations;
        this.eventsManager = eventsManager;
        this.gameContentHelper = gameContentHelper;
        this.simpleIntegration = simpleIntegration;

        // Events
        modEvents.GameLoop.SaveLoaded += this.OnSaveLoaded;
    }

    private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
    {
        if (this.isLoaded)
        {
            return;
        }

        foreach (var (key, data) in this.gameContentHelper.Load<Dictionary<string, string>>(AssetHandler.ToolbarPath))
        {
            var info = data.Split('/');
            var modId = key.Split('/')[0];
            var index = int.Parse(info[2], CultureInfo.InvariantCulture);
            switch (info[3])
            {
                case "menu":
                    this.simpleIntegration.AddMenu(modId, index, info[0], info[4], info[1]);
                    break;
                case "method":
                    this.simpleIntegration.AddMethod(modId, index, info[0], info[4], info[1]);
                    break;
                case "keybind":
                    this.simpleIntegration.AddKeybind(modId, index, info[0], info[4], info[1]);
                    break;
            }
        }

        foreach (var integration in this.customIntegrations)
        {
            integration.AddIntegration();
        }

        this.isLoaded = true;
        this.eventsManager.InvokeToolbarIconsLoaded();
    }
}