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
// This implementation of SearchableItem was derived from
// https://github.com/CJBok/SDV-Mods/tree/master/CJBItemSpawner
#endregion

using System;
using StardewValley;

// ReSharper disable All
namespace Common.Helpers.ItemData
{
    /// <summary>A game item with metadata.</summary>
    /// <remarks>This is copied from the SMAPI source code and should be kept in sync with it.</remarks>
    internal class SearchableItem
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="type">The item type.</param>
        /// <param name="id">The unique ID (if different from the item's parent sheet index).</param>
        /// <param name="createItem">Create an item instance.</param>
        public SearchableItem(ItemType type, int id, Func<SearchableItem, Item> createItem)
        {
            this.Type = type;
            this.ID = id;
            this.CreateItem = () => createItem(this);
            this.Item = createItem(this);
        }

        /// <summary>Construct an instance.</summary>
        /// <param name="item">The item metadata to copy.</param>
        public SearchableItem(SearchableItem item)
        {
            this.Type = item.Type;
            this.ID = item.ID;
            this.CreateItem = item.CreateItem;
            this.Item = item.Item;
        }

        /*********
        ** Accessors
        *********/
        /// <summary>The item type.</summary>
        public ItemType Type { get; }

        /// <summary>A sample item instance.</summary>
        public Item Item { get; }

        /// <summary>Create an item instance.</summary>
        public Func<Item> CreateItem { get; }

        /// <summary>The item's unique ID for its type.</summary>
        public int ID { get; }

        /// <summary>The item's default name.</summary>
        public string Name => this.Item.Name;

        /// <summary>The item's display name for the current language.</summary>
        public string DisplayName => this.Item.DisplayName;

        /// <summary>Get whether the item name contains a case-insensitive substring.</summary>
        /// <param name="substring">The substring to find.</param>
        public bool NameContains(string substring)
        {
            return
                this.Name.IndexOf(substring, StringComparison.OrdinalIgnoreCase) != -1
                || this.DisplayName.IndexOf(substring, StringComparison.OrdinalIgnoreCase) != -1;
        }

        /// <summary>Get whether the item name is exactly equal to a case-insensitive string.</summary>
        /// <param name="name">The substring to find.</param>
        public bool NameEquivalentTo(string name)
        {
            return
                this.Name.Equals(name, StringComparison.OrdinalIgnoreCase)
                || this.DisplayName.Equals(name, StringComparison.OrdinalIgnoreCase);
        }
    }
}