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
    private readonly Lazy<CustomMenuComponents> _customMenuComponents;

    /// <summary>
    /// Initializes a new instance of the <see cref="MenuComponentPressed"/> class.
    /// </summary>
    /// <param name="helper"></param>
    /// <param name="services"></param>
    public MenuComponentPressed(IModHelper helper, ServiceCollection services)
    {
        this.Helper = helper;
        this._customMenuComponents = services.Lazy<CustomMenuComponents>();
        this.Helper.Events.Input.ButtonPressed += this.OnButtonPressed;
    }

    private IModHelper Helper { get; }

    private CustomMenuComponents CustomMenuComponents
    {
        get => this._customMenuComponents.Value;
    }

    private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
    {
        if ((e.Button != SButton.MouseLeft && !e.Button.IsActionButton()) || !(this.CustomMenuComponents.SideComponents.Any() || this.CustomMenuComponents.BehindComponents.Any()))
        {
            return;
        }

        var (x, y) = Game1.getMousePosition(true);
        var component = this.CustomMenuComponents.SideComponents.Concat(this.CustomMenuComponents.BehindComponents).FirstOrDefault(component => component.Component.containsPoint(x, y));
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