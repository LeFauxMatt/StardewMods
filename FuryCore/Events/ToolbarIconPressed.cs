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
internal class ToolbarIconPressed : SortedEventHandler<ToolbarIconPressedEventArgs>
{
    private readonly Lazy<ToolbarIcons> _toolbarIcons;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ToolbarIconPressed" /> class.
    /// </summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <param name="services">Provides access to internal and external services.</param>
    public ToolbarIconPressed(IModHelper helper, IModServices services)
    {
        this.Helper = helper;
        this._toolbarIcons = services.Lazy<ToolbarIcons>();
        this.Helper.Events.Input.ButtonPressed += this.OnButtonPressed;
    }

    private IModHelper Helper { get; }

    private ToolbarIcons ToolbarIcons
    {
        get => this._toolbarIcons.Value;
    }

    [EventPriority(EventPriority.High + 1000)]
    private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
    {
        if (this.HandlerCount == 0)
        {
            return;
        }

        if (e.Button != SButton.MouseLeft && !e.Button.IsActionButton() || !this.ToolbarIcons.Icons.Any())
        {
            return;
        }

        var (x, y) = Game1.getMousePosition(true);
        var icon = this.ToolbarIcons.Icons.FirstOrDefault(icon => icon.Component?.containsPoint(x, y) == true);
        if (icon is not null)
        {
            Game1.playSound("drumkit6");
            this.InvokeAll(new(
                icon,
                () => this.Helper.Input.Suppress(SButton.MouseLeft),
                () => this.Helper.Input.IsSuppressed(SButton.MouseLeft)));
        }
    }
}