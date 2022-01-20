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
    private readonly Lazy<MenuComponents> _customMenuComponents;

    /// <summary>
    /// Initializes a new instance of the <see cref="MenuComponentPressed"/> class.
    /// </summary>
    /// <param name="helper"></param>
    /// <param name="services"></param>
    public MenuComponentPressed(IModHelper helper, ServiceCollection services)
    {
        this.Helper = helper;
        this._customMenuComponents = services.Lazy<MenuComponents>();
        this.Helper.Events.Input.ButtonPressed += this.OnButtonPressed;
    }

    private IModHelper Helper { get; }

    private MenuComponents MenuComponents
    {
        get => this._customMenuComponents.Value;
    }

    private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
    {
        if ((e.Button != SButton.MouseLeft && !e.Button.IsActionButton()) || !this.MenuComponents.Components.Any())
        {
            return;
        }

        var (x, y) = Game1.getMousePosition(true);
        var component = this.MenuComponents.Components.FirstOrDefault(component => component.Component.containsPoint(x, y));
        if (component is not null)
        {
            Game1.playSound("drumkit6");
            var eventArgs = new MenuComponentPressedEventArgs(
                component,
                () => this.Helper.Input.Suppress(SButton.MouseLeft),
                () => this.Helper.Input.IsSuppressed(SButton.MouseLeft));
            this.InvokeAll(eventArgs);
        }
    }
}