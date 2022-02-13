# Easy Access

Provides easier access to machines and producers in the game.

## Contents

* [Features](#features)
    * [Collect Outputs](#collect-outputs)
    * [Dispense Inputs](#dispense-inputs)
* [Supported Objects](#supported-objects)
* [Configurations](#configurations)
    * [Item Tags](#item-tags)
        * [Range Values](#range-values)

## Features

### Collect Outputs

Hit a configurable key to instantly collect output items from nearby producers.<sup>1</sup>

| Config Option         | Description                                                | Default Value | Other Value(s)                                                  |
|:----------------------|:-----------------------------------------------------------|:--------------|:----------------------------------------------------------------|
| CollectOutputs        | Enables the Collect Outputs feature.                       | `"Location"`  | `"Disabled"`, `"Default"`, `"Location"`, `"World"` <sup>2</sup> |
| CollectItems          | Assigns the keybind for collecting items.                  | `"Delete"`    | Any valid button code.<sup>3</sup>                              |
| CollectOutputDistance | Limits the distance that a producer can be collected from. | 15            | Any positive integer or `-1`.<sup>4</sup>                       |
| CollectOutputItems    | A list of context tags used to select allowed items.       | `[]`          | The tags to allow.<sup>5</sup>                                  |

1. Included producers are determined by config options.
2. See [Range Values](#range-values).
3. See [Button Codes](https://stardewvalleywiki.com/Modding:Player_Guide/Key_Bindings#Button_codes).
4. Measured in tiles away from the player. Use `-1` for "unlimited" distance.
5. See [Item Tags](#item-tags).

### Dispense Inputs

Hit a configurable key to instantly dispense input items into nearby producers.<sup>1</sup>

| Config Option         | Description                                                | Default Value | Other Value(s)                                                  |
|:----------------------|:-----------------------------------------------------------|:--------------|:----------------------------------------------------------------|
| DispenseInputs        | Enables the Dispense Inputs feature.                       | `"Location"`  | `"Disabled"`, `"Default"`, `"Location"`, `"World"` <sup>2</sup> |
| DispenseItems         | Assigns the keybind for dispensing items.                  | `"Insert"`    | Any valid button code.<sup>3</sup>                              |
| DispenseInputDistance | Limits the distance that a producer can be dispensed into. | 15            | Any positive integer or `-1`.<sup>4</sup>                       |
| DispenseInputItems    | A list of context tags used to select allowed items.       | `[]`          | The tags to allow.<sup>5</sup>                                  |
| DispenseInputPriority | Prioritize certain producers over others.                  | 0             | Any integer value.                                              |

1. Included producers are determined by config options.
2. See [Range Values](#range-values).
3. See [Button Codes](https://stardewvalleywiki.com/Modding:Player_Guide/Key_Bindings#Button_codes).
4. Measured in tiles away from the player. Use `-1` for "unlimited" distance.
5. See [Item Tags](#item-tags).

## Supported Objects

* Bee House
* Bone Mill
* Cask<sup>1</sup>
* Charcoal Kiln
* Cheese Press
* Coffee Maker
* Crab Pots<sup>2</sup>
* Crystalarium
* Furnace
* Geode Crusher
* Heavy Tapper
* Keg
* Lightning Rod
* Loom
* Mayonnaise Machine
* Mushroom Box
* Oil Maker
* Ostrich Incubator
* Preserves Jar
* Recycling Machine
* Seed Maker
* Slime Egg Press
* Slime Incubator
* Tapper
* Wood Chipper

Default Config:

1. Only items of iridium quality will be extracted out of casks.
2. Crab pots will only dispense regular bait into them.

## Configurations

### Item Tags

The game adds various [Context Tags](https://stardewcommunitywiki.com/Modding:Context_tags)
to each item which are used throughout this mod.

There are a few ways to see what context tags each item contains:

* Enter the console command `debug listtags` to show all tags for the currently held item.
* Refer to the [Modding Docs](https://stardewcommunitywiki.com/Modding:Context_tags) for some tags (note may be
  outdated).
* Install [Lookup Anything](https://www.nexusmods.com/stardewvalley/mods/541),
  enable [ShowDataMiningField](https://github.com/Pathoschild/StardewMods/tree/stable/LookupAnything#configure) in its
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

### Range Values

The Range value limits which chests will be selected for a feature relative to the player.

* **Default** - The value will be inherited from a parent config.<sup>1</sup>
* **Disabled** - The feature will be disabled.
* **Location** - Only chests in the players current location.
* **World** - Any chest accessible to the player in the world.

1. If parent value is unspecified, Location will be the default value.