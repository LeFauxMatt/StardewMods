using System.Collections.Generic;
using System.Linq;
using Common.PatternPatches;
using ExpandedStorage.Common.Extensions;
using ExpandedStorage.Framework;
using ExpandedStorage.Framework.Extensions;
using ExpandedStorage.Framework.Integrations;
using ExpandedStorage.Framework.Models;
using ExpandedStorage.Framework.Patches;
using ExpandedStorage.Framework.UI;
using MoreCraftables.API;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;
using ObjectFactory = ExpandedStorage.Framework.ObjectFactory;

// ReSharper disable ClassNeverInstantiated.Global

namespace ExpandedStorage
{
    public class ExpandedStorage : Mod, IAssetEditor
    {
        /// <summary>Tracks previously held chest before placing into world.</summary>
        internal static readonly PerScreen<Chest> HeldChest = new();

        /// <summary>Tracks all chests that may be used for vacuum items.</summary>
        internal static readonly PerScreen<IDictionary<Chest, Storage>> VacuumChests = new();

        /// <summary>Dictionary of Expanded Storage configs</summary>
        private static readonly IDictionary<string, Storage> Storages = new Dictionary<string, Storage>();

        /// <summary>Dictionary of Expanded Storage tabs</summary>
        private static readonly IDictionary<string, StorageTab> StorageTabs = new Dictionary<string, StorageTab>();

        /// <summary>Tracks previously held chest lid frame.</summary>
        private readonly PerScreen<int> _currentLidFrame = new();

        /// <summary>Reflected currentLidFrame for previousHeldChest.</summary>
        private readonly PerScreen<IReflectedField<int>> _currentLidFrameReflected = new();

        /// <summary>The mod configuration.</summary>
        private ModConfig _config;

        private ContentLoader _contentLoader;

        private ExpandedStorageAPI _expandedStorageAPI;
        private IMoreCraftablesAPI _moreCraftablesAPI;

        /// <summary>Get whether this instance can load the initial version of the given asset.</summary>
        /// <param name="asset">Basic metadata about the asset being loaded.</param>
        public bool CanEdit<T>(IAssetInfo asset)
        {
            // Load bigCraftable on next tick for vanilla storages
            if (asset.AssetNameEquals("Data/BigCraftablesInformation"))
                Helper.Events.GameLoop.UpdateTicked += _expandedStorageAPI.OnAssetsLoaded;
            return false;
        }

        /// <summary>Load a matched asset.</summary>
        /// <param name="asset">Basic metadata about the asset being loaded.</param>
        public void Edit<T>(IAssetData asset)
        {
        }

        /// <summary>Returns ExpandedStorageConfig by item name.</summary>
        public static Storage GetConfig(object context)
        {
            return Storages
                .Select(c => c.Value)
                .FirstOrDefault(c => c.MatchesContext(context));
        }

        /// <summary>Returns true if item is an ExpandedStorage.</summary>
        public static bool HasConfig(object context)
        {
            return Storages.Any(c => c.Value.MatchesContext(context));
        }

        /// <summary>Returns ExpandedStorageTab by tab name.</summary>
        public static StorageTab GetTab(string tabName)
        {
            return StorageTabs.TryGetValue(tabName, out var tab) ? tab : null;
        }

        public override object GetApi()
        {
            return _expandedStorageAPI;
        }

        public override void Entry(IModHelper helper)
        {
            _config = helper.ReadConfig<ModConfig>();
            Monitor.Log(_config.SummaryReport, LogLevel.Debug);

            _expandedStorageAPI = new ExpandedStorageAPI(Monitor, Helper, Storages, StorageTabs);
            _contentLoader = new ContentLoader(Monitor, Helper, _expandedStorageAPI);

            if (helper.ModRegistry.IsLoaded("spacechase0.CarryChest"))
            {
                Monitor.Log("Expanded Storage should not be run alongside Carry Chest!", LogLevel.Warn);
                _config.AllowCarryingChests = false;
            }

            var isAutomateLoaded = helper.ModRegistry.IsLoaded("Pathoschild.Automate");
            ChestExtensions.Init(helper.Reflection);
            FarmerExtensions.Init(Monitor);
            MenuViewModel.Init(helper.Events, helper.Input, _config);
            MenuModel.Init(_config);

            // Events
            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
            helper.Events.World.ObjectListChanged += OnObjectListChanged;

            if (_config.AllowCarryingChests)
            {
                helper.Events.GameLoop.UpdateTicking += OnUpdateTicking;
                helper.Events.Input.ButtonPressed += OnButtonPressed;
            }

            if (_config.AllowAccessCarriedChest) helper.Events.Input.ButtonsChanged += OnButtonsChanged;

            if (_config.AllowVacuumItems)
            {
                helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
                helper.Events.Player.InventoryChanged += OnInventoryChanged;
            }

            // Harmony Patches
            new Patcher<ModConfig>(ModManifest.UniqueID).ApplyAll(
                new FarmerPatch(Monitor, _config),
                //new ItemPatch(Monitor, _config),
                new ObjectPatch(Monitor, _config),
                new ChestPatch(Monitor, _config),
                new ItemGrabMenuPatch(Monitor, _config, helper.Reflection),
                new InventoryMenuPatch(Monitor, _config),
                new MenuWithInventoryPatch(Monitor, _config),
                new DebrisPatch(Monitor, _config),
                new AutomatePatch(Monitor, _config, helper.Reflection, isAutomateLoaded));
        }

        /// <summary>Setup Generic Mod Config Menu</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            _moreCraftablesAPI = Helper.ModRegistry.GetApi<IMoreCraftablesAPI>("furyx639.MoreCraftables");
            _moreCraftablesAPI.AddHandledType(ModManifest, new HandledType());
            _moreCraftablesAPI.AddObjectFactory(ModManifest, new ObjectFactory());

            var modConfigApi = Helper.ModRegistry.GetApi<IGenericModConfigMenuAPI>("spacechase0.GenericModConfigMenu");
            if (modConfigApi == null)
                return;

            modConfigApi.RegisterModConfig(ModManifest,
                () => _config = new ModConfig(),
                () => Helper.WriteConfig(_config));
            ModConfig.RegisterModConfig(ModManifest, modConfigApi, _config);
        }

        /// <summary>Track toolbar changes before user input.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private static void OnObjectListChanged(object sender, ObjectListChangedEventArgs e)
        {
            if (!Context.IsPlayerFree)
                return;

            var oldChest = HeldChest.Value;
            var chest = e.Added
                .Select(p => p.Value)
                .OfType<Chest>()
                .LastOrDefault(HasConfig);

            if (oldChest == null || chest == null || chest.items.Any() || !ReferenceEquals(e.Location, Game1.currentLocation))
                return;

            // Backup method for restoring carried Chest items
            chest.name = oldChest.name;
            chest.playerChoiceColor.Value = oldChest.playerChoiceColor.Value;
            if (oldChest.items.Any())
                chest.items.CopyFrom(oldChest.items);
            foreach (var modData in oldChest.modData)
                chest.modData.CopyFrom(modData);
        }

        /// <summary>Initialize player item vacuum chests.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            if (!Game1.player.IsLocalPlayer)
                return;

            VacuumChests.Value = Game1.player.Items
                .Take(_config.VacuumToFirstRow ? 12 : Game1.player.MaxItems)
                .Where(i => i is Chest)
                .ToDictionary(i => i as Chest, GetConfig)
                .Where(s => s.Value != null && s.Value.VacuumItems)
                .ToDictionary(s => s.Key, s => s.Value);

            Monitor.Log($"Found {VacuumChests.Value.Count} For Vacuum\n" + string.Join("\n", VacuumChests.Value.Select(s => $"\t{s.Value.StorageName}")), LogLevel.Debug);
        }

        /// <summary>Refresh player item vacuum chests.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnInventoryChanged(object sender, InventoryChangedEventArgs e)
        {
            if (!e.IsLocalPlayer)
                return;

            VacuumChests.Value = e.Player.Items
                .Take(_config.VacuumToFirstRow ? 12 : e.Player.MaxItems)
                .Where(i => i is Chest)
                .ToDictionary(i => i as Chest, GetConfig)
                .Where(s => s.Value != null && s.Value.VacuumItems)
                .ToDictionary(s => s.Key, s => s.Value);

            Monitor.VerboseLog($"Found {VacuumChests.Value.Count} For Vacuum\n" + string.Join("\n", VacuumChests.Value.Select(s => $"\t{s.Value.StorageName}")));
        }

        /// <summary>Track toolbar changes before user input.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnUpdateTicking(object sender, UpdateTickingEventArgs e)
        {
            if (!Context.IsPlayerFree)
                return;

            if (Game1.player.CurrentItem is not Chest chest)
            {
                HeldChest.Value = null;
                return;
            }

            if (!ReferenceEquals(HeldChest.Value, chest))
            {
                HeldChest.Value = chest;
                chest.fixLidFrame();
            }

            if (!_config.AllowAccessCarriedChest
                || chest.frameCounter.Value <= -1
                || _currentLidFrame.Value > chest.getLastLidFrame())
                return;

            chest.frameCounter.Value--;
            if (chest.frameCounter.Value > 0
                || !chest.GetMutex().IsLockHeld())
                return;

            if (_currentLidFrame.Value == chest.getLastLidFrame())
            {
                chest.frameCounter.Value = -1;
                _currentLidFrame.Value = chest.startingLidFrame.Value;
                _currentLidFrameReflected.Value.SetValue(_currentLidFrame.Value);
                chest.ShowMenu();
            }
            else
            {
                chest.frameCounter.Value = 5;
                _currentLidFrame.Value++;
                _currentLidFrameReflected.Value.SetValue(_currentLidFrame.Value);
            }
        }

        /// <summary>Track toolbar changes before user input.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsPlayerFree)
                return;

            var location = Game1.currentLocation;
            var pos = Game1.player.GetToolLocation() / 64f;
            pos.X = (int) pos.X;
            pos.Y = (int) pos.Y;

            if (HeldChest.Value == null
                && _config.AllowCarryingChests
                && e.Button.IsUseToolButton()
                && location.CarryChest(pos))
            {
                Helper.Input.Suppress(e.Button);
            }
            else if (HeldChest.Value != null
                     && _config.AllowAccessCarriedChest
                     && e.Button.IsActionButton()
                     && HeldChest.Value.Stack <= 1)
            {
                if (location.objects.TryGetValue(pos, out var obj) && HasConfig(obj))
                    return;

                var config = GetConfig(HeldChest.Value);
                if (!config.AccessCarried)
                    return;

                HeldChest.Value.GetMutex().RequestLock(delegate
                {
                    HeldChest.Value.fixLidFrame();
                    HeldChest.Value.performOpenChest();
                    _currentLidFrameReflected.Value = Helper.Reflection.GetField<int>(HeldChest.Value, "currentLidFrame");
                    _currentLidFrame.Value = HeldChest.Value.startingLidFrame.Value;
                    Game1.playSound(config.OpenSound);
                    Game1.player.Halt();
                    Game1.player.freezePause = 1000;
                });

                Helper.Input.Suppress(e.Button);
            }
        }

        /// <summary>Track toolbar changes before user input.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        private void OnButtonsChanged(object sender, ButtonsChangedEventArgs e)
        {
            if (HeldChest.Value == null || Game1.activeClickableMenu != null || !_config.Controls.OpenCrafting.JustPressed())
                return;

            var config = GetConfig(HeldChest.Value);
            if (!config.AccessCarried)
                return;

            HeldChest.Value.GetMutex().RequestLock(delegate
            {
                var pos = Utility.getTopLeftPositionForCenteringOnScreen(800 + IClickableMenu.borderWidth * 2, 600 + IClickableMenu.borderWidth * 2);
                Game1.activeClickableMenu = new CraftingPage(
                    (int) pos.X,
                    (int) pos.Y,
                    800 + IClickableMenu.borderWidth * 2,
                    600 + IClickableMenu.borderWidth * 2,
                    false,
                    true,
                    new List<Chest> {HeldChest.Value})
                {
                    exitFunction = delegate { HeldChest.Value.GetMutex().ReleaseLock(); }
                };
            });

            Helper.Input.SuppressActiveKeybinds(_config.Controls.OpenCrafting);
        }
    }
}