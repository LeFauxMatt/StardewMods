## Content Format

### Overview

The overall structure of an Expanded Storage content pack consists of two files `manifest.json` and `expandedStorage.json`

From `manifest.json` you must specify that the mod is a content pack for Expanded Storage by including the following:
```json
"ContentPackFor": {
  "UniqueID": "furyx639.ExpandedStorage"
}
```

Your `expandedStorage.json` should list all [storages](#expanded-storage) and [tabs](#storage-tabs) it has using the format outlined in the following sections.

```json
{
  "ExpandedStorage": [
    {
      // Expanded Storage
    },
    {
      // Expanded Storage
    }
  ],
  "StorageTabs": [
    {
      // Storage Tab
    },
    {
      // Storage Tab
    }
  ]
}
```

### Expanded Storage

An Expanded Storage must be loaded into the game as a [Big Craftable](https://stardewcommunitywiki.com/Modding:Big_craftables_data). These can be loaded using various methods, but [Json Assets](https://www.nexusmods.com/stardewvalley/mods/1720) is recommended. Other options include [Custom Chest Types](https://www.nexusmods.com/stardewvalley/mods/7755), [Content Patcher](https://www.nexusmods.com/stardewvalley/mods/1915), or loading as a [SMAPI mod](https://stardewcommunitywiki.com/Modding:Modder_Guide/APIs/Content#Add_a_new_asset).

```json
"StorageName": "Large Chest",
"Capacity": 72,
"CanCarry": true,
"AccessCarried": true,
"VacuumItems": true,
"ShowSearchBar": true,
"IsPlaceable": true,
"OpenSound": "openChest",
"IsFridge": false,
"SpecialChestType": "None",
"AllowList": [],
"BlockList": [],
"Tabs": ["Crops", "Materials", "Other"],
"ModData": {
  "ModDataKey": "ModDataValue"
}
```

field               | description
--------------------|-------------
`StorageName`       | Name of the object, must match the Big Craftable name. **(Required)**
`Capacity`          | Number of item slots this storage supports. `-1` will be treated as infinite items, `0` will use the default vanilla value. (default 0)
`CanCarry`          | Allows this storage to be picked up when it contains items. (default `true`)
`AccessCarried`     | Open the chest menu while item is currently being held. (default `false`)
`VacuumItems`       | Storage will collect dropped items directly into it, bypassing the backpack. (default `false`)
`ShowSearchBar`     | Add a search bar to the chest menu for this storage. (default `false`)
`IsPlaceable`       | Set to `false` to disallow chest from being placed. (default `true`)
`OpenSound`         | Sound to play when storage is being opened. (default `"openChest"`)
`IsFridge`          | Set to `true` if storage should be treated as a Mini-Fridge. (default `false`)
`SpecialChestType`  | `"None"`, `"MiniShippingBin"`, or `"JunimoChest"`. (default `"None"`) 
`AllowList`         | Restrict chest to only accept items containing these [tags](#context-tags). (default `null`)
`BlockList`         | Restrict chest to reject items containing these [tags](#context-tags). (default `null`)
`Tabs`              | Adds [tabs](#storage-tabs) to the chest menu for this storage by the tab name(s). (default `null`)
`ModData`           | Adds to the storage [modData](#mod-data) when placed. (default `null`)

### Storage Tabs

```json
"TabName": "Crops",
"TabImage": "Crops.png",
"AllowList": [
    "category_greens",
    "category_flowers",
    "category_fruits",
    "category_vegetables"
],
"BlockList": []
```

field           | description
----------------|-------------
`TabName`       | Name of the object, must match the Big Craftable name. **(Required)**
`TabImage`      | Number of item slots this storage supports. `-1` will be treated as infinite items, `0` will use the default vanilla value. (default 0)
`AllowList`     | Restrict chest to only accept items containing these [tags](#context-tags). (default `null`)
`BlockList`     | Restrict chest to reject items containing these [tags](#context-tags). (default `null`)

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

If you think any other examples would be useful to add to this list, please let me know on my [Discord server](https://discord.gg/MR29ZgUeSd).