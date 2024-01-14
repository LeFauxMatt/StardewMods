namespace StardewMods.Common.Services;

using StardewMods.Common.Interfaces;
using StardewMods.Common.Models.Events;

/// <summary>
/// The ConfigManager class is responsible for managing the configuration for a mod.
/// It provides methods to initialize, reset, save, and retrieve the configuration.
/// </summary>
/// <typeparam name="TConfig">The type of the configuration.</typeparam>
internal class ConfigManager<TConfig>
    where TConfig : class, new()
{
    private readonly IEventPublisher eventPublisher;
    private readonly IModHelper modHelper;

    private bool initialized;

    /// <summary>Initializes a new instance of the <see cref="ConfigManager{TConfig}" /> class.</summary>
    /// <param name="eventPublisher">Dependency used for publishing events.</param>
    /// <param name="modHelper">Dependency for events, input, and content.</param>
    public ConfigManager(IEventPublisher eventPublisher, IModHelper modHelper)
    {
        this.eventPublisher = eventPublisher;
        this.modHelper = modHelper;
        this.Config = this.modHelper.ReadConfig<TConfig>();
    }

    /// <summary>Gets the backing config.</summary>
    protected TConfig Config { get; private set; }

    /// <summary>Perform initialization routine.</summary>
    public void Init()
    {
        if (this.initialized)
        {
            return;
        }

        this.initialized = true;
        this.eventPublisher.Publish(new ConfigChangedEventArgs<TConfig>(this.Config));
    }

    /// <summary>Returns a new instance of IModConfig.</summary>
    /// <returns>The new instance of IModConfig.</returns>
    public virtual TConfig GetDefault() => new();

    /// <summary>Returns a new instance of IModConfig by reading the DefaultConfig from the mod helper.</summary>
    /// <returns>The new instance of IModConfig.</returns>
    public virtual TConfig GetNew() => this.modHelper.ReadConfig<TConfig>();

    /// <summary>Resets the configuration by reassigning to <see cref="TConfig" />.</summary>
    public void Reset()
    {
        this.Config = this.GetNew();
        this.eventPublisher.Publish(new ConfigChangedEventArgs<TConfig>(this.Config));
    }

    /// <summary>Saves the provided config.</summary>
    /// <param name="config">The config object to be saved.</param>
    public void Save(TConfig config)
    {
        this.modHelper.WriteConfig(config);
        this.Config = config;
        this.eventPublisher.Publish(new ConfigChangedEventArgs<TConfig>(this.Config));
    }
}