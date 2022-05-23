#nullable disable

namespace StardewMods.FuryCore.Events;

using System;
using System.Linq;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewMods.FuryCore.Interfaces;
using StardewMods.FuryCore.Models.CustomEvents;
using StardewMods.FuryCore.Services;
using StardewValley;

/// <inheritdoc />
internal class HudComponentPressed : SortedEventHandler<ClickableComponentPressedEventArgs>
{
    private readonly Lazy<HudComponents> _toolbarIcons;

    /// <summary>
    ///     Initializes a new instance of the <see cref="HudComponentPressed" /> class.
    /// </summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <param name="services">Provides access to internal and external services.</param>
    public HudComponentPressed(IModHelper helper, IModServices services)
    {
        this.Helper = helper;
        this._toolbarIcons = services.Lazy<HudComponents>();
        this.Helper.Events.Input.ButtonPressed += this.OnButtonPressed;
    }

    private IModHelper Helper { get; }

    private HudComponents HudComponents
    {
        get => this._toolbarIcons.Value;
    }

    [EventPriority(EventPriority.High)]
    private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
    {
        if (!Game1.displayHUD || Game1.activeClickableMenu is not null || this.HandlerCount == 0 || !this.HudComponents.Components.Any())
        {
            return;
        }

        if (e.Button is not SButton.MouseLeft or SButton.MouseRight)
        {
            return;
        }

        var (x, y) = Game1.getMousePosition(true);
        var icon = this.HudComponents.Components.FirstOrDefault(icon => icon.Component?.containsPoint(x, y) == true);
        if (icon is not null)
        {
            Game1.playSound("drumkit6");
            this.InvokeAll(new(
                e.Button,
                icon,
                () => this.Helper.Input.Suppress(e.Button),
                () => this.Helper.Input.IsSuppressed(e.Button)));
        }
    }
}