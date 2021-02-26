using Harmony;
using ImJustMatt.Common.PatternPatches;
using ImJustMatt.ExpandedStorage.Framework.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;

namespace ImJustMatt.ExpandedStorage.Framework.Patches
{
    internal class DiscreteColorPickerPatch : Patch<ModConfig>
    {
        private static Texture2D _hueBar;
        private static Color[] _colors;
        private static HSLColor _color;

        public DiscreteColorPickerPatch(IMonitor monitor, ModConfig config, IContentHelper contentHelper)
            : base(monitor, config)
        {
            _hueBar = contentHelper.Load<Texture2D>("assets/Hue.png");
            _colors = new Color[_hueBar.Width * _hueBar.Height];
            _hueBar.GetData(_colors);
        }

        protected internal override void Apply(HarmonyInstance harmony)
        {
            harmony.Patch(
                AccessTools.Constructor(typeof(DiscreteColorPicker), new[] {typeof(int), typeof(int), typeof(int), typeof(Item)}),
                postfix: new HarmonyMethod(GetType(), nameof(ConstructorPostfix))
            );

            harmony.Patch(
                AccessTools.Method(typeof(DiscreteColorPicker), nameof(DiscreteColorPicker.receiveLeftClick)),
                new HarmonyMethod(GetType(), nameof(ReceiveLeftClickPrefix))
            );

            harmony.Patch(
                AccessTools.Method(typeof(DiscreteColorPicker), nameof(DiscreteColorPicker.getSelectionFromColor)),
                new HarmonyMethod(GetType(), nameof(GetSelectionFromColorPrefix))
            );

            harmony.Patch(
                AccessTools.Method(typeof(DiscreteColorPicker), nameof(DiscreteColorPicker.getColorFromSelection)),
                new HarmonyMethod(GetType(), nameof(GetColorFromSelectionPrefix))
            );

            harmony.Patch(
                AccessTools.Method(typeof(DiscreteColorPicker), nameof(DiscreteColorPicker.draw), new[] {typeof(SpriteBatch)}),
                new HarmonyMethod(GetType(), nameof(DrawPrefix))
            );
        }

        public static void ConstructorPostfix(DiscreteColorPicker __instance)
        {
            //__instance.yPositionOnScreen -= 28;
            __instance.yPositionOnScreen -= 64;
            __instance.height += 28;
            __instance.totalColors = _hueBar.Width + 1;
        }

        public static bool ReceiveLeftClickPrefix(DiscreteColorPicker __instance, int x, int y, bool playSound)
        {
            if (__instance.visible)
                return false;

            var area = new Rectangle(__instance.xPositionOnScreen + IClickableMenu.borderWidth / 2, __instance.yPositionOnScreen + IClickableMenu.borderWidth / 2, 748, 52);
            if (!area.Contains(x, y))
                return true;

            Game1.playSound("coin");

            var selection = x - area.X;
            if (selection <= 36)
            {
                __instance.colorSelection = 0;
                _color.S = 0;
                _color.L = 0;
            }
            else if (y >= area.Y + 32)
            {
                // Hue Selection
                selection = (int) ((selection - 36) / 712f * __instance.totalColors);
                var color = HSLColor.FromColor(_colors[selection]);
                if (__instance.colorSelection == 0)
                    _color = color;
                else
                    _color.H = color.H;
                __instance.colorSelection = selection;
            }
            else
                switch (selection)
                {
                    case <= 388:
                        // Saturation
                        _color.S = (selection - 36) / 352f;
                        break;
                    case >= 396:
                        // Lightness
                        _color.L = (selection - 396) / 352f;
                        break;
                    default:
                        return false;
                }

            if (__instance.itemToDrawColored is not Chest chest)
                return false;

            chest.playerChoiceColor.Value = _color.ToRgbColor();
            chest.playerChoiceColor.A = 255;
            chest.resetLidFrame();
            return false;
        }

        public static bool GetSelectionFromColorPrefix(DiscreteColorPicker __instance, ref int __result, Color c)
        {
            _color = HSLColor.FromColor(c);
            __result = c.Equals(Color.Black)
                ? 0
                : (int) (_color.H * _hueBar.Width);
            return false;
        }

        public static bool GetColorFromSelectionPrefix(DiscreteColorPicker __instance, ref Color __result, int selection)
        {
            __result = _color.ToRgbColor();
            __result.A = 255;
            return false;
        }

        public static bool DrawPrefix(DiscreteColorPicker __instance, SpriteBatch b)
        {
            if (__instance.visible)
                return false;

            // Background
            IClickableMenu.drawTextureBox(b, __instance.xPositionOnScreen, __instance.yPositionOnScreen, __instance.width, __instance.height, Color.LightGray);

            // Transparent Square
            b.Draw(Game1.mouseCursors,
                new Vector2(__instance.xPositionOnScreen + IClickableMenu.borderWidth / 2, __instance.yPositionOnScreen + IClickableMenu.borderWidth / 2),
                new Rectangle(295, 503, 7, 7),
                Color.White,
                0f,
                Vector2.Zero,
                4f,
                SpriteEffects.None,
                0.88f);

            // Saturation Bar
            for (var i = 0; i < 16; i++)
            {
                b.Draw(Game1.staminaRect,
                    new Rectangle(__instance.xPositionOnScreen + IClickableMenu.borderWidth / 2 + 36 + i * 22, __instance.yPositionOnScreen + IClickableMenu.borderWidth / 2, 22, 28),
                    new HSLColor {H = _color.H, S = i / 16f, L = _color.L}.ToRgbColor());
            }

            if (__instance.colorSelection != 0)
                b.Draw(Game1.mouseCursors,
                    new Rectangle(__instance.xPositionOnScreen + IClickableMenu.borderWidth / 2 + 36 - 10 + (int) (_color.S * 352), __instance.yPositionOnScreen + IClickableMenu.borderWidth / 2 + 16, 20, 16),
                    new Rectangle(412, 495, 5, 4),
                    Color.White);

            // Lightness Bar
            for (var i = 0; i < 16; i++)
            {
                b.Draw(Game1.staminaRect,
                    new Rectangle(__instance.xPositionOnScreen + IClickableMenu.borderWidth / 2 + 396 + i * 22, __instance.yPositionOnScreen + IClickableMenu.borderWidth / 2, 22, 28),
                    new HSLColor {H = _color.H, S = _color.S, L = i / 16f}.ToRgbColor());
            }

            if (__instance.colorSelection != 0)
                b.Draw(Game1.mouseCursors,
                    new Rectangle(__instance.xPositionOnScreen + IClickableMenu.borderWidth / 2 + 396 - 10 + (int) (_color.L * 352), __instance.yPositionOnScreen + IClickableMenu.borderWidth / 2 + 16, 20, 16),
                    new Rectangle(412, 495, 5, 4),
                    Color.White);

            // Hue Bar
            b.Draw(_hueBar,
                new Rectangle(__instance.xPositionOnScreen + IClickableMenu.borderWidth / 2 + 36, __instance.yPositionOnScreen + IClickableMenu.borderWidth / 2 + 32, 712, 28),
                Color.White);

            if (__instance.colorSelection != 0)
                b.Draw(Game1.mouseCursors,
                    new Rectangle(__instance.xPositionOnScreen + IClickableMenu.borderWidth / 2 + 36 - 10 + (int) (_color.H * 712), __instance.yPositionOnScreen + IClickableMenu.borderWidth / 2 + 32 + 16, 20, 16),
                    new Rectangle(412, 495, 5, 4),
                    Color.White);
            else
                IClickableMenu.drawTextureBox(b,
                    Game1.mouseCursors,
                    new Rectangle(375, 357, 3, 3),
                    __instance.xPositionOnScreen + IClickableMenu.borderWidth / 2 - 4,
                    __instance.yPositionOnScreen + IClickableMenu.borderWidth / 2 - 4,
                    36,
                    36,
                    Color.Black,
                    4f,
                    false);

            // Chest
            if (__instance.itemToDrawColored is Chest chest)
                chest.draw(b, __instance.xPositionOnScreen + __instance.width + IClickableMenu.borderWidth / 2, __instance.yPositionOnScreen + 16, 1f, true);

            return false;
        }
    }
}