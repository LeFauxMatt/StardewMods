namespace StardewMods.BetterChests.Framework.Services.Features;

using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI.Events;
using StardewMods.BetterChests.Framework.Enums;
using StardewMods.BetterChests.Framework.Interfaces;
using StardewMods.BetterChests.Framework.Models.Containers;
using StardewMods.BetterChests.Framework.Services.Factory;
using StardewMods.Common.Services.Integrations.FuryCore;
using StardewMods.Common.Services.Integrations.ToolbarIcons;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Network;
using StardewValley.Objects;

/// <summary>Craft using items from placed chests and chests in the farmer's inventory.</summary>
internal sealed class CraftFromChest : BaseFeature
{
#nullable disable
    private static CraftFromChest instance;
#nullable enable

    private readonly ContainerFactory containerFactory;
    private readonly Harmony harmony;
    private readonly IInputHelper inputHelper;
    private readonly IModEvents modEvents;
    private readonly ToolbarIconsIntegration toolbarIconsIntegration;

    /// <summary>Initializes a new instance of the <see cref="CraftFromChest" /> class.</summary>
    /// <param name="log">Dependency used for logging debug information to the console.</param>
    /// <param name="modConfig">Dependency used for accessing config data.</param>
    /// <param name="containerFactory">Dependency used for accessing containers.</param>
    /// <param name="harmony">Dependency used to patch external code.</param>
    /// <param name="inputHelper">Dependency used for checking and changing input state.</param>
    /// <param name="modEvents">Dependency used for managing access to events.</param>
    /// <param name="toolbarIconsIntegration">Dependency for Toolbar Icons integration.</param>
    public CraftFromChest(
        ILog log,
        ModConfig modConfig,
        ContainerFactory containerFactory,
        Harmony harmony,
        IInputHelper inputHelper,
        IModEvents modEvents,
        ToolbarIconsIntegration toolbarIconsIntegration)
        : base(log, modConfig)
    {
        CraftFromChest.instance = this;
        this.containerFactory = containerFactory;
        this.harmony = harmony;
        this.inputHelper = inputHelper;
        this.modEvents = modEvents;
        this.toolbarIconsIntegration = toolbarIconsIntegration;
    }

    /// <inheritdoc />
    public override bool ShouldBeActive => this.ModConfig.DefaultOptions.CraftFromChest != RangeOption.Disabled;

    /// <inheritdoc />
    protected override void Activate()
    {
        // Events
        this.modEvents.Input.ButtonsChanged += this.OnButtonsChanged;

        // Patches
        this.harmony.Patch(
            AccessTools.DeclaredConstructor(typeof(GameMenu), [typeof(bool)]),
            transpiler: new HarmonyMethod(
                typeof(CraftFromChest),
                nameof(CraftFromChest.GameMenu_constructor_transpiler)));

        this.harmony.Patch(
            AccessTools.DeclaredMethod(typeof(Workbench), nameof(Workbench.checkForAction)),
            transpiler: new HarmonyMethod(
                typeof(CraftFromChest),
                nameof(CraftFromChest.Workbench_checkForAction_transpiler)));

        // Integrations
        if (!this.toolbarIconsIntegration.IsLoaded)
        {
            return;
        }

        this.toolbarIconsIntegration.Api.AddToolbarIcon(
            this.Id,
            AssetHandler.IconTexturePath,
            new Rectangle(32, 0, 16, 16),
            I18n.Button_CraftFromChest_Name());

        this.toolbarIconsIntegration.Api.ToolbarIconPressed += this.OnToolbarIconPressed;
    }

    /// <inheritdoc />
    protected override void Deactivate()
    {
        // Events
        this.modEvents.Input.ButtonsChanged -= this.OnButtonsChanged;

        // Patches
        this.harmony.Unpatch(
            AccessTools.DeclaredConstructor(typeof(GameMenu), [typeof(bool)]),
            AccessTools.DeclaredMethod(typeof(CraftFromChest), nameof(CraftFromChest.GameMenu_constructor_transpiler)));

        this.harmony.Unpatch(
            AccessTools.DeclaredMethod(typeof(Workbench), nameof(Workbench.checkForAction)),
            AccessTools.DeclaredMethod(
                typeof(CraftFromChest),
                nameof(CraftFromChest.Workbench_checkForAction_transpiler)));

        // Integrations
        if (!this.toolbarIconsIntegration.IsLoaded)
        {
            return;
        }

        this.toolbarIconsIntegration.Api.RemoveToolbarIcon(this.Id);
        this.toolbarIconsIntegration.Api.ToolbarIconPressed -= this.OnToolbarIconPressed;
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

    private static IEnumerable<CodeInstruction> Workbench_checkForAction_transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var found = default(CodeInstruction);
        foreach (var instruction in instructions)
        {
            found ??= instruction.opcode == OpCodes.Ldfld && instruction.operand is FieldInfo
                {
                    Name: "nearby_chests",
                } ? instruction : null;

            if (found is not null && instruction.Is(OpCodes.Newobj, AccessTools.Constructor(typeof(List<NetMutex>))))
            {
                yield return new CodeInstruction(OpCodes.Ldloc_0);
                yield return found;
                yield return CodeInstruction.Call(typeof(CraftFromChest), nameof(CraftFromChest.AddNearbyChests));
                yield return instruction;
            }
            else
            {
                yield return instruction;
            }
        }
    }

    [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Harmony")]
    private static void AddNearbyChests(List<Chest> nearbyChests)
    {
        if (CraftFromChest.instance.ModConfig.CraftFromWorkbench is RangeOption.Disabled or RangeOption.Default)
        {
            return;
        }

        var containers = CraftFromChest.instance.containerFactory.GetAll(Predicate).OfType<ChestContainer>();
        foreach (var container in containers)
        {
            if (!nearbyChests.Contains(container.Chest))
            {
                nearbyChests.Add(container.Chest);
            }
        }

        return;

        bool Predicate(IContainer container) =>
            container.Options.CraftFromChest is not RangeOption.Disabled
            && container.Items.Count > 0
            && !container.Options.CraftFromChestDisableLocations.Contains(Game1.player.currentLocation.Name)
            && !(container.Options.CraftFromChestDisableLocations.Contains("UndergroundMine")
                && Game1.player.currentLocation is MineShaft mineShaft
                && mineShaft.Name.StartsWith("UndergroundMine", StringComparison.OrdinalIgnoreCase))
            && CraftFromChest.instance.ModConfig.CraftFromWorkbench.WithinRange(
                CraftFromChest.instance.ModConfig.CraftFromWorkbenchDistance,
                container.Location,
                container.TileLocation);
    }

    private static List<Chest>? GetMaterials()
    {
        var containers = CraftFromChest.instance.containerFactory.GetAll(Predicate).OfType<ChestContainer>().ToList();
        return containers.Count > 0 ? containers.Select(container => container.Chest).ToList() : null;

        bool Predicate(IContainer container) =>
            container.Options.CraftFromChest is not (RangeOption.Disabled or RangeOption.Default)
            && container.Items.Count > 0
            && !container.Options.CraftFromChestDisableLocations.Contains(Game1.player.currentLocation.Name)
            && !(container.Options.CraftFromChestDisableLocations.Contains("UndergroundMine")
                && Game1.player.currentLocation is MineShaft mineShaft
                && mineShaft.Name.StartsWith("UndergroundMine", StringComparison.OrdinalIgnoreCase))
            && container.Options.CraftFromChest.WithinRange(
                container.Options.CraftFromChestDistance,
                container.Location,
                container.TileLocation);
    }

    private void OnButtonsChanged(object? sender, ButtonsChangedEventArgs e)
    {
        if (!Context.IsPlayerFree || !this.ModConfig.Controls.OpenCrafting.JustPressed())
        {
            return;
        }

        this.inputHelper.SuppressActiveKeybinds(this.ModConfig.Controls.OpenCrafting);
        this.OpenCraftingMenu();
    }

    private void OnToolbarIconPressed(object? sender, string id)
    {
        if (id == this.Id)
        {
            this.OpenCraftingMenu();
        }
    }

    private void OpenCraftingMenu()
    {
        var containers = this.containerFactory.GetAll(Predicate).OfType<ChestContainer>().ToList();
        if (containers.Count == 0)
        {
            this.Log.Alert(I18n.Alert_CraftFromChest_NoEligible());
            return;
        }

        var chests = containers.Select(container => container.Chest).ToList();
        _ = new MultipleMutexRequest(
            chests.Select(chest => chest.GetMutex()).ToArray(),
            request =>
            {
                var width = 800 + (IClickableMenu.borderWidth * 2);
                var height = 600 + (IClickableMenu.borderWidth * 2);
                var (x, y) = Utility.getTopLeftPositionForCenteringOnScreen(width, height).ToPoint();
                Game1.activeClickableMenu = new CraftingPage(x, y, width, height, false, true, chests);
                Game1.activeClickableMenu.exitFunction = request.ReleaseLocks;
            },
            _ =>
            {
                this.Log.Alert(I18n.Alert_CraftFromChest_NoEligible());
            });

        return;

        bool Predicate(IContainer container) =>
            container.Options.CraftFromChest is not (RangeOption.Disabled or RangeOption.Default)
            && container.Items.Count > 0
            && !container.Options.CraftFromChestDisableLocations.Contains(Game1.player.currentLocation.Name)
            && !(container.Options.CraftFromChestDisableLocations.Contains("UndergroundMine")
                && Game1.player.currentLocation is MineShaft mineShaft
                && mineShaft.Name.StartsWith("UndergroundMine", StringComparison.OrdinalIgnoreCase))
            && container.Options.CraftFromChest.WithinRange(
                container.Options.CraftFromChestDistance,
                container.Location,
                container.TileLocation);
    }
}