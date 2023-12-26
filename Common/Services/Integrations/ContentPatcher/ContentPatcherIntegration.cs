namespace StardewMods.Common.Services.Integrations.ContentPatcher;

using StardewModdingAPI.Events;
using StardewMods.Common.Extensions;

/// <inheritdoc />
internal sealed class ContentPatcherIntegration : ModIntegration<IContentPatcherApi>
{
    private const string ModUniqueId = "Pathoschild.ContentPatcher";
    private const string ModVersion = "1.28.0";

    private readonly IModEvents modEvents;

    private EventHandler? conditionsApiReady;
    private int countDown = 10;

    /// <summary>Initializes a new instance of the <see cref="ContentPatcherIntegration" /> class.</summary>
    /// <param name="modEvents">Dependency used for managing access to events.</param>
    /// <param name="modRegistry">SMAPI's mod registry.</param>
    public ContentPatcherIntegration(IModEvents modEvents, IModRegistry modRegistry)
        : base(modRegistry, ContentPatcherIntegration.ModUniqueId, ContentPatcherIntegration.ModVersion)
    {
        this.modEvents = modEvents;

        if (this.IsLoaded)
        {
            this.modEvents.GameLoop.UpdateTicked += this.OnUpdateTicked;
        }
    }

    /// <summary>Event that is triggered when the conditions API is ready.</summary>
    /// <remarks>Subscribe to this event to perform certain actions when the conditions API is ready.</remarks>
    public event EventHandler ConditionsApiReady
    {
        add => this.conditionsApiReady += value;
        remove => this.conditionsApiReady -= value;
    }

    private void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
    {
        if (--this.countDown == 0)
        {
            this.modEvents.GameLoop.UpdateTicked -= this.OnUpdateTicked;
        }

        if (!this.IsLoaded || !this.Api.IsConditionsApiReady)
        {
            return;
        }

        this.conditionsApiReady?.InvokeAll(this);
    }
}