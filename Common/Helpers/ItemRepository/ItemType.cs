#pragma warning disable

#region License

// MIT License
//
// Copyright (c) 2018 CJBok
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
//     of this software and associated documentation files (the "Software"), to deal
//     in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
//     furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
//     copies or substantial portions of the Software.
//
//     THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//     IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//     FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//     AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//     LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

#endregion

#region README

// This implementation of ItemType was derived from
// https://github.com/CJBok/SDV-Mods/tree/master/CJBItemSpawner

#endregion

namespace StardewMods.Common.Helpers.ItemRepository;

/// <summary>An item type that can be searched and added to the player through the console.</summary>
internal enum ItemType
{
    /// <summary>A big craftable object in <see cref="StardewValley.Game1.bigCraftablesInformation" /></summary>
    BigCraftable,

    /// <summary>A <see cref="StardewValley.Objects.Boots" /> item.</summary>
    Boots,

    /// <summary>A <see cref="StardewValley.Objects.Clothing" /> item.</summary>
    Clothing,

    /// <summary>A <see cref="StardewValley.Objects.Wallpaper" /> flooring item.</summary>
    Flooring,

    /// <summary>A <see cref="StardewValley.Objects.Furniture" /> item.</summary>
    Furniture,

    /// <summary>A <see cref="StardewValley.Objects.Hat" /> item.</summary>
    Hat,

    /// <summary>Any object in <see cref="StardewValley.Game1.objectInformation" /> (except rings).</summary>
    Object,

    /// <summary>A <see cref="StardewValley.Objects.Ring" /> item.</summary>
    Ring,

    /// <summary>A <see cref="StardewValley.Tool" /> tool.</summary>
    Tool,

    /// <summary>A <see cref="StardewValley.Objects.Wallpaper" /> wall item.</summary>
    Wallpaper,

    /// <summary>A <see cref="StardewValley.Tools.MeleeWeapon" /> or <see cref="StardewValley.Tools.Slingshot" /> item.</summary>
    Weapon,
}
