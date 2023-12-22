# Better Chests

Adds enhanced chest features to the game.

## Contents

* [Features](#features)
    * [Auto Organize](#auto-organize)
    * [Better Color Picker](#better-color-picker)
    * [Carry Chest](#carry-chest)
    * [Chest Finder](#chest-finder)
    * [Chest Info](#chest-info)
    * [Chest Menu Tabs](#chest-menu-tabs)
    * [Collect Items](#collect-items)
    * [Configurator](#configurator)
    * [Craft From Chest](#craft-from-chest)
    * [Craft From Workbench](#craft-from-workbench)
    * [Filter Items](#filter-items)
    * [Label Chest](#label-chest)
    * [Open Held Chest](#open-held-chest)
    * [Organize Chest](#organize-chest)
    * [Resize Chest](#resize-chest)
    * [Search Items](#search-items)
    * [Slot Lock](#slot-lock)
    * [Stash To Chest](#stash-to-chest)
    * [Transfer Items](#transfer-items)
    * [Unload Chest](#unload-chest)
* [Usage](#usage)
    * [Item Tags](#item-tags)
* [Configurations](#configurations)
    * [Config Inheritance](#config-inheritance)
    * [Group By Values](#group-by-values)
    * [In Game Menus](#in-game-menus)
    * [Option Values](#option-values)
    * [Range Values](#range-values)
    * [Slot Lock Colors](#slot-lock-colors)
    * [Sort By Values](#sort-by-values)
* [Mod Integrations](#mod-integrations)
    * [Automate](#automate)
    * [Better Crafting](#better-crafting)
    * [Horse Overhaul](#horse-overhaul)
* [Customization](#customization)
    * [Customized Tabs](#customized-tabs)
    * [Other Assets](#other-assets)
* [Translations](#translations)

## Features

### Auto Organize

Every night, after going to bed, your chests will automatically organize items
based on the same rules
as [Stash to Chest](#stash-to-chest). Items will only move from/to chests with
the feature enabled, and only if the
other chest has a higher priority.

It also applies your [Organize Chest](#organize-chest) rules at the end.

| Config       | Description                         | Default Value | Other Value(s)                        |
|:-------------|:------------------------------------|:--------------|:--------------------------------------|
| AutoOrganize | Enables the  Auto Organize feature. | `"Enabled"`   | `"Disabled"`, `"Default"`<sup>1</sup> |

### Better Color Picker

Replaces the Chest Color Picker with a more precise version that lets you pick a
color with sliders for hue, saturation,
and lightness.

| Config                | Description                                   | Default Value | Other Value(s)                        |
|:----------------------|:----------------------------------------------|:--------------|:--------------------------------------|
| CustomColorPicker     | Enables the Custom Color Picker feature.      | `"Enabled"`   | `"Disabled"`, `"Default"`<sup>1</sup> |
| CustomColorPickerArea | Which side will the Color Picker be added to. | `"Right"`     | `"Left"`                              |

1. See [Option Values](#option-values).

### Carry Chest

With Carry Chest enabled, you can hit the Use Tool button to pick up chests into
your inventory even if it has items.

| Config               | Description                             | Default Value | Other Value(s)                                                       |
|:---------------------|:----------------------------------------|:--------------|:---------------------------------------------------------------------|
| CarryChest           | Enables the Carry Chest feature.        | `"Enabled"`   | `"Disabled"`, `"Default"`<sup>1</sup>                                |
| CarryChestLimit      | Limits how many chests can be carried.  | `0`           | Any positive integer.<sup>2</sup>                                    |
| CarryChestSlow       | Enables the Carry Chest Slow feature.   | `Enabled`     | `"Disabled"`, `"Default"`<sup>1</sup>                                |
| CarryChestSlowAmount | Slowness debuff while carrying a chest. | `0`           | Positive integer from `0` (normal speed) to `4` (slime speed debuff) |

1. See [Option Values](#option-values).
2. Use `0` for unlimited chests.

### Chest Finder

Search for which chest(s) have the item you're looking for.

| Config           | Description                                     | Default Value                         | Other Value(s)                     |
|:-----------------|:------------------------------------------------|---------------------------------------|:-----------------------------------|
| ChestFinder      | Enables the Chest Finder feature.               | `true`                                | `true', 'false'                    |
| CloseChestFinder | Assigns a keybind to close the chest finder.    | `"Escape"`                            | Any valid button code.<sup>1</sup> |
| FindChest        | Assigns a keybind to find a chest.              | `"LeftControl + F, RightControl + F"` | Any valid button code.<sup>1</sup> |
| OpenFoundChest   | Assigns a keybind to open a found chest.        | `"Enter"`                             | Any valid button code.<sup>1</sup> |
| OpenNextChest    | Assigns a keybind to open the next found chest. | `"Tab"`                               | Any valid button code.<sup>1</sup> |

1.

See [Button Codes](https://stardewvalleywiki.com/Modding:Player_Guide/Key_Bindings#Button_codes).

### Chest Info

Show stats to the left of the currently opened chest.

| Config     | Description                                | Default Value | Other Value(s)                        |
|:-----------|:-------------------------------------------|---------------|:--------------------------------------|
| ChestInfo  | Enables the Chest Info feature.            | `"Disabled"`  | `"Disabled"`, `"Default"`<sup>1</sup> |
| ToggleInfo | Assigns a keybind to show/hide chest info. | `"F1"`        | Any valid button code.<sup>2</sup>    |

1. See [Option Values](#option-values).
2.

See [Button Codes](https://stardewvalleywiki.com/Modding:Player_Guide/Key_Bindings#Button_codes).

### Chest Menu Tabs

Tabs are added to the Chest Menu to allow you to quickly toggle between
categories of items.<sup>1</sup>

| Config          | Description                                    | Default Value    | Other Value(s)                        |
|:----------------|:-----------------------------------------------|------------------|:--------------------------------------|
| ChestMenuTabs   | Enables the Chest Menu Tabs feature.           | `"Enabled"`      | `"Disabled"`, `"Default"`<sup>2</sup> |
| ChestMenuTabSet | Assigns what tabs to show/hide.                | `[]`<sup>3</sup> | The names of tabs to show.            |
| NextTab         | Assigns a keybind to move to the next tab.     | `"DPadRight"`    | Any valid button code.<sup>4</sup>    |
| PreviousTab     | Assigns a keybind to move to the previous tab. | `"DPadLeft"`     | Any valid button code.<sup>4</sup>    |

1. See [Customized Tabs](#customized-tabs).
2. See [Option Values](#option-values).
3. An empty string array shows all available tabs.
4.

See [Button Codes](https://stardewvalleywiki.com/Modding:Player_Guide/Key_Bindings#Button_codes).

### Collect Items

While you are carrying a chest in your inventory, any items you pick up may be
collected directly into the chest,
bypassing your inventory.<sup>1</sup>

| Config       | Description                        | Default Value | Other Value(s)                        |
|:-------------|:-----------------------------------|:--------------|:--------------------------------------|
| CollectItems | Enables the Collect Items feature. | `"Enabled"`   | `"Disabled"`, `"Default"`<sup>1</sup> |

1. See [Option Values](#option-values).

### Configurator

Configure chests individually using a hotkey, configuration tool, or toolbar
icon.

| Config        | Description                                   | Default Value | Other Value(s)                                                  |
|:--------------|:----------------------------------------------|:--------------|:----------------------------------------------------------------|
| Configurator  | Enables the Configurator feature.             | `true`        | `true`, `false`                                                 |
| Configure     | Assigns the keybind for configuring a chest.  | `"End"`       | Any valid button code.<sup>1</sup>                              |
| ConfigureMenu | The type of config menu to open from a chest. | `"Simple"`    | `"Default"`, `"Categorize"`, `"Full"`, `"Advanced"`<sup>2</sup> |

1.

See [Button Codes](https://stardewvalleywiki.com/Modding:Player_Guide/Key_Bindings#Button_codes).

2. See [In Game Menus](#in-game-menus).

### Craft From Chest

Hit a configurable key to bring up a crafting menu that uses items stored in
nearby chests.

| Config                         | Description                                               | Default Value | Other Value(s)                                                                 |
|:-------------------------------|:----------------------------------------------------------|:--------------|:-------------------------------------------------------------------------------|
| CraftFromChest                 | Enables the Craft From Chest feature.                     | `"Location"`  | `"Disabled"`, `"Default"`, `"Inventory"`, `"Location"`, `"World"` <sup>1</sup> |
| OpenCrafting                   | Assigns the keybind for opening the crafting menu.        | `"K"`         | Any valid button code.<sup>2</sup>                                             |
| CraftFromChestDisableLocations | A list of locations that crafting will not be allowed in. | `[]`          | The locations to block.<sup>3</sup>                                            |
| CraftFromChestDistance         | Limits the distance that a chest can be crafted from.     | -1            | Any positive integer or `-1`.<sup>4</sup>                                      |

1. See [Range Values](#range-values).
2.

See [Button Codes](https://stardewvalleywiki.com/Modding:Player_Guide/Key_Bindings#Button_codes).

3. Add `"UndergroundMine"` to the list to disable in Mine and Skull Cavern.
4. Measured in tiles away from the player. Use `-1` for "unlimited" distance.

### Craft From Workbench

Allows the Workbench to craft from chests within a configurable range.

| Config                     | Description                                                                          | Default Value | Other Value(s)                                                                 |
|:---------------------------|:-------------------------------------------------------------------------------------|:--------------|:-------------------------------------------------------------------------------|
| CraftFromWorkbench         | Enables the Craft From Workbench feature.                                            | `"Location"`  | `"Disabled"`, `"Default"`, `"Inventory"`, `"Location"`, `"World"` <sup>1</sup> |
| CraftFromWorkbenchDistance | Limits the distance that a chest can be crafted from when accessed from a workbench. | -1            | Any positive integer or `-1`.<sup>2</sup>                                      |

1. See [Range Values](#range-values).
2. Measured in tiles away from the player. Use `-1` for "unlimited" distance.

### Filter Items

Impose restrictions on what types of items are allowed to go into a chest. With
this enabled, items that are not part of
the allowed list will be blocked.

| Config          | Description                                          | Default Value | Other Value(s)                        |
|:----------------|:-----------------------------------------------------|:--------------|:--------------------------------------|
| FilterItems     | Enables the Filter Items feature.                    | `"Enabled"`   | `"Disabled"`, `"Default"`<sup>1</sup> |
| FilterItemsList | A list of context tags used to select allowed items. | `[]`          | The tags to allow.<sup>2</sup>        |

1. See [Option Values](#option-values).
2. See [Item Tags](#item-tags).

### Label Chest

Add a label to a chest and have it appear in the menu and in-game while holding
shift.

| Config     | Description                      | Default Value | Other Value(s)  |
|:-----------|:---------------------------------|:--------------|:----------------|
| LabelChest | Enables the Label Chest feature. | `true`        | `true', 'false' |

### Open Held Chest

With a chest as your active item, you can hit the action button to bring up the
chest menu and access the chests
contents.

| Config        | Description                          | Default Value | Other Value(s)                        |
|:--------------|:-------------------------------------|:--------------|:--------------------------------------|
| OpenHeldChest | Enables the Open Held Chest feature. | `"Enabled"`   | `"Disabled"`, `"Default"`<sup>1</sup> |

1. See [Option Values](#option-values).

### Organize Chest

Organize Chest allows you to group and sort items by a configurable property of
those items.

| Config Option        | Description                         | Default Value | Other Value(s)                           |
|:---------------------|:------------------------------------|:--------------|:-----------------------------------------|
| OrganizeChest        | Enables the Organize Chest feature. | `"Enabled"`   | `"Disabled"`, `"Default"`<sup>1</sup>    |
| OrganizeChestGroupBy | What will items be grouped by.      | `"Default"`   | See [Group By Values](#group-by-values). |
| OrganizeChestSortBy  | What will items be sorted by.       | `"Default"`   | See [Sort By Values](#sort-by-values).   |

1. See [Option Values](#option-values).

### Resize Chest

The default storage for a chest is 36 items. With Resize Chest enabled you can
increase storage space up to virtually
unlimited storage.<sup>1</sup>

| Config              | Description                              | Default Value | Other Value(s)                            |
|:--------------------|:-----------------------------------------|:--------------|:------------------------------------------|
| ResizeChest         | Enables the Resize Chest feature.        | `"Enabled"`   | `"Disabled"`, `"Default"`<sup>2</sup>     |
| ResizeChestCapacity | The number of items the chest can store. | `60`          | Any positive integer or `-1`.<sup>3</sup> |
| ScrollDown          | Assigns a keybind to scroll down.        | `"DPadDown"`  | Any valid button code.<sup>4</sup>        |
| ScrollUp            | Assigns a keybind to scroll up.          | `"DPadUp"`    | Any valid button code.<sup>4</sup>        |

1. If the number of items exceeds the menu space, you can scroll to access the
   overflow.
2. See [Option Values](#option-values).
3. Use `-1` for "unlimited" items or a positive multiple of 12 for limited
   storage.
4.

See [Button Codes](https://stardewvalleywiki.com/Modding:Player_Guide/Key_Bindings#Button_codes).

### Search Items

Adds a search bar to the top of the Chest Menu to only display items that meet a
search criteria.

| Config          | Description                                              | Default Value | Other Value(s)                        |
|:----------------|:---------------------------------------------------------|:--------------|:--------------------------------------|
| SearchItems     | Enables the Search Items feature.                        | `"Enabled"`   | `"Disabled"`, `"Default"`<sup>1</sup> |
| SearchTagSymbol | Denotes a prefix for searching by item tags.<sup>2</sup> | `"#"`         | Any single character symbol.          |

1. See [Option Values](#option-values).
2. See [Item Tags](#item-tags).

### Slot Lock

Hover over an item slot in your backpack, and hit a configurable key to lock the
item in its slot which prevents it from
being stashed into a chest.

| Config        | Description                                     | Default Value | Other Value(s)                        |
|:--------------|:------------------------------------------------|:--------------|:--------------------------------------|
| SlotLock      | Enables the Slot Lock feature.                  | `"Enabled"`   | `"Disabled"`, `"Default"`<sup>1</sup> |
| SlotLockColor | Assign the outline color for locked slots.      | `"Red"`       | `"Blue"`, `"Green"`, ...<sup>2</sup>  |
| SlotLockHold  | Hold the LockSlot key and click to lock a slot. | `true`        | `false`                               |
| LockSlot      | Assigns the keybind for locking a slot.         | `"A"`         | Any valid button code.<sup>3</sup>    |

1. See [Option Values](#option-values).
2. See [Slot Lock Colors](#slot-lock-colors).
3.

See [Button Codes](https://stardewvalleywiki.com/Modding:Player_Guide/Key_Bindings#Button_codes).

### Stash To Chest

Hit a configurable key to instantly stash items from your inventory into nearby
chests.<sup>1</sup>

| Config Option                | Description                                               | Default Value | Other Value(s)                                                                |
|:-----------------------------|:----------------------------------------------------------|:--------------|:------------------------------------------------------------------------------|
| StashToChest                 | Enables the Stash To Chest feature.                       | `"Location"`  | `"Disabled"`, `"Default"`, `"Inventory"`, `"Location"`, `"World"`<sup>2</sup> |
| StashItems                   | Assigns the keybind for stashing items.                   | `"Z"`         | Any valid button code.<sup>3</sup>                                            |
| StashToChestDisableLocations | A list of locations that stashing will not be allowed in. | `[]`          | The locations to block.<sup>4</sup>                                           |
| StashToChestDistance         | Limits the distance that a chest can be stashed into.     | -1            | Any positive integer or `-1`.<sup>5</sup>                                     |
| StashToChestPriority         | Prioritize certain chests over others.                    | 0             | Any integer value.                                                            |
| StashToChestStacks           | Allow stashing items to existing stacks.                  | `"Enabled"`   | `"Disabled"`                                                                  |

1. Included chests are determined by config options.
2. See [Range Values](#range-values).
3.

See [Button Codes](https://stardewvalleywiki.com/Modding:Player_Guide/Key_Bindings#Button_codes).

4. Add `"UndergroundMine"` to the list to disable in Mine and Skull Cavern.
5. Measured in tiles away from the player. Use `-1` for "unlimited" distance.

### Transfer Items

Adds buttons that transfer all items into or out of the currently opened chest.

| Config        | Description                         | Default Value | Other Value(s) |
|:--------------|:------------------------------------|:--------------|:---------------|
| TransferItems | Enables the Transfer Items feature. | `"Enabled"`   | `"Disabled"`   |

### Unload Chest

While carrying a chest and facing another chest, hit the Use Tool button to
unload the carried chests contents into the
placed chest.

| Config Option      | Description                       | Default Value | Other Value(s)                       |
|:-------------------|:----------------------------------|:--------------|:-------------------------------------|
| UnloadChest        | Enables the Unload Chest feature. | `"Disabled"`  | `"Enabled"`, `"Default"`<sup>1</sup> |
| UnloadChestCombine | Combine source chest with target  | `"Disabled"`  | `"Enabled"`                          |

1. See [Option Values](#option-values).

## Usage

### Item Tags

The game adds
various [Context Tags](https://stardewvalleywiki.com/Modding:Items#Context_tags)
to each item which are used throughout this mod.

There are a few ways to see what context tags each item contains:

* Enter the console command `debug listtags` to show all tags for the currently
  held item.
* Refer to
  the [Modding Docs](https://stardewvalleywiki.com/Modding:Items#Context_tags)
  for some tags (note may be
  outdated).
* Install [Lookup Anything](https://www.nexusmods.com/stardewvalley/mods/541),
  enable [ShowDataMiningField](https://github.com/Pathoschild/StardewMods/tree/stable/LookupAnything#configure)
  in its
  config and hit F1 while hovering over any item.

Here are examples of some useful tags:

| Description | Tags                                                        |
|-------------|-------------------------------------------------------------|
| Category    | `category_clothing`, `category_boots`, `category_hats`, ... |
| Color       | `color_red`, `color_blue`, ...                              |
| Name        | `item_sap`, `item_wood`, ...                                |
| Type        | `wood_item`, `trash_item`, ...                              |
| Quality     | `quality_none`, `quality_gold`, ...                         |
| Season      | `season_spring`, `season_fall`, ...                         |
| Index       | `id_o_709`, `id_r_529`, ...                                 |

In addition to the vanilla game tags, this also adds some custom tags to items:

| Tag                  | Description                                                                                  |
|----------------------|----------------------------------------------------------------------------------------------|
| `category_artifact`  | Selects any item that is an Artifact.                                                        |
| `category_furniture` | Includes regular furniture items with this tag even though the game doesn't normally add it. |
| `donate_bundle`      | Selects any items that are missing from the Community Center Bundle.                         |
| `donate_museum`      | Selects any items that can be donated to the Museum.                                         |

## Configurations

For ease of use, it is recommended to set config options
from [Generic Mod Config Menu](https://www.nexusmods.com/stardewvalley/mods/5098).

### Config Inheritance

For some config options, the same options can be applied to multiple levels. The
lowest level config will always take
precedence.

* **Default Chest** - Every chest inherits from the Default Chest options.
* **Chest Type** - Applies to a particular type of chest.
    * Chest
    * Stone Chest
    * Junimo Chest
    * Auto-Grabber
    * Mini-Fridge
    * Mini-Shipping Bin
    * Fridge
    * Custom chests added by other mods.<sup>1</sup>
* **Individual Chest** - A single instance of a chest can be configured
  individually.

1. External mod must "opt-in" by [registering custom chest](#api) with Better
   Chests.

### Group By Values

Group by is a text property of the item that organize will order by first.

* **Default** - Group by the original organize method.
* **Category** - Group by the item category.
* **Color** - Group by the item color (only if context tag supports it).
* **Name** - Group by the item name.

## In Game Menus

The types of in-game menus.

* **Default** - Inherit from the parent option.
* **Categorize** - Only the Categorize menu will be loaded.
* **Simple** - Only show ChestLabel, Categorize, and Priority.
* **Full** - Show all options.
* **Advanced** - Show all options, and replace some options with open fields.

### Option Values

The option value determines whether a feature will be enabled or disabled for a
chest.

* **Default** - The value will be inherited from a parent config.<sup>1</sup>
* **Disabled** - The feature will be disabled.
* **Enabled** - The feature will be enabled.

1. If parent value is unspecified, Enabled will be the default value.

### Range Values

The Range value limits which chests will be selected for a feature relative to
the player.

* **Default** - The value will be inherited from a parent config.<sup>1</sup>
* **Disabled** - The feature will be disabled.
* **Inventory** - Only chests in the player inventory.
* **Location** - Only chests in the players current location.
* **World** - Any chest accessible to the player in the world.

1. If parent value is unspecified, Location will be the default value.

### Slot Lock Colors

The color to indicate an item slot is locked.

* **Red** - ( R:255, G:0, B:0 )
* **DarkRed** - ( R:139, G:0, B:0 )
* **PaleVioletRed** - ( R:219, G:112, B:147 )
* **Blue** - ( R:0, G:0, B:255 )
* **Green** - ( R:0, G:128, B:0 )
* **DarkGreen** - ( R:0, G:100, B:0 )
* **Jade** - ( R:0, G:128, B:128 )
* **Brown** - ( R:165, G:42, B:42 )
* **DarkBrown** - ( R:128, G:0, B:0 )
* **Yellow** - ( R:255, G:255, B:0 )
* **DarkYellow** - ( R:218, G:165, B:32 )
* **Aquamarine** - ( R:127, G:255, B:212 )
* **Purple** - ( R:128, G:0, B:128 )
* **DarkPurple** - ( R:75, G:0, B:130 )
* **Cyan** - ( R:0, G:255, B:255 )
* **Pink** - ( R:255, G:192, B:203 )
* **Orange** - ( R:255, G:165, B:0 )
* **Gray** - ( R:128, G:128, B:128 )

### Sort By Values

Sort by is a numerical property of the item that organize will order by second.

* **Default** - Sort by the original organize method.
* **Type** - Sort by the numerical category type of the item.
* **Quality** - Sort by the quality of the item (if applicable).
* **Quantity** - Sort by the stack size of the item.

## Mod Integrations

### Automate

When [Filter Items](#filter-items) is enabled, then any categorizations that a
chest has will be applied to
[Automate](https://www.nexusmods.com/stardewvalley/mods/1063). This means that
Automate will be blocked from adding
items into the chest if the Filter list does not allow it.

### Better Crafting

Craft from Chest will launch a Better Crafting Page and it will correctly
include all chests with the feature enabled.

### Horse Overhaul

Better Chests automatically integrates
with [Horse Overhaul](https://www.nexusmods.com/stardewvalley/mods/7911)
saddlebags. The distance to the player's Horse will be considered for features
such
as [Craft from Chest](#craft-from-chest) and
[Stash to Chest](#stash-to-chest).

The SaddleBag can have its own Better Chest config by adding an entry for a
chest named `"SaddleBag"` to the
`BetterChests/assets/chests.json` file.

## Customization

### API

Register your chest using
the [Better Chests API](../Common/Integrations/BetterChests/IBetterChestsApi.cs).

### Customized Tabs

The default tabs are defined by the `tabs.json` and `tabs.png` files which are
both found under the `assets` folder.

If `tabs.json` is not found, a default one is automatically generated:

| Key<sup>1</sup> | Display Name | Texture                                | Icon Index | Item Tags<sup>2</sup>                                                                                        |
|:----------------|:-------------|:---------------------------------------|:-----------|:-------------------------------------------------------------------------------------------------------------|
| Clothing        | Clothing     | `furyx639.BetterChests\\Tabs\\Texture` | 0          | Boots, Clothing, Hat                                                                                         |
| Cooking         | Cooking      | `furyx639.BetterChests\\Tabs\\Texture` | 1          | Artisan Goods, Cooking, Egg, Ingredients, Meat, Milk, Sell at Pierre's and Marnie's, Sell at Pierre's, Syrup |
| Crops           | Crops        | `furyx639.BetterChests\\Tabs\\Texture` | 2          | Flowers, Fruits, Greens, Vegetables                                                                          |
| Equipment       | Equipment    | `furyx639.BetterChests\\Tabs\\Texture` | 3          | Equipment, Ring, Tool, Weapon                                                                                |
| Fishing         | Fishing      | `furyx639.BetterChests\\Tabs\\Texture` | 4          | Bait, Fishing, Sell at Fish Shop, Tackle                                                                     | 
| Materials       | Materials    | `furyx639.BetterChests\\Tabs\\Texture` | 5          | Building Resources, Crafting, Gem, Metal Resources, Minerals, Monster Loot                                   |
| Misc            | Misc         | `furyx639.BetterChests\\Tabs\\Texture` | 6          | Big Craftable, Furniture, Junk                                                                               | 
| Seeds           | Seeds        | `furyx639.BetterChests\\Tabs\\Texture` | 7          | Fertilizer, Seeds                                                                                            |

You can edit these files directly for personal usage.

### Other Assets

#### Icons

Replace any or all of the icons for the Configure, Craft from Chest, and Stash
to Chest buttons by editing the
image<sup>1</sup>:

`furyx639.BetterChests/Icons`.

Sample `content.json`:

```jsonc
{
  "Format": "1.24.0",
  "Changes": [
    {
      "Action": "EditImage",
      "Target": "furyx639.BetterChests/Icons",
      "FromFile": "assets/MyConfigureButton.png",
      "FromArea": {"X": 0, "Y": 0, "Width": 16, "Height": 16},
      "ToArea": {"X": 0, "Y": 0, "Width": 16, "Height": 16}
    },
  ]
}
```

1. See
   the [Edit Image](https://github.com/Pathoschild/StardewMods/blob/develop/ContentPatcher/docs/author-guide/action-editimage.md)
   docs for Content Patcher.

#### Tab Texture

Replace any or all of the default tab textures by editing the image<sup>1</sup>:

`furyx639.BetterChests\\Tabs\\Textures`

Sample `content.json`:

```jsonc
{
  "Format": "1.24.0",
  "Changes": [
    // Replace texture for mining icon
    {
      "Action": "EditImage",
      "Target": "furyx639.BetterChests\\Tabs\\Texture",
      "FromFile": "assets/mining-icon.png",
      "FromArea": {"X": 0, "Y": 0, "Width": 16, "Height": 16},
      "ToArea" {"X": 48, "Y": 0, "Width": 16, "Height": 16}
    }
  ]
}
```

1. See
   the [Edit Image](https://github.com/Pathoschild/StardewMods/blob/develop/ContentPatcher/docs/author-guide/action-editimage.md)
   docs for Content Patcher.

## Translations

See [here](i18n/default.json) for the base translation file.

| Language                   | Status            | Credits              |
|:---------------------------|:------------------|:---------------------|
| [Chinese](i18n/zh.json)    | ✔️ Complete       | Andyc66              |
| [French](i18n/fr.json)     | ❔ Incomplete      | Ayatus               |
| [German](i18n/de.json)     | ❔ Incomplete      | Loni4ever            |
| [Hungarian](i18n/hu.json)  | ❔ Incomplete      | martin66789          |
| [Italian](i18n/it.json)    | ✔️ Complete       | zomboide             |
| Japanese                   | ❌️ Not Translated |                      |
| [Korean](i18n/ko.json)     | ✔️ Complete       | wally232             |
| [Portuguese](i18n/pt.json) | ✔️ Complete       | Aulberon             |
| [Russian](i18n/ru.json)    | ❔ Incomplete      | Newrotd              |
| [Spanish](i18n/es.json)    | ✔️ Complete       | Querbis              |
| [Turkish](i18n/tr.json)    | ✔️ Complete       | KawaiFoxHappyClaws76 |
