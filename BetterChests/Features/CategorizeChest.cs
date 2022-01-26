namespace BetterChests.Features;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using BetterChests.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BetterChests.Models;
using Common.Helpers;
using FuryCore.Enums;
using FuryCore.Interfaces;
using FuryCore.Models;
using FuryCore.Services;
using FuryCore.UI;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;

/// <inheritdoc />
internal class CategorizeChest : Feature
{
    private const string AutomateChestContainerType = "Pathoschild.Stardew.Automate.Framework.Storage.ChestContainer";
    private const string AutomateModUniqueId = "Pathochild.Automate";

    private readonly PerScreen<ManagedChest> _managedChest = new();
    private readonly PerScreen<MenuComponent> _configureButton = new();
    private readonly PerScreen<ItemGrabMenu> _returnMenu = new();
    private readonly PerScreen<ItemSelectionMenu> _itemSelectionMenu = new();
    private readonly Lazy<IMenuComponents> _customMenuComponents;
    private readonly Lazy<IHarmonyHelper> _harmony;

    /// <summary>
    /// Initializes a new instance of the <see cref="CategorizeChest"/> class.
    /// </summary>
    /// <param name="config">Data for player configured mod options.</param>
    /// <param name="helper">SMAPI helper for events, input, and content.</param>
    /// <param name="services">Internal and external dependency <see cref="IService" />.</param>
    public CategorizeChest(IConfigModel config, IModHelper helper, IServiceLocator services)
        : base(config, helper, services)
    {
        CategorizeChest.Instance = this;
        this._customMenuComponents = services.Lazy<IMenuComponents>();
        this._harmony = services.Lazy<IHarmonyHelper>(
            harmony =>
            {
                if (!CategorizeChest.Instance.Helper.ModRegistry.IsLoaded(CategorizeChest.AutomateModUniqueId))
                {
                    return;
                }

                var storeMethod = ReflectionHelper.GetAssemblyByName("Automate")?
                    .GetType(CategorizeChest.AutomateChestContainerType)?
                    .GetMethod("Store", BindingFlags.Public | BindingFlags.Instance);
                if (storeMethod is not null)
                {
                    harmony.AddPatch(
                        this.Id,
                        storeMethod,
                        typeof(CategorizeChest),
                        nameof(CategorizeChest.Automate_Store_prefix));
                }
            });
    }

    private static CategorizeChest Instance { get; set; }

    private IHarmonyHelper Harmony
    {
        get => this._harmony.Value;
    }

    private ManagedChest ManagedChest
    {
        get => this._managedChest.Value;
        set => this._managedChest.Value = value;
    }

    private MenuComponent ConfigureButton
    {
        get => this._configureButton.Value ??= new(
            new(
                new(0, 0, Game1.tileSize, Game1.tileSize),
                this.Helper.Content.Load<Texture2D>("assets/configure.png"),
                Rectangle.Empty,
                Game1.pixelZoom),
            ComponentArea.Right)
        {
            Name = "Configure",
        };
    }

    private ItemSelectionMenu CurrentItemSelectionMenu
    {
        get => this._itemSelectionMenu.Value;
        set => this._itemSelectionMenu.Value = value;
    }

    private IMenuComponents MenuComponents
    {
        get => this._customMenuComponents.Value;
    }

    private ItemGrabMenu ReturnMenu
    {
        get => this._returnMenu.Value;
        set => this._returnMenu.Value = value;
    }

    /// <inheritdoc />
    protected override void Activate()
    {
        this.Harmony.ApplyPatches(this.Id);
        this.FuryEvents.ItemGrabMenuChanged += this.OnItemGrabMenuChanged;
        this.FuryEvents.MenuComponentPressed += this.OnMenuComponentPressed;
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        this.Harmony.UnapplyPatches(this.Id);
        this.FuryEvents.ItemGrabMenuChanged -= this.OnItemGrabMenuChanged;
        this.FuryEvents.MenuComponentPressed -= this.OnMenuComponentPressed;
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Naming is determined by Harmony.")]
    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Type is determined by Harmony.")]
    private static bool Automate_Store_prefix(Chest ___Chest, object stack)
    {
        var item = CategorizeChest.Instance.Helper.Reflection.GetProperty<Item>(stack, "Sample").GetValue();
        return !CategorizeChest.Instance.ManagedChests.FindChest(___Chest, out var managedChest) || managedChest.ItemMatcherByType.Matches(item);
    }

    private void OnItemGrabMenuChanged(object sender, ItemGrabMenuChangedEventArgs e)
    {
        switch (e.ItemGrabMenu)
        {
            // Enter ItemSelectionMenu
            case ItemSelectionMenu:
                return;

            // Enter an Eligible ItemGrabMenu
            case not null when this.ManagedChests.FindChest(e.Chest, out var managedChest):
                this.MenuComponents.Components.Insert(0, this.ConfigureButton);
                this.ReturnMenu = e.ItemGrabMenu;
                this.ManagedChest = managedChest;
                return;

            // Exit ItemSelectionMenu
            case null when this.ReturnMenu is not null && this.CurrentItemSelectionMenu is not null && this.ManagedChest is not null:
                // Save ItemSelectionMenu to ModData
                var filterItems = this.ManagedChest.ItemMatcherByType.StringValue;
                if (string.IsNullOrWhiteSpace(filterItems))
                {
                    this.ManagedChest.Chest.modData.Remove("FilterItems");
                }
                else
                {
                    this.ManagedChest.Chest.modData["FilterItems"] = filterItems;
                }

                this.CurrentItemSelectionMenu?.UnregisterEvents(this.Helper.Events.Input);
                this.CurrentItemSelectionMenu = null;
                Game1.activeClickableMenu = this.ReturnMenu;
                return;

            default:
                this.ReturnMenu = e.ItemGrabMenu;
                return;
        }
    }

    private void OnMenuComponentPressed(object sender, MenuComponentPressedEventArgs e)
    {
        if (this.ManagedChest is null || !ReferenceEquals(this.ConfigureButton, e.Component))
        {
            return;
        }

        this.CurrentItemSelectionMenu?.UnregisterEvents(this.Helper.Events.Input);
        this.CurrentItemSelectionMenu ??= new(this.Helper.Input, this.ManagedChest.ItemMatcherByType);
        this.CurrentItemSelectionMenu.RegisterEvents(this.Helper.Events.Input);

        Game1.activeClickableMenu = this.CurrentItemSelectionMenu;
        e.SuppressInput();
    }
}