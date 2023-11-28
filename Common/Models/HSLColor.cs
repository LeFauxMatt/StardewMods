namespace StardewMods.Common.Models;

using System;
using Microsoft.Xna.Framework;

internal struct HslColor
{
    private const float Tolerance = 1 / 255f;

    // HSL stands for Hue, Saturation and Luminance. HSL
    // color space makes it easier to do calculations
    // that operate on these channels
    // Helpful color math can be found here:
    // https://www.easyrgb.com/en/math.php

    /// <summary>Initializes a new instance of the <see cref="HslColor" /> struct.</summary>
    /// <param name="h">The hue.</param>
    /// <param name="s">The saturation.</param>
    /// <param name="l">The luminance.</param>
    public HslColor(float h, float s, float l)
    {
        this.H = h;
        this.S = s;
        this.L = l;
    }

    /// <summary>
    ///     Gets or sets hue: the 'color' of the color.
    /// </summary>
    public float H { get; set; }

    /// <summary>Gets or sets luminance: The brightness or lightness of the color.</summary>
    public float L { get; set; }

    /// <summary>Gets or sets saturation: How grey or vivid/colorful a color is.</summary>
    public float S { get; set; }

    public static HslColor FromColor(Color color)
    {
        return HslColor.FromRgb(color.R, color.G, color.B);
    }

    public static HslColor FromRgb(byte r, byte g, byte b)
    {
        var hsl = default(HslColor);

        hsl.H = 0;
        hsl.S = 0;
        hsl.L = 0;

        var fR = r / 255f;
        var fG = g / 255f;
        var fB = b / 255f;
        var min = Math.Min(Math.Min(fR, fG), fB);
        var max = Math.Max(Math.Max(fR, fG), fB);
        var delta = max - min;

        // luminance is the ave of max and min
        hsl.L = (max + min) / 2f;

        if (delta <= 0)
        {
            return hsl;
        }

        if (hsl.L < 0.5f)
        {
            hsl.S = delta / (max + min);
        }
        else
        {
            hsl.S = delta / (2 - max - min);
        }

        var deltaR = (((max - fR) / 6f) + (delta / 2f)) / delta;
        var deltaG = (((max - fG) / 6f) + (delta / 2f)) / delta;
        var deltaB = (((max - fB) / 6f) + (delta / 2f)) / delta;

        if (Math.Abs(fR - max) < HslColor.Tolerance)
        {
            hsl.H = deltaB - deltaG;
        }
        else if (Math.Abs(fG - max) < HslColor.Tolerance)
        {
            hsl.H = (1f / 3f) + deltaR - deltaB;
        }
        else if (Math.Abs(fB - max) < HslColor.Tolerance)
        {
            hsl.H = (2f / 3f) + deltaG - deltaR;
        }

        if (hsl.H < 0)
        {
            hsl.H += 1;
        }

        if (hsl.H > 1)
        {
            hsl.H -= 1;
        }

        return hsl;
    }

    public HslColor GetComplement()
    {
        // complementary colors are across the color wheel
        // which is 180 degrees or 50% of the way around the
        // wheel. Add 50% to our hue and wrap large/small values
        var h = this.H + 0.5f;
        if (h > 1)
        {
            h -= 1;
        }

        return new(h, this.S, this.L);
    }

    public Color ToRgbColor()
    {
        var c = default(Color);

        if (this.S == 0)
        {
            c.R = (byte)(this.L * 255f);
            c.G = (byte)(this.L * 255f);
            c.B = (byte)(this.L * 255f);
        }
        else
        {
            var v2 = this.L + this.S - (this.S * this.L);
            if (this.L < 0.5f)
            {
                v2 = this.L * (1 + this.S);
            }

            var v1 = (2f * this.L) - v2;

            c.R = (byte)(255f * HslColor.HueToRgb(v1, v2, this.H + (1f / 3f)));
            c.G = (byte)(255f * HslColor.HueToRgb(v1, v2, this.H));
            c.B = (byte)(255f * HslColor.HueToRgb(v1, v2, this.H - (1f / 3f)));
        }

        c.A = 255;
        return c;
    }

    private static float HueToRgb(float v1, float v2, float vH)
    {
        vH += vH < 0 ? 1 : 0;
        vH -= vH > 1 ? 1 : 0;
        var ret = v1;

        if (6 * vH < 1)
        {
            ret = v1 + ((v2 - v1) * 6 * vH);
        }
        else if (2 * vH < 1)
        {
            ret = v2;
        }
        else if (3 * vH < 2)
        {
            ret = v1 + ((v2 - v1) * ((2f / 3f) - vH) * 6f);
        }

        return MathHelper.Clamp(ret, 0, 1);
    }
}