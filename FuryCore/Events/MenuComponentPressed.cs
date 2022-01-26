namespace FuryCore.Events;

using System;
using System.Linq;
using FuryCore.Models;
using FuryCore.Services;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

/// <inheritdoc />
internal class MenuComponentPressed : SortedEventHandler<MenuComponentPressedEventArgs>
{
    private readonly Lazy<MenuComponents> _components;

    /// <summary>
    ///     Initializes a new instance of the <see cref="MenuComponentPressed" /> class.
    /// </summary>
    /// <param name="helper"></param>
    /// <param name="services"></param>
    public MenuComponentPressed(IModHelper helper, ServiceCollection services)
    {
        this.Helper = helper;
        this._components = services.Lazy<MenuComponents>();
        this.Helper.Events.Input.ButtonPressed += this.OnButtonPressed;
    }

    private IModHelper Helper { get; }

    private MenuComponents Components
    {
        get => this._components.Value;
    }

    [EventPriority(EventPriority.High + 1000)]
    private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
    {
        if ((e.Button != SButton.MouseLeft && !e.Button.IsActionButton()) || !this.Components.Components.Any())
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