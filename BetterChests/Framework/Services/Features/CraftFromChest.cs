namespace StardewMods.BetterChests.Framework.Services.Features;

using System.Reflection.Emit;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI.Events;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.BetterChests.Framework.Services.Factory;
using StardewMods.Common.Interfaces;
using StardewMods.Common.Services.Integrations.BetterChests.Enums;
using StardewMods.Common.Services.Integrations.BetterChests.Interfaces;
using StardewMods.Common.Services.Integrations.FauxCore;
using StardewMods.Common.Services.Integrations.ToolbarIcons;
using StardewValley.Inventories;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Network;
using StardewValley.Objects;

/// <summary>Craft using items from placed chests and chests in the farmer's inventory.</summary>
internal sealed class CraftFromChest : BaseFeature<CraftFromChest>
{
    private static CraftFromChest instance = null!;

    private readonly AssetHandler assetHandler;
    private readonly ContainerFactory containerFactory;
    private readonly Harmony harmony;
    private readonly IInputHelper inputHelper;
    private readonly ToolbarIconsIntegration toolbarIconsIntegration;

    /// <summary>Initializes a new instance of the <see cref="CraftFromChest" /> class.</summary>
    /// <param name="assetHandler">Dependency used for handling assets.</param>
    /// <param name="containerFactory">Dependency used for accessing containers.</param>
    /// <param name="eventManager">Dependency used for managing events.</param>
    /// <param name="harmony">Dependency used to patch external code.</param>
    /// <param name="inputHelper">Dependency used for checking and changing input state.</param>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="manifest">Dependency for accessing mod manifest.</param>
    /// <param name="modConfig">Dependency used for accessing config data.</param>
    /// <param name="toolbarIconsIntegration">Dependency for Toolbar Icons integration.</param>
    public CraftFromChest(
        AssetHandler assetHandler,
        ContainerFactory containerFactory,
        IEventManager eventManager,
        Harmony harmony,
        IInputHelper inputHelper,
        ILog log,
        IManifest manifest,
        IModConfig modConfig,
        ToolbarIconsIntegration toolbarIconsIntegration)
        : base(eventManager, log, manifest, modConfig)
    {
        CraftFromChest.instance = this;
        this.assetHandler = assetHandler;
        this.containerFactory = containerFactory;
        this.harmony = harmony;
        this.inputHelper = inputHelper;
        this.toolbarIconsIntegration = toolbarIconsIntegration;
    }

    /// <inheritdoc />
    public override bool ShouldBeActive => this.Config.DefaultOptions.CraftFromChest != RangeOption.Disabled;

    /// <inheritdoc />
    protected override void Activate()
    {
        // Events
        this.Events.Subscribe<ButtonsChangedEventArgs>(this.OnButtonsChanged);
        this.Events.Subscribe<ButtonPressedEventArgs>(this.OnButtonPressed);

        // Patches
        this.harmony.Patch(
            AccessTools.DeclaredConstructor(typeof(GameMenu), [typeof(bool)]),
            transpiler: new HarmonyMethod(
                typeof(CraftFromChest),
                nameof(CraftFromChest.GameMenu_constructor_transpiler)));

        // Integrations
        if (!this.toolbarIconsIntegration.IsLoaded)
        {
            return;
        }

        this.toolbarIconsIntegration.Api.AddToolbarIcon(
            this.Id,
            this.assetHandler.IconTexturePath,
            new Rectangle(32, 0, 16, 16),
            I18n.Button_CraftFromChest_Name());

        this.toolbarIconsIntegration.Api.Subscribe<IIconPressedEventArgs>(this.OnIconPressed);
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        // Events
        this.Events.Unsubscribe<ButtonsChangedEventArgs>(this.OnButtonsChanged);
        this.Events.Unsubscribe<ButtonPressedEventArgs>(this.OnButtonPressed);

        // Patches
        this.harmony.Unpatch(
            AccessTools.DeclaredConstructor(typeof(GameMenu), [typeof(bool)]),
            AccessTools.DeclaredMethod(typeof(CraftFromChest), nameof(CraftFromChest.GameMenu_constructor_transpiler)));

        // Integrations
        if (!this.toolbarIconsIntegration.IsLoaded)
        {
            return;
        }

        this.toolbarIconsIntegration.Api.RemoveToolbarIcon(this.Id);
        this.toolbarIconsIntegration.Api.Unsubscribe<IIconPressedEventArgs>(this.OnIconPressed);
    }

    private static IEnumerable<CodeInstruction> GameMenu_constructor_transpiler(
        IEnumerable<CodeInstruction> instructions)
    {
        var found = false;
        var craftingPageConstructor = AccessTools.GetDeclaredConstructors(typeof(CraftingPage)).First();
        foreach (var instruction in instructions)
        {
            if (found)
            {
                if (instruction.Is(OpCodes.Newobj, craftingPageConstructor))
                {
                    yield return CodeInstruction.Call(typeof(CraftFromChest), nameof(CraftFromChest.GetMaterials));
                }
                else
                {
                    yield return new CodeInstruction(OpCodes.Ldnull);
                }
            }

            found = instruction.opcode == OpCodes.Ldnull;
            if (!found)
            {
                yield return instruction;
            }
        }
    }

    private static List<IInventory>? GetMaterials()
    {
        var containers = CraftFromChest.instance.containerFactory.GetAll(Predicate).ToList();
        return containers.Count > 0 ? containers.Select(container => container.Items).ToList() : null;

        bool Predicate(IStorageContainer container) =>
            container.Options.CraftFromChest is not (RangeOption.Disabled or RangeOption.Default)
            && container.Items.Count > 0
            && !CraftFromChest.instance.Config.CraftFromChestDisableLocations.Contains(
                Game1.player.currentLocation.Name)
            && !(CraftFromChest.instance.Config.CraftFromChestDisableLocations.Contains("UndergroundMine")
                && Game1.player.currentLocation is MineShaft mineShaft
                && mineShaft.Name.StartsWith("UndergroundMine", StringComparison.OrdinalIgnoreCase))
            && container.Options.CraftFromChest.WithinRange(
                container.Options.CraftFromChestDistance,
                container.Location,
                container.TileLocation);
    }

    private void OnButtonPressed(ButtonPressedEventArgs e)
    {
        if (this.Config.CraftFromWorkbench is RangeOption.Disabled or RangeOption.Default
            || !Context.IsPlayerFree
            || Game1.player.CurrentItem is Tool
            || !e.Button.IsUseToolButton()
            || this.inputHelper.IsSuppressed(e.Button))
        {
            return;
        }

        if (!Game1.currentLocation.Objects.TryGetValue(e.Cursor.GrabTile, out var obj) || obj is not Workbench)
        {
            return;
        }

        this.OpenCraftingMenu(this.WorkbenchPredicate);
    }

    private void OnButtonsChanged(ButtonsChangedEventArgs e)
    {
        if (!Context.IsPlayerFree || !this.Config.Controls.OpenCrafting.JustPressed())
        {
            return;
        }

        this.inputHelper.SuppressActiveKeybinds(this.Config.Controls.OpenCrafting);
        this.OpenCraftingMenu(this.DefaultPredicate);
    }

    private void OnIconPressed(IIconPressedEventArgs e)
    {
        if (e.Id == this.Id)
        {
            this.OpenCraftingMenu(this.DefaultPredicate);
        }
    }

    private void OpenCraftingMenu(Func<IStorageContainer, bool> predicate)
    {
        var containers = this.containerFactory.GetAll(predicate).ToList();
        if (containers.Count == 0)
        {
            this.Log.Alert(I18n.Alert_CraftFromChest_NoEligible());
            return;
        }

        var mutexes = containers.Select(container => container.Mutex).OfType<NetMutex>().ToArray();
        var inventories = containers.Select(container => container.Items).ToList();
        _ = new MultipleMutexRequest(
            mutexes,
            request =>
            {
                var width = 800 + (IClickableMenu.borderWidth * 2);
                var height = 600 + (IClickableMenu.borderWidth * 2);
                var (x, y) = Utility.getTopLeftPositionForCenteringOnScreen(width, height).ToPoint();
                Game1.activeClickableMenu = new CraftingPage(x, y, width, height, false, true, inventories);
                Game1.activeClickableMenu.exitFunction = request.ReleaseLocks;
            },
            _ =>
            {
                this.Log.Alert(I18n.Alert_CraftFromChest_NoEligible());
            });
    }

    private bool DefaultPredicate(IStorageContainer container) =>
        container.Options.CraftFromChest is not (RangeOption.Disabled or RangeOption.Default)
        && container.Items.Count > 0
        && !this.Config.CraftFromChestDisableLocations.Contains(Game1.player.currentLocation.Name)
        && !(this.Config.CraftFromChestDisableLocations.Contains("UndergroundMine")
            && Game1.player.currentLocation is MineShaft mineShaft
            && mineShaft.Name.StartsWith("UndergroundMine", StringComparison.OrdinalIgnoreCase))
        && container.Options.CraftFromChest.WithinRange(
            container.Options.CraftFromChestDistance,
            container.Location,
            container.TileLocation);

    private bool WorkbenchPredicate(IStorageContainer container) =>
        container.Options.CraftFromChest is not RangeOption.Disabled
        && container.Items.Count > 0
        && !CraftFromChest.instance.Config.CraftFromChestDisableLocations.Contains(Game1.player.currentLocation.Name)
        && !(CraftFromChest.instance.Config.CraftFromChestDisableLocations.Contains("UndergroundMine")
            && Game1.player.currentLocation is MineShaft mineShaft
            && mineShaft.Name.StartsWith("UndergroundMine", StringComparison.OrdinalIgnoreCase))
        && CraftFromChest.instance.Config.CraftFromWorkbench.WithinRange(
            CraftFromChest.instance.Config.CraftFromWorkbenchDistance,
            container.Location,
            container.TileLocation);
}