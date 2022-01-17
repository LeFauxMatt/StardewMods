namespace BetterChests.Features;

using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BetterChests.Models;
using CommonHarmony;
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
    private const string AutomateAssemblyType = "Pathoschild.Stardew.Automate.Framework.Storage.ChestContainer";
    private const string AutomateModUniqueId = "Pathochild.Automate";

    private readonly PerScreen<ManagedChest> _managedChest = new();
    private readonly PerScreen<MenuComponent> _configureButton = new();
    private readonly PerScreen<ItemGrabMenu> _returnMenu = new();
    private readonly PerScreen<ItemSelectionMenu> _itemSelectionMenu = new();
    private readonly Lazy<IFuryMenu> _customMenuComponents;
    private readonly Lazy<HarmonyHelper> _harmony;

    /// <summary>
    /// Initializes a new instance of the <see cref="CategorizeChest"/> class.
    /// </summary>
    /// <param name="config"></param>
    /// <param name="helper"></param>
    /// <param name="services"></param>
    public CategorizeChest(ModConfig config, IModHelper helper, ServiceCollection services)
        : base(config, helper, services)
    {
        CategorizeChest.Instance = this;
        this._customMenuComponents = services.Lazy<IFuryMenu>();
        this._harmony = services.Lazy<HarmonyHelper>(CategorizeChest.AddPatches);
    }

    private static CategorizeChest Instance { get; set; }

    private HarmonyHelper Harmony
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
        get => this._configureButton.Value;
        set => this._configureButton.Value = value;
    }

    private ItemSelectionMenu CurrentItemSelectionMenu
    {
        get => this._itemSelectionMenu.Value;
        set => this._itemSelectionMenu.Value = value;
    }

    private IFuryMenu FuryMenu
    {
        get => this._customMenuComponents.Value;
    }

    private ItemGrabMenu ReturnMenu
    {
        get => this._returnMenu.Value;
        set => this._returnMenu.Value = value;
    }

    /// <inheritdoc />
    public override void Activate()
    {
        this.Harmony.ApplyPatches(nameof(CategorizeChest));
        this.FuryEvents.ItemGrabMenuChanged += this.OnItemGrabMenuChanged;
        this.FuryEvents.MenuComponentPressed += this.OnMenuComponentPressed;
    }

    /// <inheritdoc />
    public override void Deactivate()
    {
        this.Harmony.UnapplyPatches(nameof(CategorizeChest));
        this.FuryEvents.ItemGrabMenuChanged -= this.OnItemGrabMenuChanged;
        this.FuryEvents.MenuComponentPressed -= this.OnMenuComponentPressed;
    }

    private static void AddPatches(HarmonyHelper harmony)
    {
        if (CategorizeChest.Instance.Helper.ModRegistry.IsLoaded(CategorizeChest.AutomateModUniqueId))
        {
            harmony.AddPatch(
                nameof(CategorizeChest),
                new AssemblyPatch("Automate").Method(CategorizeChest.AutomateAssemblyType, "Store"),
                typeof(CategorizeChest),
                nameof(CategorizeChest.Automate_Store_prefix));
        }
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Naming is determined by Harmony.")]
    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Type is determined by Harmony.")]
    private static bool Automate_Store_prefix(Chest ___Chest, object stack)
    {
        var item = CategorizeChest.Instance.Helper.Reflection.GetProperty<Item>(stack, "Sample").GetValue();
        return !CategorizeChest.Instance.ManagedChests.FindChest(___Chest, out var managedChest) || managedChest.ItemMatcher.Matches(item);
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
                this.ConfigureButton ??= new(new(
                    new(0, 0, Game1.tileSize, Game1.tileSize),
                    this.Helper.Content.Load<Texture2D>("assets/configure.png"),
                    Rectangle.Empty,
                    Game1.pixelZoom))
                {
                    Name = "Configure",
                };
                this.FuryMenu.SideComponents.Insert(0, this.ConfigureButton);
                this.ReturnMenu = e.ItemGrabMenu;
                this.ManagedChest = managedChest;
                return;

            // Exit ItemSelectionMenu
            case null when this.ReturnMenu is not null && this.CurrentItemSelectionMenu is not null && this.ManagedChest is not null:
                // Save ItemSelectionMenu to ModData
                var filterItems = this.ManagedChest.ItemMatcher.StringValue;
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
        this.CurrentItemSelectionMenu ??= new(this.Helper.Input, this.ManagedChest.ItemMatcher);
        this.CurrentItemSelectionMenu.RegisterEvents(this.Helper.Events.Input);

        Game1.activeClickableMenu = this.CurrentItemSelectionMenu;
        e.SuppressInput();
    }
}