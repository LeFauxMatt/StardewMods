## eXpanded Storage Content Pack

### Contents

* [Overview](#overview)
* [eXpanded Storage](#expanded-storage)
* [Sprite Sheet](#sprite-sheet)
* [Context Tags](#context-tags)
* [Mod Data](#mod-data)

### Overview

All eXpanded Storage content packs must contain the following files:

- `manifest.json`
- `expanded-storage.json`
- `content.json`
- `assets/[SpriteSheet].png`
- `i18n/default.json`

#### manifest.json

`manifest.json` should specify this is a content pack for eXpanded Storage:

```json
{
  "ContentPackFor": {
    "UniqueID": "furyx639.ExpandedStorage"
  }
}
```

See more on manifest [here](https://stardewvalleywiki.com/Modding:Modder_Guide/APIs/Manifest#Basic_examples)

#### expanded-storage.json

`expanded-storage.json` is used to define each storage:

```json
{
  "MyStorage": {
    
  },
  "AnotherStorage": {
    
  }
}
```
See more on the supported fields [below](#expanded-storage).

#### content.json

`content.json` follows [Dynamic Game Assets format](https://github.com/spacechase0/StardewValleyMods/tree/develop/DynamicGameAssets#big-craftables)
for loading big craftables.

```json
{
  "$ItemType": "BigCraftable",
  "ID": "MyStorage",
  "Texture": "assets/MyStorage.png:0"
}
```


### Expanded Storage

Example content pack:

```json
{
  "StorageName": {
    "SpecialChestType": "None",
    "IsFridge": false,
    "HeldStorage": false,
    "OpenNearby": 0,
    "OpenNearbySound": "doorCreak",
    "CloseNearbySound": "doorCreakReverse",
    "OpenSound": "openChest",
    "PlaceSound": "axe",
    "CarrySound": "pickUpItem",
    "IsPlaceable": true,
    "Image": "MyStorage.png",
    "Frames": 1,
    "Depth": 16,
    "Animation": "None",
    "PlayerColor": true,
    "ModData": {
      "ModDataKey": "ModDataValue"
    },
    "Capacity": 36,
    "AllowList": [],
    "BlockList": [],
    "EnabledFeatures": ["CanCarry", "AccessCarried"]
  }
}
```

field               | description
--------------------|-------------
`StorageName`       | Name of the object, must match the Big Craftable name. **(Required)**
`SpecialChestType`  | `"None"`, `"MiniShippingBin"`, or `"JunimoChest"`. (default `"None"`)
`IsFridge`          | Set to `true` if storage should be treated as a Mini-Fridge. (default `false`)
`HeldStorage`       | Set to `true` to pull items from held storage such as Auto-Grabber. (default `false`)
`OpenNearby`        | Set to `1` for storage to automatically open whenever a Farmer is within 1 tile. (default `0`)
`OpenNearbySound`   | Sound to play when OpenNearby is true and player approaches storage. (default `"doorCreak"`)  <sup>[1](#handyheadphones)</sup>
`CloseNearbySound`  | Sound to play when OpenNearby is true and player walks away from storage. (default `"doorCreak"`)  <sup>[1](#handyheadphones)</sup>
`OpenSound`         | Sound to play when storage is being opened. (default `"openChest"`) <sup>[1](#handyheadphones)</sup>
`PlaceSound`        | Sound to play when storage is placed. (default `"axe"`) <sup>[1](#handyheadphones)</sup>
`CarrySound`        | Sound to play when storage is picked up. (default `"pickUpItem"`) <sup>[1](#handyheadphones)</sup>
`IsPlaceable`       | Set to `false` to disallow chest from being placed. (default `true`)
`Image`             | Image in assets used as the spritesheet for this storage. (default `null`) <sup>[2](#spritesheet)</sup>
`Frames`            | Number of frames to animate when opening the storage. (default `1`)
`Depth`             | Distance from bottom of sprite that is used to determine placement/obstruction. (default `height-16`)
`Animation`         | `"None"`, `"Loop"`, or `"Color"`. (default `"None"`)
`PlayerColor`       | Set to `true` to allow player color choice. (default `false`)
`ModData`           | Adds to the storage [modData](#mod-data) when placed. (default `null`)
`AllowList`         | Restrict chest to only accept items containing these [tags](#context-tags). (default `[]`)
`BlockList`         | Restrict chest to reject items containing these [tags](#context-tags). (default `[]`)
`Capacity`          | Number of item slots this storage supports. `-1` will be treated as infinite items, `0` will use the default vanilla value. (default `0`) <sup>[3](#storagecapacity)</sup>
`EnabledFeatures`   | List of features to enable. (default `[]`) <sup>[4](#features)</sup>

<span id="handyheadphones">1.</span> I recommend
[Handy Headphones](https://www.nexusmods.com/stardewvalley/mods/7936) to listen
to sounds available to play from in-game.  
<span id="spritesheet">2.</span> Refer to the [Sprite Sheets](#sprite-sheets) section for arranging sprites correctly.  
<span id="storagecapacity">3.</span> Assign a capacity of at least one row (12) to avoid visual glitches.  
<span id="features">4.</span> Refer to [XSPlus](https://github.com/ImJustMatt/StardewMods/tree/develop/XSPlus/docs/features.md) for documentation on features.

### Sprite Sheet

A sprite sheet is a single image with frames that span horizontally and layers which span vertically.

#### Storage Dimensions

This sprite sheet is automatically used to determine the object's dimensions.

The width of the object is determined by `Total Image Width ÷ Frames`.

If `PlayerColor` is set to `false` the height of the object will be `Total Image Height`.  
If `PlayerColor` is set to `true` the height of the object will be `Total Image Height ÷ 3`.

#### Animation Frames

The left-most frame will always be the object at rest, and every frame after is animated when the storage is opened.

#### Player Color

When `PlayerColor` is set to `true` then the spritesheet must include three rows of sprites.

row | sprites
----|---------
1   | Default render when no player color choice is made.
2   | Base layer that will be rendered with the player's color choice.
3   | Drawn over the Base layer, but without the player's color choice.


### Context Tags

The game adds various [context tags](https://stardewcommunitywiki.com/Modding:Context_tags) to every item. The recommended way to find what context tags are supported by which items is to install [Lookup Anything](https://www.nexusmods.com/stardewvalley/mods/541) and enable it's [ShowDataMiningFields](https://github.com/Pathoschild/StardewMods/tree/stable/LookupAnything#configure) option. Then simply press `F1` while hovered over any item to find it's data.

Below are some of the more useful tags:

description     | tags
----------------|------
Category        | `category_clothing`, `category_boots`, `category_hats`, ...
Color           | `color_red`, `color_blue`, ...
Name            | `item_sap`, `item_wood`, ...
Type            | `wood_item`, `trash_item`, ...
Quality         | `quality_none`, `quality_gold`, ...
Season          | `season_spring`, `season_fall`, ...
Index           | `id_o_709`, `id_r_529`, ...

Additionally the following custom tags are supported:

tag                 | description
--------------------|-------------
`category_artifact` | Custom tag to select for any Artifacts.
`category_furniture`| Includes regular furniture items with this tag even though the game doesn't normally add it.
`donate_bundle`     | Selects any items that are missing from the Community Center Bundle.
`donate_museum`     | Selects any items that can be donated to the Museum.

### Mod Data

All items support a key-value store known as Mod Data. You can initialize values for your storage to integrate with features from mods. This will only add the modData when the item is originally obtained or crafted, and it will not override if a value already exists.

Of course Mod Data opens a world of possibilities depending on the mod, but here are a few notable examples:

key                                             | description
------------------------------------------------|-------------
`Pathoschild.ChestsAnywhere/IsIgnored`          | If `true` storage will be de-listed from Chests Anywhere .
`Pathoschild.ChestsAnywhere/Category`           | Storage will automatically be added to a Category.
`Pathoschild.Automate/StoreItems`               | If `"disable"` Automate will not store items in this storage
`Pathoschild.Automate/TakeItems`                | If `"disable"` Automate will not take items from this storage
