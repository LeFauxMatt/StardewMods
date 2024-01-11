namespace StardewMods.ItemIconOverlays.Framework.Services;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Services;
using StardewMods.Common.Services.Integrations.FuryCore;
using StardewValley.Menus;

/// <summary>Manages overlays for items.</summary>
internal sealed class OverlayManager : BaseService
{
    private readonly IGameContentHelper gameContentHelper;
    private readonly IconManager iconManager;
    private readonly ItemPropertyManager itemPropertyManager;
    private readonly IReflectionHelper reflectionHelper;

    /// <summary>Initializes a new instance of the <see cref="OverlayManager" /> class.</summary>
    /// <param name="eventSubscriber">Dependency used for subscribing to events.</param>
    /// <param name="gameContentHelper">Dependency used for loading game assets.</param>
    /// <param name="iconManager">Dependency used for managing icons.</param>
    /// <param name="itemPropertyManager">Dependency used for getting item properties.</param>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="reflectionHelper">Dependency used for reflecting into external code.</param>
    public OverlayManager(
        IEventSubscriber eventSubscriber,
        IGameContentHelper gameContentHelper,
        IconManager iconManager,
        ItemPropertyManager itemPropertyManager,
        ILog log,
        IManifest manifest,
        IReflectionHelper reflectionHelper)
        : base(log, manifest)
    {
        this.gameContentHelper = gameContentHelper;
        this.iconManager = iconManager;
        this.itemPropertyManager = itemPropertyManager;
        this.reflectionHelper = reflectionHelper;
        eventSubscriber.Subscribe<RenderedActiveMenuEventArgs>(this.OnRenderedActiveMenu);
        eventSubscriber.Subscribe<RenderedHudEventArgs>(this.OnRenderedHud);
    }

    private void OnRenderedActiveMenu(RenderedActiveMenuEventArgs e)
    {
        switch (Game1.activeClickableMenu)
        {
            case ItemGrabMenu itemGrabMenu:
                this.DrawOverlay(e.SpriteBatch, itemGrabMenu.inventory);
                this.DrawOverlay(e.SpriteBatch, itemGrabMenu.ItemsToGrabMenu);
                return;
        }
    }

    private void OnRenderedHud(RenderedHudEventArgs e)
    {
        if (!Game1.displayHUD || !Context.IsPlayerFree || Game1.activeClickableMenu is not null)
        {
            return;
        }

        var toolbar = Game1.onScreenMenus.OfType<Toolbar>().FirstOrDefault();
        if (toolbar is not null)
        {
            this.DrawOverlay(e.SpriteBatch, toolbar);
        }
    }

    private void DrawOverlay(SpriteBatch spriteBatch, Toolbar toolbar)
    {
        var buttons = this.reflectionHelper.GetField<List<ClickableComponent>>(toolbar, "buttons").GetValue();
        for (var index = 0; index < 12; ++index)
        {
            var item = Game1.player.Items.ElementAtOrDefault(index);
            if (item is not null)
            {
                this.DrawOverlay(spriteBatch, item, buttons[index]);
            }
        }
    }

    private void DrawOverlay(SpriteBatch spriteBatch, InventoryMenu inventoryMenu)
    {
        foreach (var slot in inventoryMenu.inventory)
        {
            if (!int.TryParse(slot.name, out var index))
            {
                continue;
            }

            var item = inventoryMenu.actualInventory.ElementAtOrDefault(index);
            if (item is not null)
            {
                this.DrawOverlay(spriteBatch, item, slot);
            }
        }
    }

    private void DrawOverlay(SpriteBatch spriteBatch, Item item, ClickableComponent component)
    {
        var iconData = this.iconManager.GetData(item);
        foreach (var icon in iconData)
        {
            if (!this.itemPropertyManager.TryGetValue(item, icon.Path, out var value) || !value.CompareTo(icon.Value))
            {
                continue;
            }

            var texture = this.gameContentHelper.Load<Texture2D>(icon.Texture);
            var pos = new Vector2(component.bounds.X, component.bounds.Y);
            spriteBatch.Draw(texture, pos, icon.SourceRect, Color.White, 0f, Vector2.Zero, 4, SpriteEffects.None, 1f);
        }
    }
}