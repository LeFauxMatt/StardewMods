namespace StardewMods.BetterChests.Framework.Services.Features;

using HarmonyLib;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.BetterChests.Framework.Enums;
using StardewMods.BetterChests.Framework.Models.Events;
using StardewMods.Common.Services.Integrations.FuryCore;
using StardewValley.Menus;

// TODO: Add UI for sorting method

/// <summary>Sort items in a chest using a customized criteria.</summary>
internal sealed class OrganizeItems : BaseFeature<OrganizeItems>
{
#nullable disable
    private static OrganizeItems instance;
#nullable enable

    private readonly ContainerOperations containerOperations;
    private readonly Harmony harmony;
    private readonly IInputHelper inputHelper;
    private readonly PerScreen<bool> isActive = new();
    private readonly ItemGrabMenuManager itemGrabMenuManager;
    private readonly IModEvents modEvents;

    /// <summary>Initializes a new instance of the <see cref="OrganizeItems" /> class.</summary>
    /// <param name="configManager">Dependency used for accessing config data.</param>
    /// <param name="containerOperations">Dependency used for handling operations between containers.</param>
    /// <param name="harmony">Dependency used to patch external code.</param>
    /// <param name="inputHelper">Dependency used for checking and changing input state.</param>
    /// <param name="itemGrabMenuManager">Dependency used for managing the item grab menu.</param>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="modEvents">Dependency used for managing access to events.</param>
    public OrganizeItems(
        ConfigManager configManager,
        ContainerOperations containerOperations,
        Harmony harmony,
        IInputHelper inputHelper,
        ItemGrabMenuManager itemGrabMenuManager,
        ILog log,
        IManifest manifest,
        IModEvents modEvents)
        : base(log, manifest, configManager)
    {
        OrganizeItems.instance = this;
        this.containerOperations = containerOperations;
        this.harmony = harmony;
        this.inputHelper = inputHelper;
        this.itemGrabMenuManager = itemGrabMenuManager;
        this.modEvents = modEvents;
    }

    /// <inheritdoc />
    public override bool ShouldBeActive => this.Config.DefaultOptions.OrganizeItems != Option.Disabled;

    /// <inheritdoc />
    protected override void Activate()
    {
        // Events
        this.modEvents.Input.ButtonPressed += this.OnButtonPressed;
        this.itemGrabMenuManager.ItemGrabMenuChanged += this.OnItemGrabMenuChanged;

        // Patches
        this.harmony.Patch(
            AccessTools.DeclaredMethod(typeof(ItemGrabMenu), nameof(ItemGrabMenu.organizeItemsInList)),
            new HarmonyMethod(typeof(OrganizeItems), nameof(OrganizeItems.ItemGrabMenu_organizeItemsInList_prefix)));
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        // Events
        this.modEvents.Input.ButtonPressed -= this.OnButtonPressed;
        this.itemGrabMenuManager.ItemGrabMenuChanged -= this.OnItemGrabMenuChanged;

        // Patches
        this.harmony.Unpatch(
            AccessTools.DeclaredMethod(typeof(ItemGrabMenu), nameof(ItemGrabMenu.organizeItemsInList)),
            AccessTools.DeclaredMethod(
                typeof(OrganizeItems),
                nameof(OrganizeItems.ItemGrabMenu_organizeItemsInList_prefix)));
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static bool ItemGrabMenu_organizeItemsInList_prefix(IList<Item> items)
    {
        if (!OrganizeItems.instance.isActive.Value
            || OrganizeItems.instance.itemGrabMenuManager.Top.Container is not
                { } container
            || !items.Equals(container.Items)
            || container.Options is
            {
                OrganizeItemsGroupBy: GroupBy.Default,
                OrganizeItemsSortBy: SortBy.Default,
            })
        {
            return true;
        }

        OrganizeItems.instance.containerOperations.OrganizeItems(container);
        return false;
    }

    private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        if (!this.isActive.Value
            || e.Button is not (SButton.MouseLeft or SButton.MouseRight)
            || this.itemGrabMenuManager.CurrentMenu?.organizeButton is null
            || this.itemGrabMenuManager.Top.Container is null)
        {
            return;
        }

        var (mouseX, mouseY) = Game1.getMousePosition(true);
        if (!this.itemGrabMenuManager.CurrentMenu.organizeButton.containsPoint(mouseX, mouseY))
        {
            return;
        }

        this.inputHelper.Suppress(e.Button);
        Game1.playSound("Ship");
        this.containerOperations.OrganizeItems(this.itemGrabMenuManager.Top.Container, e.Button == SButton.MouseRight);
    }

    private void OnItemGrabMenuChanged(object? sender, ItemGrabMenuChangedEventArgs e)
    {
        if (this.itemGrabMenuManager.CurrentMenu is null
            || this.itemGrabMenuManager.Top.Container?.Options.OrganizeItems != Option.Enabled)
        {
            this.isActive.Value = false;
            return;
        }

        this.isActive.Value = this.itemGrabMenuManager.CurrentMenu.organizeButton != null;
    }
}