namespace XSPlus.Features
{
    using System.Collections.Generic;
    using Common.Helpers;
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
    internal class ColorPicker : BaseFeature
    {
        private const int Height = 598;
        private const int Width = 98;
        private const int Cells = 16;
        private const int Gap = 6;
        private static readonly Rectangle SelectRect = new(412, 495, 5, 4);
        private readonly IContentHelper _contentHelper;
        private readonly PerScreen<IClickableMenu> _menu = new();
        private readonly PerScreen<Chest> _chest = new();
        private readonly PerScreen<bool> _attached = new();
        private readonly PerScreen<int> _screenId = new() { Value = -1 };
        private readonly PerScreen<Hold> _holding = new();
        private readonly PerScreen<Rectangle> _area = new();
        private readonly PerScreen<ClickableTextureComponent> _transparentBox = new();
        private readonly PerScreen<Rectangle> _hueRect = new();
        private readonly PerScreen<IList<Rectangle>> _saturationBars = new() { Value = new List<Rectangle>() };
        private readonly PerScreen<IList<Rectangle>> _lightnessBars = new() { Value = new List<Rectangle>() };
        private readonly PerScreen<IList<Color>> _saturationColors = new() { Value = new List<Color>() };
        private readonly PerScreen<IList<Color>> _lightnessColors = new() { Value = new List<Color>() };
        private readonly PerScreen<float> _hue = new();
        private readonly PerScreen<float> _saturation = new();
        private readonly PerScreen<float> _lightness = new();
        private readonly PerScreen<int> _hueY = new();
        private readonly PerScreen<int> _saturationY = new();
        private readonly PerScreen<int> _lightnessY = new();
        private Texture2D _texture;
        private Color[] _colors;
        private int _totalColors;
        private int _hueBarHeight;
        private int _cellHeight;
        private int _totalHeight;

        /// <summary>Initializes a new instance of the <see cref="ColorPicker"/> class.</summary>
        /// <param name="contentHelper">Provides an API for loading content assets.</param>
        internal ColorPicker(IContentHelper contentHelper)
            : base("ColorPicker")
        {
            this._contentHelper = contentHelper;
        }

        private enum Hold
        {
            None,
            Hue,
            Saturation,
            Lightness,
            Transparent,
        }

        private float Hue
        {
            get => this._hue.Value;
            set => this._hue.Value = MathHelper.Clamp(value, 0, 1);
        }

        private float Saturation
        {
            get => this._saturation.Value;
            set => this._saturation.Value = MathHelper.Clamp(value, 0, 1);
        }

        private float Lightness
        {
            get => this._lightness.Value;
            set => this._lightness.Value = MathHelper.Clamp(value, 0.01f, 1);
        }

        private int HueY
        {
            get => this._hueY.Value;
            set => this._hueY.Value = (int)MathHelper.Clamp(value, this._area.Value.Top + 36, this._area.Value.Top + this._hueBarHeight + 36);
        }

        private int SaturationY
        {
            get => this._saturationY.Value;
            set => this._saturationY.Value = (int)MathHelper.Clamp(value, this._area.Value.Top + 36, this._area.Value.Top + this._totalHeight + 36);
        }

        private int LightnessY
        {
            get => this._lightnessY.Value;
            set => this._lightnessY.Value = (int)MathHelper.Clamp(value, this._area.Value.Top + 36 + this._totalHeight + ColorPicker.Gap, this._area.Value.Top + 36 + (this._totalHeight * 2) + ColorPicker.Gap);
        }

        /// <inheritdoc/>
        public override void Activate(IModEvents modEvents, Harmony harmony)
        {
            // Events
            CommonFeature.ItemGrabMenuChanged += this.OnItemGrabMenuChanged;
            CommonFeature.RenderedActiveMenu += this.OnRenderedActiveMenu;
            modEvents.GameLoop.GameLaunched += this.OnGameLaunched;
            modEvents.Input.ButtonPressed += this.OnButtonPressed;
            modEvents.Input.ButtonReleased += this.OnButtonReleased;
            modEvents.Input.CursorMoved += this.OnCursorMoved;
            modEvents.Input.MouseWheelScrolled += this.OnMouseWheelScrolled;
        }

        /// <inheritdoc/>
        public override void Deactivate(IModEvents modEvents, Harmony harmony)
        {
            // Events
            CommonFeature.ItemGrabMenuChanged -= this.OnItemGrabMenuChanged;
            CommonFeature.RenderedActiveMenu -= this.OnRenderedActiveMenu;
            modEvents.GameLoop.GameLaunched -= this.OnGameLaunched;
            modEvents.Input.ButtonPressed -= this.OnButtonPressed;
            modEvents.Input.ButtonReleased -= this.OnButtonReleased;
            modEvents.Input.CursorMoved -= this.OnCursorMoved;
            modEvents.Input.MouseWheelScrolled -= this.OnMouseWheelScrolled;
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            this._texture = this._contentHelper.Load<Texture2D>("assets/hue.png");
            this._totalColors = this._texture.Width * this._texture.Height;
            this._colors = new Color[this._totalColors];
            this._texture.GetData(this._colors);
            this._hueBarHeight = ColorPicker.Height + ColorPicker.Gap - IClickableMenu.borderWidth - 46;
            this._totalHeight = (this._hueBarHeight - ColorPicker.Gap) / 2;
            this._cellHeight = this._totalHeight / ColorPicker.Cells;
            this._transparentBox.Value = new ClickableTextureComponent(
                Rectangle.Empty,
                Game1.mouseCursors,
                new Rectangle(295, 503, 7, 7),
                4f);

            for (int i = 0; i < ColorPicker.Cells; i++)
            {
                this._saturationBars.Value.Add(Rectangle.Empty);
                this._lightnessBars.Value.Add(Rectangle.Empty);
                this._saturationColors.Value.Add(Color.White);
                this._lightnessColors.Value.Add(Color.White);
            }
        }

        private void OnItemGrabMenuChanged(object sender, CommonFeature.ItemGrabMenuChangedEventArgs e)
        {
            if (!e.Attached || !this.IsEnabledForItem(e.Chest))
            {
                this._screenId.Value = -1;
                this._attached.Value = false;
                return;
            }

            this._attached.Value = true;
            this._screenId.Value = e.ScreenId;
            this._holding.Value = Hold.None;
            this._chest.Value = new Chest(true, e.Chest.ParentSheetIndex)
            {
                Name = e.Chest.Name,
                lidFrameCount = { Value = e.Chest.lidFrameCount.Value },
                playerChoiceColor = { Value = e.Chest.playerChoiceColor.Value },
            };
            foreach (var modData in e.Chest.modData)
            {
                this._chest.Value.modData.CopyFrom(modData);
            }

            this._chest.Value.resetLidFrame();

            this._area.Value = new Rectangle(e.ItemGrabMenu.xPositionOnScreen + e.ItemGrabMenu.width + 96 + (IClickableMenu.borderWidth / 2), e.ItemGrabMenu.yPositionOnScreen - 56 + (IClickableMenu.borderWidth / 2), 58, ColorPicker.Height - IClickableMenu.borderWidth);
            this._hueRect.Value = new Rectangle(this._area.Value.Left, this._area.Value.Top + 36, 28, this._hueBarHeight);
            this._transparentBox.Value.bounds.X = this._area.Value.Left;
            this._transparentBox.Value.bounds.Y = this._area.Value.Top;
            this._transparentBox.Value.bounds.Width = 7;
            this._transparentBox.Value.bounds.Height = 7;
            HSLColor chestColor = HSLColor.FromColor(e.Chest.playerChoiceColor.Value);
            this.Hue = chestColor.H;
            this.Saturation = chestColor.S;
            this.Lightness = chestColor.L;
            for (int i = 0; i < ColorPicker.Cells; i++)
            {
                this._saturationBars.Value[i] = new Rectangle(this._area.Value.Left + 36, this._area.Value.Top + 36 + (i * this._cellHeight), 22, this._cellHeight);
                this._lightnessBars.Value[i] = new Rectangle(this._area.Value.Left + 36, this._area.Value.Top + 36 + (i * this._cellHeight) + this._totalHeight + ColorPicker.Gap, 22, this._cellHeight);
            }

            this.UpdateColors(false);
        }

        private void OnRenderedActiveMenu(object sender, RenderedActiveMenuEventArgs e)
        {
            if (!this._attached.Value)
            {
                return;
            }

            // Draw Chest
            // this._chest.Value.draw(e.SpriteBatch, this._area.Value.Left, this._area.Value.Top - (IClickableMenu.borderWidth / 2) - Game1.tileSize, 1f, true);
            Chest chest = this._chest.Value;
            SpriteBatch b = e.SpriteBatch;
            int x = this._area.Value.Left;
            int y = this._area.Value.Top - (IClickableMenu.borderWidth / 2) - Game1.tileSize;
            chest.draw(b, x, y, 1f, true);

            // Menu Background
            IClickableMenu.drawTextureBox(e.SpriteBatch, this._area.Value.Left - (IClickableMenu.borderWidth / 2), this._area.Value.Top - (IClickableMenu.borderWidth / 2), ColorPicker.Width, ColorPicker.Height, Color.LightGray);

            // Transparent Box
            this._transparentBox.Value.draw(e.SpriteBatch);

            // Hue Bar
            e.SpriteBatch.Draw(this._texture, this._hueRect.Value, Color.White);

            // Cells
            for (int i = 0; i < ColorPicker.Cells; i++)
            {
                // Saturation Bar
                e.SpriteBatch.Draw(Game1.staminaRect, this._saturationBars.Value[i], this._saturationColors.Value[i]);

                // Lightness Bar
                e.SpriteBatch.Draw(Game1.staminaRect, this._lightnessBars.Value[i], this._lightnessColors.Value[i]);
            }

            if (this._chest.Value.playerChoiceColor.Value == Color.Black)
            {
                IClickableMenu.drawTextureBox(
                    b: e.SpriteBatch,
                    texture: Game1.mouseCursors,
                    sourceRect: new Rectangle(375, 357, 3, 3),
                    x: this._area.Value.Left - 4,
                    y: this._area.Value.Top - 4,
                    width: 36,
                    height: 36,
                    color: Color.Black,
                    scale: 4f,
                    drawShadow: false);
            }

            // Hue Selection
            e.SpriteBatch.Draw(
                texture: Game1.mouseCursors,
                destinationRectangle: new Rectangle(this._area.Value.Left - 8, this.HueY - 1, 20, 16),
                sourceRectangle: ColorPicker.SelectRect,
                color: Color.White,
                rotation: MathHelper.PiOver2,
                origin: new Vector2(2.5f, 4f),
                effects: SpriteEffects.None,
                layerDepth: 1);

            // Saturation Selection
            e.SpriteBatch.Draw(
                texture: Game1.mouseCursors,
                destinationRectangle: new Rectangle(this._area.Value.Left + 28, this.SaturationY - 1, 20, 16),
                sourceRectangle: ColorPicker.SelectRect,
                color: Color.White,
                rotation: MathHelper.PiOver2,
                origin: new Vector2(2.5f, 4f),
                effects: SpriteEffects.None,
                layerDepth: 1);

            // Lightness Selection
            e.SpriteBatch.Draw(
                texture: Game1.mouseCursors,
                destinationRectangle: new Rectangle(this._area.Value.Left + 28, this.LightnessY - 1, 20, 16),
                sourceRectangle: ColorPicker.SelectRect,
                color: Color.White,
                rotation: MathHelper.PiOver2,
                origin: new Vector2(2.5f, 4f),
                effects: SpriteEffects.None,
                layerDepth: 1);
        }

        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!this._attached.Value || this._screenId.Value != Context.ScreenId || e.Button != SButton.MouseLeft || this._holding.Value != Hold.None || Game1.activeClickableMenu is not ItemGrabMenu)
            {
                return;
            }

            Point point = Game1.getMousePosition(true);
            if (!this._area.Value.Contains(point.X, point.Y))
            {
                return;
            }

            int relativeY = point.Y - this._area.Value.Top - 36;
            if (relativeY <= 0 && point.X <= this._area.Value.Center.X)
            {
                this._holding.Value = Hold.Transparent;
                this.HueY = 0;
                this.SaturationY = 0;
                this.LightnessY = 0;
                this._chest.Value.playerChoiceColor.Value = Color.Black;
                Game1.playSound("coin");
                this.UpdateColors(false);
                return;
            }

            if (point.X <= this._area.Value.Center.X)
            {
                // Hue Selection
                this._holding.Value = Hold.Hue;
                this.HueY = point.Y;
                Game1.playSound("coin");
            }
            else if (relativeY <= this._totalHeight)
            {
                // Saturation
                this._holding.Value = Hold.Saturation;
                this.SaturationY = point.Y;
                Game1.playSound("coin");
            }
            else if (relativeY >= this._totalHeight + ColorPicker.Gap)
            {
                // Lightness
                this._holding.Value = Hold.Lightness;
                this.LightnessY = point.Y;
                Game1.playSound("coin");
            }

            this.UpdateColors();
        }

        private void OnButtonReleased(object sender, ButtonReleasedEventArgs e)
        {
            if (!this._attached.Value || this._screenId.Value != Context.ScreenId || e.Button != SButton.MouseLeft || this._holding.Value == Hold.None || Game1.activeClickableMenu is not ItemGrabMenu { context: Chest chest })
            {
                return;
            }

            this._holding.Value = Hold.None;
            chest.playerChoiceColor.Value = this._chest.Value.playerChoiceColor.Value;
        }

        private void OnCursorMoved(object sender, CursorMovedEventArgs e)
        {
            if (!this._attached.Value || this._screenId.Value != Context.ScreenId || this._holding.Value is Hold.None or Hold.Transparent || Game1.activeClickableMenu is not ItemGrabMenu)
            {
                return;
            }

            Point point = Game1.getMousePosition(true);
            switch (this._holding.Value)
            {
                case Hold.Hue:
                    // Hue Selection
                    this.HueY = point.Y;
                    break;

                case Hold.Saturation:
                    // Saturation
                    this.SaturationY = point.Y;
                    break;

                case Hold.Lightness:
                    // Lightness
                    this.LightnessY = point.Y;
                    break;
            }

            this.UpdateColors();
        }

        private void OnMouseWheelScrolled(object sender, MouseWheelScrolledEventArgs e)
        {
            if (!this._attached.Value || this._screenId.Value != Context.ScreenId || Game1.activeClickableMenu is not ItemGrabMenu { context: Chest chest })
            {
                return;
            }

            Point point = Game1.getMousePosition(true);
            if (!this._area.Value.Contains(point.X, point.Y))
            {
                return;
            }

            int relativeY = point.Y - this._area.Value.Top - 36;
            if (relativeY <= 0)
            {
                return;
            }

            int delta = e.Delta > 0 ? -10 : 10;
            if (point.X <= this._area.Value.Center.X)
            {
                // Hue Selection
                this.HueY += delta;
            }
            else if (relativeY <= this._totalHeight)
            {
                // Saturation
                this.SaturationY += delta;
            }
            else if (relativeY >= this._totalHeight + ColorPicker.Gap)
            {
                // Lightness
                this.LightnessY += delta;
            }

            this.UpdateColors();
            chest.playerChoiceColor.Value = this._chest.Value.playerChoiceColor.Value;
        }

        private void UpdateColors(bool updateChest = true)
        {
            switch (this._holding.Value)
            {
                case Hold.Hue:
                    int hue = (int)MathHelper.Clamp((this.HueY - this._area.Value.Top - 36f) / this._hueBarHeight * this._totalColors, 1, this._totalColors);
                    HSLColor chestColor = HSLColor.FromColor(this._colors[hue - 1]);
                    this.Hue = chestColor.H;
                    if (chestColor.H != 0 && this._chest.Value.playerChoiceColor.Value == Color.Black)
                    {
                        this.Saturation = chestColor.S;
                        this.Lightness = chestColor.L;
                        this.SaturationY = (int)(this._area.Value.Top + 36 + (chestColor.S * this._totalHeight));
                        this.LightnessY = (int)(this._area.Value.Top + 36 + this._totalHeight + ColorPicker.Gap + (chestColor.L * this._totalHeight));
                    }

                    break;
                case Hold.Saturation:
                    this.Saturation = (this.SaturationY - this._area.Value.Top - 72) / (float)this._totalHeight;
                    break;
                case Hold.Lightness:
                    this.Lightness = (this.LightnessY - this._area.Value.Top - 72 - this._totalHeight - ColorPicker.Gap) / (float)this._totalHeight;
                    break;
                case Hold.Transparent:
                    this.Hue = 0;
                    this.Saturation = 0;
                    this.Lightness = 0;
                    break;
            }

            for (int i = 0; i < ColorPicker.Cells; i++)
            {
                this._saturationColors.Value[i] = new HSLColor { H = this.Hue, S = i / 16f, L = this.Lightness }.ToRgbColor();
                this._lightnessColors.Value[i] = new HSLColor { H = this.Hue, S = this.Saturation, L = i / 16f }.ToRgbColor();
            }

            if (updateChest)
            {
                this._chest.Value.playerChoiceColor.Value = new HSLColor { H = this.Hue, S = this.Saturation, L = this.Lightness }.ToRgbColor();
            }
        }
    }
}