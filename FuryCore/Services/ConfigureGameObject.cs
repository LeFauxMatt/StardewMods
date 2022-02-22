namespace StardewMods.FuryCore.Services;

using System;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.FuryCore.Attributes;
using StardewMods.FuryCore.Events;
using StardewMods.FuryCore.Interfaces;
using StardewMods.FuryCore.Interfaces.CustomEvents;
using StardewMods.FuryCore.Interfaces.GameObjects;
using StardewMods.FuryCore.Models;

/// <inheritdoc cref="StardewMods.FuryCore.Interfaces.IConfigureGameObject" />
[FuryCoreService(true)]
internal class ConfigureGameObject : IConfigureGameObject, IModService
{
    private readonly ConfiguringGameObject _configuringGameObject;
    private readonly PerScreen<IGameObject> _currentObject = new();
    private readonly Lazy<ModConfigMenu> _modConfigMenu;
    private readonly ResettingConfig _resettingConfig;
    private readonly SavingConfig _savingConfig;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ConfigureGameObject" /> class.
    /// </summary>
    /// <param name="config">The data for player configured mod options.</param>
    /// <param name="helper">SMAPI helper to read/save config data and for events.</param>
    /// <param name="manifest">The mod manifest to subscribe to GMCM with.</param>
    /// <param name="services">Provides access to internal and external services.</param>
    public ConfigureGameObject(ConfigData config, IModHelper helper, IManifest manifest, IModServices services)
    {
        this._modConfigMenu = services.Lazy<ModConfigMenu>();
        this._configuringGameObject = new(config, helper, manifest, services);
        this._resettingConfig = new(services);
        this._savingConfig = new(services);
        helper.Events.Display.MenuChanged += this.OnMenuChanged;
    }

    /// <inheritdoc />
    public event EventHandler<IConfiguringGameObjectEventArgs> ConfiguringGameObject
    {
        add => this._configuringGameObject.Add(value);
        remove => this._configuringGameObject.Remove(value);
    }

    /// <inheritdoc />
    public event EventHandler<IResettingConfigEventArgs> ResettingConfig
    {
        add => this._resettingConfig.Add(value);
        remove => this._resettingConfig.Remove(value);
    }

    /// <inheritdoc />
    public event EventHandler<ISavingConfigEventArgs> SavingConfig
    {
        add => this._savingConfig.Add(value);
        remove => this._savingConfig.Remove(value);
    }

    /// <inheritdoc />
    public IGameObject CurrentObject
    {
        get => this._currentObject.Value;
        private set => this._currentObject.Value = value;
    }

    private ModConfigMenu ModConfigMenu
    {
        get => this._modConfigMenu.Value;
    }

    /// <summary>
    ///     Registers a new Config Menu for the current object.
    /// </summary>
    /// <param name="gameObject">The game object to configure.</param>
    public void Register(IGameObject gameObject)
    {
        if (this.ModConfigMenu.Register(this._resettingConfig.Reset, this._savingConfig.Save))
        {
            this.CurrentObject = gameObject;
        }
    }

    /// <summary>
    ///     Shows the Config Menu.
    /// </summary>
    public void Show()
    {
        if (this.CurrentObject is not null)
        {
            this.ModConfigMenu.Show();
        }
    }

    private void OnMenuChanged(object sender, MenuChangedEventArgs e)
    {
        if (this.CurrentObject is not null && e.OldMenu?.GetType().Name == "SpecificModConfigMenu")
        {
            this.CurrentObject = null;
            this.ModConfigMenu.Unregister();
        }
    }
}