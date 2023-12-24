namespace StardewMods.BetterChests.Framework.Services.Features;

using HarmonyLib;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewMods.BetterChests.Framework.Enums;
using StardewMods.BetterChests.Framework.Models.Events;
using StardewMods.Common.Services.Integrations.FuryCore;
using StardewValley.Menus;

/// <summary>Sort items in a chest using a customized criteria.</summary>
internal sealed class OrganizeChest : BaseFeature
{
#nullable disable
    private static OrganizeChest instance;
#nullable enable

    private readonly Harmony harmony;
    private readonly IInputHelper inputHelper;
    private readonly PerScreen<bool> isActive = new();
    private readonly ItemGrabMenuManager itemGrabMenuManager;
    private readonly IModEvents modEvents;

    /// <summary>Initializes a new instance of the <see cref="OrganizeChest" /> class.</summary>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="modConfig">Dependency used for accessing config data.</param>
    /// <param name="harmony">Dependency used to patch external code.</param>
    /// <param name="inputHelper">Dependency used for checking and changing input state.</param>
    /// <param name="itemGrabMenuManager">Dependency used for managing the item grab menu.</param>
    /// <param name="modEvents">Dependency used for managing access to events.</param>
    public OrganizeChest(
        ILog log,
        ModConfig modConfig,
        Harmony harmony,
        IInputHelper inputHelper,
        ItemGrabMenuManager itemGrabMenuManager,
        IModEvents modEvents)
        : base(log, modConfig)
    {
        OrganizeChest.instance = this;
        this.harmony = harmony;
        this.inputHelper = inputHelper;
        this.itemGrabMenuManager = itemGrabMenuManager;
        this.modEvents = modEvents;
    }

    /// <inheritdoc />
    public override bool ShouldBeActive => this.ModConfig.DefaultOptions.OrganizeChest != Option.Disabled;

    /// <summary>Organizes the items in the collection.</summary>
    /// <param name="reverse">Determines whether to sort the items in reverse order. The default value is false.</param>
    public void OrganizeItems(bool reverse = false)
    {
        if (this.itemGrabMenuManager.Top.Container == null)
        {
            return;
        }

        var options = this.itemGrabMenuManager.Top.Container.Options;
        if (options is
            {
                OrganizeChestGroupBy: GroupBy.Default,
                OrganizeChestSortBy: SortBy.Default,
            })
        {
            ItemGrabMenu.organizeItemsInList(this.itemGrabMenuManager.Top.Container.Items);
            return;
        }

        var items = this.itemGrabMenuManager.Top.Container.Items.ToArray();
        Array.Sort(
            items,
            (i1, i2) =>
            {
                if (i2 == null)
                {
                    return -1;
                }

                if (i1 == null)
                {
                    return 1;
                }

                if (i1.Equals(i2))
                {
                    return 0;
                }

                var g1 = options.OrganizeChestGroupBy switch
                    {
                        GroupBy.Category => i1
                            .GetContextTags()
                            .FirstOrDefault(tag => tag.StartsWith("category_", StringComparison.OrdinalIgnoreCase)),
                        GroupBy.Color => i1
                            .GetContextTags()
                            .FirstOrDefault(tag => tag.StartsWith("color_", StringComparison.OrdinalIgnoreCase)),
                        GroupBy.Name => i1.DisplayName,
                        _ => null,
                    }
                    ?? string.Empty;

                var g2 = options.OrganizeChestGroupBy switch
                    {
                        GroupBy.Category => i2
                            .GetContextTags()
                            .FirstOrDefault(tag => tag.StartsWith("category_", StringComparison.OrdinalIgnoreCase)),
                        GroupBy.Color => i2
                            .GetContextTags()
                            .FirstOrDefault(tag => tag.StartsWith("color_", StringComparison.OrdinalIgnoreCase)),
                        GroupBy.Name => i2.DisplayName,
                        _ => null,
                    }
                    ?? string.Empty;

                if (!g1.Equals(g2, StringComparison.OrdinalIgnoreCase))
                {
                    return string.Compare(g1, g2, StringComparison.OrdinalIgnoreCase);
                }

                var o1 = options.OrganizeChestSortBy switch
                {
                    SortBy.Type => i1.Category, SortBy.Quality => i1.Quality, SortBy.Quantity => i1.Stack, _ => 0,
                };

                var o2 = options.OrganizeChestSortBy switch
                {
                    SortBy.Type => i2.Category, SortBy.Quality => i2.Quality, SortBy.Quantity => i2.Stack, _ => 0,
                };

                return o1.CompareTo(o2);
            });

        if (reverse)
        {
            Array.Reverse(items);
        }

        this.itemGrabMenuManager.Top.Container.Items.OverwriteWith(items);
    }

    /// <inheritdoc />
    protected override void Activate()
    {
        // Events
        this.modEvents.Input.ButtonPressed += this.OnButtonPressed;
        this.itemGrabMenuManager.ItemGrabMenuChanged += this.OnItemGrabMenuChanged;

        // Patches
        this.harmony.Patch(
            AccessTools.DeclaredMethod(typeof(ItemGrabMenu), nameof(ItemGrabMenu.organizeItemsInList)),
            new HarmonyMethod(typeof(OrganizeChest), nameof(OrganizeChest.ItemGrabMenu_organizeItemsInList_prefix)));
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
                typeof(OrganizeChest),
                nameof(OrganizeChest.ItemGrabMenu_organizeItemsInList_prefix)));
    }

    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Harmony")]
    [SuppressMessage("StyleCop", "SA1313", Justification = "Harmony")]
    private static bool ItemGrabMenu_organizeItemsInList_prefix(IList<Item> items)
    {
        if (!OrganizeChest.instance.isActive.Value
            || !items.Equals(OrganizeChest.instance.itemGrabMenuManager.Top.Container?.Items))
        {
            return true;
        }

        OrganizeChest.instance.OrganizeItems();
        return false;
    }

    private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        if (!this.isActive.Value
            || e.Button is not (SButton.MouseLeft or SButton.MouseRight)
            || this.itemGrabMenuManager.CurrentMenu?.organizeButton is null)
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
        this.OrganizeItems(true);
    }

    private void OnItemGrabMenuChanged(object? sender, ItemGrabMenuChangedEventArgs e)
    {
        if (this.itemGrabMenuManager.CurrentMenu is null
            || this.itemGrabMenuManager.Top.Container?.Options.OrganizeChest != Option.Enabled)
        {
            this.isActive.Value = false;
            return;
        }

        this.isActive.Value = this.itemGrabMenuManager.CurrentMenu.organizeButton != null;
    }
}