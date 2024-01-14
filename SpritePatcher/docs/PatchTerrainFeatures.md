# Terrain Features

* [Bush](#bush)
* [CosmeticPlant](#cosmeticplant)
* [Flooring](#flooring)
* [FruitTree](#fruittree)
* [GiantCrop](#giantcrop)
* [Grass](#grass)
* [HoeDirt](#hoedirt)
* [ResourceClump](#resourceclump)
* [Tree](#tree)


## Bush

| field           | type | description                              |
|-----------------|------|------------------------------------------|
| `IsSheltered()` | bool | Get whether the bush is planted indoors. |

## CosmeticPlant

## Flooring

## FruitTree

| field                | type                          | description                                |
|----------------------|-------------------------------|--------------------------------------------|
| `fruit.Value`        | [Item](./PatchItems#object)[] | Get the fruit.                             |

## GiantCrop

## Grass

## HoeDirt

| field                | type | description                                |
|----------------------|------|--------------------------------------------|
| `Crop`               | Crop | Get the crop.                              |
| `HasFertilizer()`    | bool | Get whether the hoe dirt is fertilized.    |
| `hasPaddyCrop()`     | bool | Get whether the hoe dirt has a paddy crop. |
| `needsWatering()`    | bool | Get whether the hoe dirt needs watering.   |
| `readyiForHarvest()` | bool | Get whether the hoe dirt needs watering.   |

## ResourceClump

## Tree


| field                  | type   | description                            |
|------------------------|--------|----------------------------------------|
| `treeType.Value`       | string | Get the tree type.                     |
| `tapped.Value`         | bool   | Get whether the tree is tapped.        |
| `wasShakenToday.Value` | bool   | Get whether the tree was shaken today. |
| `hasSeed.Value`        | bool   | Get whether the tree has a seed.       |