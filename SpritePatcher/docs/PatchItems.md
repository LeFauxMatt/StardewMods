# Items

* [Boots](#boots)
* [Chest](#chest)
* [Clothing](#clothing)
* [ColoredObject](#coloredobject)
* [CombinedRing](#combinedring)
* [CrabPot](#crabpot)
* [Fence](#fence)
* [FishTank](#fishtank)
* [Furniture](#furniture)
* [Hat](#hat)
* [IndoorPot](#indoorpot)
* [ItemPedestal](#itempedestal)
* [Object](#object)
* [Ring](#ring)
* [Wallpaper](#wallpaper)
* [WoodChipper](#woodchipper)


## Boots

## Chest

### Common Attributes

| field                 | type              | description                 |
|-----------------------|-------------------|-----------------------------|
| `GetItemsForPlayer()` | [Item](#object)[] | Get the items in the Chest. |

## Clothing

## ColoredObject

### Common Attributes

| field         | type  | description                          |
|---------------|-------|--------------------------------------|
| `color.Value` | Color | Get the Color of the Colored object. |

## CombinedRing

### Common Attributes

| field                 | type            | description    |
|-----------------------|-----------------|----------------|
| `combinedRings.Value` | [Ring](#ring)[] | Get the rings. |

## CrabPot

### Common Attributes

| field        | type              | description        |
|--------------|-------------------|--------------------|
| `bait.Value` | [Object](#object) | Get the bait item. |

## Fence

Refer to `Data/Fences.json` in
the [unpacked Content folder](https://stardewvalleywiki.com/Modding:Editing_XNB_files#Unpack_game_files)
for a complete list of the fence targets/areas.

## FishTank

## Furniture

## Hat

## IndoorPot

### Common Attributes

| field           | type                                      | description                  |
|-----------------|-------------------------------------------|------------------------------|
| `hoeDirt.Value` | [HoeDirt](./PatchTerrainFeatures#hoedirt) | Get the hoe dirt in the pot. |

## ItemPedestal

## Object

Refer to `Data/Objects.json` in
the [unpacked Content folder](https://stardewvalleywiki.com/Modding:Editing_XNB_files#Unpack_game_files)
for a complete list of the object targets/areas.

### Common Attributes

| field              | type   | description          |
|--------------------|--------|----------------------|
| `heldObject.Value` | Object | Get the held object. |

*Held object is used in many objects such as Casks and Kegs.*

## Ring

## Wallpaper

## WoodChipper
