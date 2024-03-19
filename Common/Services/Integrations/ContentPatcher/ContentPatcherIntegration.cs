namespace StardewMods.Common.Services.Integrations.ContentPatcher;

using StardewModdingAPI.Events;
using StardewMods.Common.Interfaces;

/// <inheritdoc />
internal sealed class ContentPatcherIntegration : ModIntegration<IContentPatcherApi>
{
    private const string ModUniqueId = "Pathoschild.ContentPatcher";
    private const string ModVersion = "1.28.0";

    private readonly IEventManager eventManager;

    private int countDown = 10;

    /// <summary>Initializes a new instance of the <see cref="ContentPatcherIntegration" /> class.</summary>
    /// <param name="eventManager">Dependency used for managing events.</param>
    /// <param name="modRegistry">Dependency used for fetching metadata about loaded mods.</param>
    public ContentPatcherIntegration(IEventManager eventManager, IModRegistry modRegistry)
        : base(modRegistry, ContentPatcherIntegration.ModUniqueId, ContentPatcherIntegration.ModVersion)
    {
        this.eventManager = eventManager;

        if (this.IsLoaded)
        {
            this.eventManager.Subscribe<UpdateTickedEventArgs>(this.OnUpdateTicked);
        }
    }

    private void OnUpdateTicked(UpdateTickedEventArgs e)
    {
        if (--this.countDown == 0)
        {
            this.eventManager.Unsubscribe<UpdateTickedEventArgs>(this.OnUpdateTicked);
        }

        if (!this.IsLoaded || !this.Api.IsConditionsApiReady)
        {
            return;
        }

        this.eventManager.Unsubscribe<UpdateTickedEventArgs>(this.OnUpdateTicked);
        this.eventManager.Publish(new ConditionsApiReadyEventArgs());
    }
}