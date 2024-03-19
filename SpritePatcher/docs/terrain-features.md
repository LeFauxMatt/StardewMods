## Terrain Features

* [Bush](#bush)
* [CosmeticPlant](#cosmeticplant)
* [Flooring](#flooring)
* [FruitTree](#fruittree)
* [GiantCrop](#giantcrop)
* [Grass](#grass)
* [HoeDirt](#hoedirt)
* [ResourceClump](#resourceclump)
* [Tree](#tree)


### Bush

#### Common Attributes

| field           | type | description                              |
|-----------------|------|------------------------------------------|
| `IsSheltered()` | bool | Get whether the bush is planted indoors. |

### CosmeticPlant

### Flooring

Refer to `Data/FloorsAndPaths.json` in
the [unpacked Content folder](https://stardewvalleywiki.com/Modding:Editing_XNB_files#Unpack_game_files)
for a complete list of the flooring targets/areas.

### FruitTree

Refer to `Data/FruitTrees.json` in
the [unpacked Content folder](https://stardewvalleywiki.com/Modding:Editing_XNB_files#Unpack_game_files)
for a complete list of the fruit tree targets/areas.

#### Common Attributes

| field                | type                          | description                                |
|----------------------|-------------------------------|--------------------------------------------|
| `fruit.Value`        | [Item](./PatchItems#object)[] | Get the fruit.                             |

### GiantCrop

Refer to `Data/GiantCrops.json` in
the [unpacked Content folder](https://stardewvalleywiki.com/Modding:Editing_XNB_files#Unpack_game_files)
for a complete list of the giant crop targets/areas.

### Grass

### HoeDirt

Refer to `Data/Crops.json` in
the [unpacked Content folder](https://stardewvalleywiki.com/Modding:Editing_XNB_files#Unpack_game_files)
for a complete list of the crop targets/areas.

#### Common Attributes

| field                | type | description                                |
|----------------------|------|--------------------------------------------|
| `Crop`               | Crop | Get the crop.                              |
| `HasFertilizer()`    | bool | Get whether the hoe dirt is fertilized.    |
| `hasPaddyCrop()`     | bool | Get whether the hoe dirt has a paddy crop. |
| `needsWatering()`    | bool | Get whether the hoe dirt needs watering.   |
| `readyiForHarvest()` | bool | Get whether the hoe dirt needs watering.   |

### ResourceClump

### Tree

Refer to `Data/WildTrees.json` in
the [unpacked Content folder](https://stardewvalleywiki.com/Modding:Editing_XNB_files#Unpack_game_files)
for a complete list of the tree targets/areas.

#### Common Attributes

| field                  | type   | description                            |
|------------------------|--------|----------------------------------------|
| `treeType.Value`       | string | Get the tree type.                     |
| `tapped.Value`         | bool   | Get whether the tree is tapped.        |
| `wasShakenToday.Value` | bool   | Get whether the tree was shaken today. |
| `hasSeed.Value`        | bool   | Get whether the tree has a seed.       |