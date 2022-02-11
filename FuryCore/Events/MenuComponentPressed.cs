﻿namespace StardewMods.FuryCore.Events;

using System;
using System.Linq;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewMods.FuryCore.Interfaces;
using StardewMods.FuryCore.Models.CustomEvents;
using StardewMods.FuryCore.Services;
using StardewValley;

/// <inheritdoc />
internal class MenuComponentPressed : SortedEventHandler<MenuComponentPressedEventArgs>
{
    private readonly Lazy<MenuComponents> _components;

    /// <summary>
    ///     Initializes a new instance of the <see cref="MenuComponentPressed" /> class.
    /// </summary>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <param name="services">Provides access to internal and external services.</param>
    public MenuComponentPressed(IModHelper helper, IModServices services)
    {
        this.Helper = helper;
        this._components = services.Lazy<MenuComponents>();
        this.Helper.Events.Input.ButtonPressed += this.OnButtonPressed;
    }

    private MenuComponents Components
    {
        get => this._components.Value;
    }

    private IModHelper Helper { get; }

    [EventPriority(EventPriority.High + 1000)]
    private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
    {
        if (this.HandlerCount == 0)
        {
            return;
        }

        if (e.Button != SButton.MouseLeft && !e.Button.IsActionButton() || !this.Components.Components.Any())
        {
            return;
        }

        var (x, y) = Game1.getMousePosition(true);
        var component = this.Components.Components.FirstOrDefault(component => component.Component?.containsPoint(x, y) == true);
        if (component is not null)
        {
            Game1.playSound("drumkit6");
            this.InvokeAll(new(
                component,
                () => this.Helper.Input.Suppress(SButton.MouseLeft),
                () => this.Helper.Input.IsSuppressed(SButton.MouseLeft)));
        }
    }
}