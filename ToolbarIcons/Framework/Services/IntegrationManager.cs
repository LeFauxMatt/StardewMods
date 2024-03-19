namespace StardewMods.ToolbarIcons.Framework.Services;

using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewMods.Common.Services.Integrations.ToolbarIcons;
using StardewMods.ToolbarIcons.Framework.Enums;
using StardewMods.ToolbarIcons.Framework.Interfaces;
using StardewMods.ToolbarIcons.Framework.Models;
using StardewMods.ToolbarIcons.Framework.Models.Events;
using StardewValley.Menus;

/// <summary>Base class for adding toolbar icons for integrated mods.</summary>
internal sealed class IntegrationManager : BaseService
{
    private readonly AssetHandler assetHandler;
    private readonly IEnumerable<ICustomIntegration> customIntegrations;
    private readonly IEventManager eventManager;
    private readonly IGameContentHelper gameContentHelper;
    private readonly Dictionary<string, Action> icons = new();
    private readonly IModRegistry modRegistry;
    private readonly MethodInfo overrideButtonReflected;
    private readonly IReflectionHelper reflectionHelper;
    private readonly ToolbarManager toolbarManager;

    private bool isLoaded;

    /// <summary>Initializes a new instance of the <see cref="IntegrationManager" /> class.</summary>
    /// <param name="assetHandler">Dependency used for handling assets.</param>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="customIntegrations">Integrations directly supported by the mod.</param>
    /// <param name="eventManager">Dependency used for managing events.</param>
    /// <param name="gameContentHelper">Dependency used for loading game assets.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="modRegistry">Dependency for fetching metadata about loaded mods.</param>
    /// <param name="reflectionHelper">Dependency used for reflecting into external code.</param>
    /// <param name="toolbarManager">API to add icons above or below the toolbar.</param>
    public IntegrationManager(
        AssetHandler assetHandler,
        ILog log,
        IEnumerable<ICustomIntegration> customIntegrations,
        IEventManager eventManager,
        IGameContentHelper gameContentHelper,
        IManifest manifest,
        IModRegistry modRegistry,
        IReflectionHelper reflectionHelper,
        ToolbarManager toolbarManager)
        : base(log, manifest)
    {
        // Init
        this.assetHandler = assetHandler;
        this.customIntegrations = customIntegrations;
        this.eventManager = eventManager;
        this.gameContentHelper = gameContentHelper;
        this.modRegistry = modRegistry;
        this.reflectionHelper = reflectionHelper;
        this.toolbarManager = toolbarManager;
        this.overrideButtonReflected = Game1.input.GetType().GetMethod("OverrideButton")
            ?? throw new MethodAccessException("Unable to access OverrideButton");

        // Events
        eventManager.Subscribe<SaveLoadedEventArgs>(this.OnSaveLoaded);
        eventManager.Subscribe<IIconPressedEventArgs>(this.OnIconPressed);
    }

    /// <summary>Adds a complex integration for vanilla.</summary>
    /// <param name="index">The index of the mod icon.</param>
    /// <param name="hoverText">The text to display.</param>
    /// <param name="action">Function which returns the action to perform.</param>
    private void AddCustomAction(int index, string hoverText, Action action) =>
        this.AddIcon(string.Empty, index, hoverText, action, this.assetHandler.IconPath);

    /// <summary>Adds a complex mod integration.</summary>
    /// <param name="modId">The id of the mod.</param>
    /// <param name="index">The index of the mod icon.</param>
    /// <param name="hoverText">The text to display.</param>
    /// <param name="getAction">Function which returns the action to perform.</param>
    private void AddCustomAction(string modId, int index, string hoverText, Func<IMod, Action?> getAction)
    {
        if (!this.TryGetMod(modId, out var mod))
        {
            return;
        }

        var action = getAction(mod);
        if (action is null)
        {
            return;
        }

        this.AddIcon(modId, index, hoverText, () => action.Invoke(), this.assetHandler.IconPath);
    }

    /// <summary>Adds a toolbar icon for an integrated mod.</summary>
    /// <param name="modId">The id of the mod.</param>
    /// <param name="index">The index of the mod icon.</param>
    /// <param name="hoverText">The text to display.</param>
    /// <param name="action">The action to perform for this icon.</param>
    /// <param name="texturePath">The texture path of the icon.</param>
    private void AddIcon(string modId, int index, string hoverText, Action action, string texturePath)
    {
        var texture = this.gameContentHelper.Load<Texture2D>(texturePath);
        var cols = texture.Width / 16;
        this.toolbarManager.AddToolbarIcon(
            $"{modId}.{hoverText}",
            texturePath,
            new Rectangle(16 * (index % cols), 16 * (index / cols), 16, 16),
            hoverText);

        this.icons.Add($"{modId}.{hoverText}", action);
    }

    /// <summary>Adds a simple mod integration for a keybind.</summary>
    /// <param name="modId">The id of the mod.</param>
    /// <param name="index">The index of the mod icon.</param>
    /// <param name="hoverText">The text to display.</param>
    /// <param name="keybinds">The method to run.</param>
    /// <param name="texturePath">The texture path of the icon.</param>
    private void AddKeybind(string modId, int index, string hoverText, string keybinds, string texturePath)
    {
        if (!this.modRegistry.IsLoaded(modId))
        {
            return;
        }

        var keys = keybinds.Trim().Split(' ');
        IList<SButton> buttons = new List<SButton>();
        foreach (var key in keys)
        {
            if (Enum.TryParse(key, out SButton button))
            {
                buttons.Add(button);
            }
        }

        this.AddIcon(
            modId,
            index,
            hoverText,
            () =>
            {
                foreach (var button in buttons)
                {
                    this.OverrideButton(button, true);
                }
            },
            texturePath);
    }

    /// <summary>Adds a simple mod integration for a parameterless menu.</summary>
    /// <param name="modId">The id of the mod.</param>
    /// <param name="index">The index of the mod icon.</param>
    /// <param name="hoverText">The text to display.</param>
    /// <param name="fullName">The full name to the menu class.</param>
    /// <param name="texturePath">The texture path of the icon.</param>
    private void AddMenu(string modId, int index, string hoverText, string fullName, string texturePath)
    {
        if (!this.TryGetMod(modId, out var mod))
        {
            return;
        }

        var action = mod.GetType().Assembly.GetType(fullName)?.GetConstructor(Array.Empty<Type>());
        if (action is null)
        {
            return;
        }

        this.AddIcon(
            modId,
            index,
            hoverText,
            () =>
            {
                var menu = action.Invoke(Array.Empty<object>());
                Game1.activeClickableMenu = (IClickableMenu)menu;
            },
            texturePath);
    }

    /// <summary>Adds a simple mod integration for a parameterless method.</summary>
    /// <param name="modId">The id of the mod.</param>
    /// <param name="index">The index of the mod icon.</param>
    /// <param name="hoverText">The text to display.</param>
    /// <param name="method">The method to run.</param>
    /// <param name="texturePath">The texture path of the icon.</param>
    private void AddMethod(string modId, int index, string hoverText, string method, string texturePath)
    {
        if (!this.TryGetMod(modId, out var mod))
        {
            return;
        }

        var action = this.reflectionHelper.GetMethod(mod, method, false);
        if (action is null)
        {
            return;
        }

        this.AddIcon(modId, index, hoverText, () => action.Invoke(), texturePath);
    }

    /// <summary>Adds a simple mod integration for a method with parameters.</summary>
    /// <param name="modId">The id of the mod.</param>
    /// <param name="index">The index of the mod icon.</param>
    /// <param name="hoverText">The text to display.</param>
    /// <param name="method">The method to run.</param>
    /// <param name="arguments">The arguments to pass to the method.</param>
    private void AddMethodWithParams(string modId, int index, string hoverText, string method, object?[] arguments)
    {
        if (!this.TryGetMod(modId, out var mod))
        {
            return;
        }

        var action = this.reflectionHelper.GetMethod(mod, method, false);
        if (action is null)
        {
            return;
        }

        this.AddIcon(modId, index, hoverText, () => action.Invoke(arguments), this.assetHandler.IconPath);
    }

    private void OnSaveLoaded(SaveLoadedEventArgs e)
    {
        if (this.isLoaded)
        {
            return;
        }

        // Load Custom Integrations
        foreach (var integration in this.customIntegrations)
        {
            switch (integration)
            {
                case IActionIntegration actionIntegration:
                    this.AddCustomAction(
                        actionIntegration.ModId,
                        actionIntegration.Index,
                        actionIntegration.HoverText,
                        actionIntegration.GetAction);

                    break;
                case IMethodIntegration methodIntegration:
                    this.AddMethodWithParams(
                        methodIntegration.ModId,
                        methodIntegration.Index,
                        methodIntegration.HoverText,
                        methodIntegration.MethodName,
                        methodIntegration.Arguments);

                    break;
                case IVanillaIntegration vanillaIntegration:
                    this.AddCustomAction(
                        vanillaIntegration.Index,
                        vanillaIntegration.HoverText,
                        vanillaIntegration.DoAction);

                    break;
            }
        }

        // Load Data Integrations
        foreach (var (_, data) in this.gameContentHelper.Load<Dictionary<string, ToolbarIconData>>(
            this.assetHandler.DataPath))
        {
            switch (data.Type)
            {
                case IntegrationType.Menu:
                    this.AddMenu(data.ModId, data.Index, data.HoverText, data.ExtraData, data.Texture);
                    break;
                case IntegrationType.Method:
                    this.AddMethod(data.ModId, data.Index, data.HoverText, data.ExtraData, data.Texture);
                    break;
                case IntegrationType.Keybind:
                    this.AddKeybind(data.ModId, data.Index, data.HoverText, data.ExtraData, data.Texture);
                    break;
            }
        }

        this.isLoaded = true;
        this.eventManager.Publish(new ToolbarIconsLoadedEventArgs());
    }

    private void OnIconPressed(IIconPressedEventArgs e)
    {
        if (this.icons.TryGetValue(e.Id, out var action))
        {
            action.Invoke();
        }
    }

    private void OverrideButton(SButton button, bool inputState) =>
        this.overrideButtonReflected.Invoke(Game1.input, [button, inputState]);

    /// <summary>Tries to get the instance of a mod based on the mod id.</summary>
    /// <param name="modId">The unique id of the mod.</param>
    /// <param name="mod">The mod instance.</param>
    /// <returns>Returns true if the mod instance could be obtained.</returns>
    private bool TryGetMod(string modId, [NotNullWhen(true)] out IMod? mod)
    {
        if (!this.modRegistry.IsLoaded(modId))
        {
            mod = null;
            return false;
        }

        var modInfo = this.modRegistry.Get(modId);
        mod = (IMod?)modInfo?.GetType().GetProperty("Mod")?.GetValue(modInfo);
        return mod is not null;
    }
}