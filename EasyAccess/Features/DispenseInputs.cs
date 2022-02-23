namespace StardewMods.EasyAccess.Features;

using System;
using System.Collections.Generic;
using System.Linq;
using Common.Helpers;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.EasyAccess.Enums;
using StardewMods.EasyAccess.Interfaces.Config;
using StardewMods.EasyAccess.Interfaces.ManagedObjects;
using StardewMods.FuryCore.Enums;
using StardewMods.FuryCore.Interfaces;
using StardewMods.FuryCore.Interfaces.ClickableComponents;
using StardewMods.FuryCore.Models.ClickableComponents;
using StardewMods.FuryCore.Models.CustomEvents;
using StardewValley;

/// <inheritdoc />
internal class DispenseInputs : Feature
{
    private readonly PerScreen<IClickableComponent> _dispenseButton = new();
    private readonly Lazy<IHudComponents> _toolbarIcons;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DispenseInputs" /> class.
    /// </summary>
    /// <param name="config">The <see cref="IConfigData" /> for options set by the player.</param>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <param name="services">Provides access to internal and external services.</param>
    public DispenseInputs(IConfigModel config, IModHelper helper, IModServices services)
        : base(config, helper, services)
    {
        this._toolbarIcons = services.Lazy<IHudComponents>();
    }

    /// <summary>
    ///     Gets a value indicating which producers are eligible for dispensing into.
    /// </summary>
    public IEnumerable<IManagedProducer> EligibleProducers
    {
        get
        {
            IList<IManagedProducer> eligibleProducers = new List<IManagedProducer>();
            foreach (var ((location, (x, y)), producer) in this.ManagedObjects.Producers)
            {
                // Disabled in config or by location name
                if (producer.DispenseInputs == FeatureOptionRange.Disabled)
                {
                    continue;
                }

                switch (producer.DispenseInputs)
                {
                    // Disabled if not current location for location chest
                    case FeatureOptionRange.Location when !location.Equals(Game1.currentLocation):
                        continue;
                    case FeatureOptionRange.World:
                    case FeatureOptionRange.Location when producer.DispenseInputDistance == -1:
                    case FeatureOptionRange.Location when Utility.withinRadiusOfPlayer((int)x * 64, (int)y * 64, producer.DispenseInputDistance, Game1.player):
                        eligibleProducers.Add(producer);
                        continue;
                    case FeatureOptionRange.Default:
                    case FeatureOptionRange.Disabled:
                    default:
                        continue;
                }
            }

            return eligibleProducers.OrderByDescending(eligibleProducer => eligibleProducer.DispenseInputPriority);
        }
    }

    private IClickableComponent DispenseButton
    {
        get => this._dispenseButton.Value ??= new CustomClickableComponent(
            new(
                new(0, 0, 32, 32),
                this.Helper.Content.Load<Texture2D>($"{EasyAccess.ModUniqueId}/Icons", ContentSource.GameContent),
                new(16, 0, 16, 16),
                2f)
            {
                name = "Dispense Inputs",
                hoverText = I18n.Button_DispenseInputs_Name(),
            },
            ComponentArea.Right);
    }

    private IHudComponents HudComponents
    {
        get => this._toolbarIcons.Value;
    }

    /// <inheritdoc />
    protected override void Activate()
    {
        this.HudComponents.AddToolbarIcon(this.DispenseButton);
        this.HudComponents.HudComponentPressed += this.OnHudComponentPressed;
        this.Helper.Events.Input.ButtonsChanged += this.OnButtonsChanged;
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        this.HudComponents.RemoveToolbarIcon(this.DispenseButton);
        this.HudComponents.HudComponentPressed -= this.OnHudComponentPressed;
        this.Helper.Events.Input.ButtonsChanged -= this.OnButtonsChanged;
    }

    private bool DispenseItems()
    {
        Log.Trace("Dispensing items into producers");
        var dispensedAny = false;
        foreach (var producer in this.EligibleProducers)
        {
            for (var index = 0; index < Game1.player.MaxItems; index++)
            {
                var item = Game1.player.Items[index];
                if (item is not null && producer.TrySetInput(item))
                {
                    Log.Trace($"Dispensed {item.Name} into producer {producer.QualifiedItemId}.");
                    dispensedAny = true;
                    break;
                }
            }
        }

        if (dispensedAny)
        {
            return true;
        }

        Game1.showRedMessage(I18n.Alert_DispenseInputs_NoEligible());
        return false;
    }

    private void OnButtonsChanged(object sender, ButtonsChangedEventArgs e)
    {
        if (!this.Config.ControlScheme.DispenseItems.JustPressed())
        {
            return;
        }

        if (Context.IsPlayerFree && this.DispenseItems())
        {
            this.Helper.Input.SuppressActiveKeybinds(this.Config.ControlScheme.DispenseItems);
        }
    }

    private void OnHudComponentPressed(object sender, ClickableComponentPressedEventArgs e)
    {
        if (ReferenceEquals(this.DispenseButton, e.Component))
        {
            this.DispenseItems();
            e.SuppressInput();
        }
    }
}