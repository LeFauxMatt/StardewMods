# Buildings

* [Building](#building)
* [Fish Pond](#fish-pond)
* [Junimo Hut](#junimo-hut)
* [Pet Bowl](#pet-bowl)
* [Shipping Bin](#shipping-bin)

## Building

Refer to `Data/Buildings.json` in
the [unpacked Content folder](https://stardewvalleywiki.com/Modding:Editing_XNB_files#Unpack_game_files)
for a complete list of the building targets and areas.

| target                  | area           | description       |
|-------------------------|----------------|-------------------|
| `Buildings/Barn`        | 0, 0, 112, 112 | The regular barn. |
| `Buildings/Big Barn`    | 0, 0, 112, 112 | The big barn.     |
| `Buildings/Deluxe Barn` | 0, 0, 112, 112 | The deluxe barn.  |
| `Buildings/Coop`        | 0, 0, 96, 112  | The regular coop. |
| `Buildings/Big Coop`    | 0, 0, 96, 112  | The big coop.     |
| `Buildings/Deluxe Coop` | 0, 0, 96, 112  | The deluxe coop.  |

### Common Attributes

| field                              | type                                          | description                                  |
|------------------------------------|-----------------------------------------------|----------------------------------------------|
| `buildingChests.First()`           | [Chest](./PatchItems#chest)                   | Get the building chest.                      |
| `GetIndoors().getAllFarmAnimals()` | [FarmAnimals](./PatchCharacters#farmanimal)[] | Get the animals belonging to a barn or coop. |


### Code Samples

**Apply patch only if the Building contains Cows.**

```js
if (entity is not Building building) return false;
if (!building.GetIndoors().getAllFarmAnimals().Any(animal => animal.type.Value.Contains(`Cow`))) return false;
this.Path = `{{InternalAssetKey: assets/barn.png}}`;
return true;
```

## Fish Pond

| target                | area          | description                      |
|-----------------------|---------------|----------------------------------|
| `Buildings/Fish Pond` | 0, 0, 80, 80  | The stones surrounding the pond. |
| `Buildings/Fish Pond` | 0, 80, 80, 80 | The pond water.                  |

### Common Attributes

| field                   | type                        |   | description                                                  |
|-------------------------|-----------------------------|:--|--------------------------------------------------------------|
| `FishCount`             | int                         |   | Get the number of fish in the pond.                          |
| `fishType.Value`        | string                      |   | Get the type of fish.                                        |
| `output.Value`          | [Item](./PatchItems#object) |   | Get the item output of the pond.                             |
| `neededItem.Value`      | [Item](./PatchItems#object) |   | Get the item being requested by the fish.                    |
| `neededItemCount.Value` | int                         |   | Get how many items are still needed to complete the request. |

### Code Samples

**Apply a patch depending on how many fish there are in the pond.**

```js
if (entity is not FishPond fishPond) return false;
if (fishPond.FishCount == 0) return false;
this.Path = `{{InternalAssetKey: assets/fishPond.png}}`;
if (fishPond.FishCount == fishPond.maxOccupants.Value) this.Area = new Rectangle(80, 80, 80, 80);
else if (fishPond.FishCount > fishPond.maxOccupants.Value / 2) this.Area = new Rectangle(0, 80, 80, 80);
else if (fishPond.FishCount > 0) this.Area = new Rectangle(80, 0, 80, 80);
else this.Area = new Rectangle(0, 0, 80, 80);
return true;
```

This patch will replace the texture, and draw from a different area of the
texture depending on how many fish are in the pond.

## Junimo Hut

| target                 | area           | description            |
|------------------------|----------------|------------------------|
| `Buildings/Junimo Hut` | 0, 0, 48, 64   | The spring Junimo hut. |
| `Buildings/Junimo Hut` | 48, 0, 48, 64  | The summer Junimo hut. |
| `Buildings/Junimo Hut` | 96, 0, 48, 64  | The autumn Junimo hut. |
| `Buildings/Junimo Hut` | 144, 0, 48, 64 | The winter Junimo hut. |

### Common Attributes

| field              | type                        | description             |
|--------------------|-----------------------------|-------------------------|
| `GetOutputChest()` | [Chest](./PatchItems#chest) | Get the building chest. |

## Pet Bowl

## Shipping Bin

