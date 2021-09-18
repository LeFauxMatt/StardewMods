namespace XSPlus.Features
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Common.Extensions;
    using HarmonyLib;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using StardewModdingAPI;
    using StardewModdingAPI.Events;
    using StardewModdingAPI.Utilities;
    using StardewValley;
    using StardewValley.Menus;
    using StardewValley.Objects;

    /// <inheritdoc />
    internal class InventoryTabs : FeatureWithParam<HashSet<string>>
    {
        private static readonly Type[] ItemGrabMenuConstructorParams = { typeof(IList<Item>), typeof(bool), typeof(bool), typeof(InventoryMenu.highlightThisItem), typeof(ItemGrabMenu.behaviorOnItemSelect), typeof(string), typeof(ItemGrabMenu.behaviorOnItemSelect), typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(bool), typeof(int), typeof(Item), typeof(int), typeof(object) };
        private static InventoryTabs Instance;
        private readonly IContentHelper ContentHelper;
        private readonly IInputHelper InputHelper;
        private readonly Func<KeybindList> GetPreviousTab;
        private readonly Func<KeybindList> GetNextTab;
        private readonly PerScreen<IClickableMenu> Menu = new();
        private readonly PerScreen<Chest> Chest = new();
        private readonly PerScreen<bool> Attached = new();
        private readonly PerScreen<int> TabIndex = new() { Value = -1 };
        private readonly PerScreen<int> ScreenId = new() { Value = -1 };
        private IList<Tab> Tabs;
        private Texture2D Texture;

        /// <summary>Initializes a new instance of the <see cref="InventoryTabs"/> class.</summary>
        /// <param name="contentHelper">Provides an API for loading content assets.</param>
        /// <param name="inputHelper">Provides an API for checking and changing input state.</param>
        /// <param name="getPreviousTab">Get method for configured previous tab button.</param>
        /// <param name="getNextTab">Get method for configured next tab button.</param>
        public InventoryTabs(IContentHelper contentHelper, IInputHelper inputHelper, Func<KeybindList> getPreviousTab, Func<KeybindList> getNextTab)
            : base("InventoryTabs")
        {
            InventoryTabs.Instance = this;
            this.ContentHelper = contentHelper;
            this.InputHelper = inputHelper;
            this.GetPreviousTab = getPreviousTab;
            this.GetNextTab = getNextTab;
        }

        /// <inheritdoc/>
        public override void Activate(IModEvents modEvents, Harmony harmony)
        {
            // Events
            modEvents.GameLoop.GameLaunched += this.OnGameLaunched;
            modEvents.Display.MenuChanged += this.OnMenuChanged;
            modEvents.Display.RenderingActiveMenu += this.OnRenderingActiveMenu;
            modEvents.Input.ButtonsChanged += this.OnButtonsChanged;
            modEvents.Input.ButtonPressed += this.OnButtonPressed;

            // Patches
            harmony.Patch(
                original: AccessTools.Constructor(typeof(ItemGrabMenu), InventoryTabs.ItemGrabMenuConstructorParams),
                postfix: new HarmonyMethod(typeof(InventoryTabs), nameof(InventoryTabs.ItemGrabMenu_constructor_postfix)));
        }

        /// <inheritdoc/>
        public override void Deactivate(IModEvents modEvents, Harmony harmony)
        {
            // Events
            modEvents.GameLoop.GameLaunched -= this.OnGameLaunched;
            modEvents.Display.MenuChanged -= this.OnMenuChanged;
            modEvents.Display.RenderingActiveMenu -= this.OnRenderingActiveMenu;
            modEvents.Input.ButtonsChanged -= this.OnButtonsChanged;
            modEvents.Input.ButtonPressed -= this.OnButtonPressed;

            // Patches
            harmony.Unpatch(
                original: AccessTools.Constructor(typeof(ItemGrabMenu), InventoryTabs.ItemGrabMenuConstructorParams),
                patch: AccessTools.Method(typeof(InventoryTabs), nameof(InventoryTabs.ItemGrabMenu_constructor_postfix)));
        }

        /// <summary>Remove background to render chest tabs under menu.</summary>
        [SuppressMessage("ReSharper", "SA1313", Justification = "Naming is determined by Harmony.")]
        [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Naming is determined by Harmony.")]
        [SuppressMessage("ReSharper", "SuggestBaseTypeForParameter", Justification = "Type is determined by Harmony.")]
        private static void ItemGrabMenu_constructor_postfix(ItemGrabMenu __instance)
        {
            if (__instance.context is not Chest chest || !InventoryTabs.Instance.IsEnabledForItem(chest))
            {
                return;
            }

            __instance.setBackgroundTransparency(false);
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            this.Tabs = this.ContentHelper.Load<List<Tab>>("assets/tabs.json");
            this.Texture = this.ContentHelper.Load<Texture2D>("assets/tabs.png");

            for (int i = 0; i < this.Tabs.Count; i++)
            {
                this.Tabs[i].Component = new ClickableTextureComponent(
                    bounds: new Rectangle(0, 0, 16 * Game1.pixelZoom, 16 * Game1.pixelZoom),
                    texture: this.Texture,
                    sourceRect: new Rectangle(16 * i, 0, 16, 16),
                    scale: Game1.pixelZoom);
            }
        }

        private void OnMenuChanged(object sender, MenuChangedEventArgs e)
        {
            if (ReferenceEquals(e.NewMenu, this.Menu.Value))
            {
                return;
            }

            this.Menu.Value = e.NewMenu;
            if (e.NewMenu is not ItemGrabMenu { shippingBin: false, context: Chest chest } || !this.IsEnabledForItem(chest))
            {
                CommonFeature.HighlightChestItems -= this.HighlightMethod;
                this.Attached.Value = false;
                this.ScreenId.Value = -1;
                return;
            }

            if (!this.Attached.Value)
            {
                CommonFeature.HighlightChestItems += this.HighlightMethod;
                this.Attached.Value = true;
                this.ScreenId.Value = Context.ScreenId;
            }

            if (!ReferenceEquals(this.Chest.Value, chest))
            {
                this.Chest.Value = chest;
                this.TabIndex.Value = -1;
            }
        }

        private void OnRenderingActiveMenu(object sender, RenderingActiveMenuEventArgs e)
        {
            if (!this.Attached.Value || this.ScreenId.Value != Context.ScreenId || Game1.activeClickableMenu is not ItemGrabMenu itemGrabMenu)
            {
                return;
            }

            // Draw background behind tabs
            e.SpriteBatch.Draw(Game1.fadeToBlackRect, new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height), Color.Black * 0.5f);

            // Draw tabs between inventory menus along a horizontal axis
            int x = itemGrabMenu.ItemsToGrabMenu.xPositionOnScreen;
            int y = itemGrabMenu.ItemsToGrabMenu.yPositionOnScreen + itemGrabMenu.ItemsToGrabMenu.height + (1 * Game1.pixelZoom);
            for (int i = 0; i < this.Tabs.Count; i++)
            {
                ClickableTextureComponent cc = this.Tabs[i].Component;
                Color color;
                cc.bounds.X = x;
                if (i == this.TabIndex.Value)
                {
                    cc.bounds.Y = y + (1 * Game1.pixelZoom);
                    color = Color.White;
                }
                else
                {
                    cc.bounds.Y = y;
                    color = Color.Gray;
                }

                cc.draw(e.SpriteBatch, color, 0.86f + (cc.bounds.Y / 20000f));
                x = cc.bounds.Right;
            }
        }

        private void OnButtonsChanged(object sender, ButtonsChangedEventArgs e)
        {
            if (!this.Attached.Value)
            {
                return;
            }

            KeybindList nextTabButton = this.GetNextTab();
            if (nextTabButton.JustPressed())
            {
                this.TabIndex.Value++;
                if (this.TabIndex.Value == this.Tabs.Count)
                {
                    this.TabIndex.Value = -1;
                }

                this.InputHelper.SuppressActiveKeybinds(nextTabButton);
                return;
            }

            KeybindList previousTabButton = this.GetPreviousTab();
            if (previousTabButton.JustPressed())
            {
                this.TabIndex.Value--;
                if (this.TabIndex.Value == -2)
                {
                    this.TabIndex.Value = this.Tabs.Count - 1;
                }

                this.InputHelper.SuppressActiveKeybinds(previousTabButton);
            }
        }

        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!this.Attached.Value || this.ScreenId.Value != Context.ScreenId || e.Button != SButton.MouseLeft)
            {
                return;
            }

            // Check if any tab was clicked on.
            Point point = Game1.getMousePosition(true);
            Tab tab = this.Tabs.FirstOrDefault(tab => tab.Component.containsPoint(point.X, point.Y));
            if (tab is null)
            {
                return;
            }

            // Toggle if currently active was clicked on.
            int index = this.Tabs.IndexOf(tab);
            this.TabIndex.Value = this.TabIndex.Value == index ? -1 : index;
            this.InputHelper.Suppress(e.Button);
        }

        private bool HighlightMethod(Item item)
        {
            return this.TabIndex.Value == -1 || item.MatchesTagExt(this.Tabs[this.TabIndex.Value].Tags);
        }

        [SuppressMessage("ReSharper", "ClassNeverInstantiated.Local", Justification = "Record is instantiated by ContentHelper.")]
        private record Tab
        {
            public string Name { get; set; }

            public string[] Tags { get; set; }

            public ClickableTextureComponent Component { get; set; }
        }
    }
}