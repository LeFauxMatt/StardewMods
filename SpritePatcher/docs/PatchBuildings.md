# Buildings

* [Building](#building)
* [Fish Pond](#fish-pond)
* [Junimo Hut](#junimo-hut)
* [Pet Bowl](#pet-bowl)
* [Shipping Bin](#shipping-bin)

## Building

The Building class generically describes almost all buildings in the game.

| field                              | type                                          | description                                  |
|------------------------------------|-----------------------------------------------|----------------------------------------------|
| `buildingChests.First()`           | [Chest](./PatchItems#chest)                   | Get the building chest.                      |
| `GetIndoors().getAllFarmAnimals()` | [FarmAnimals](./PatchCharacters#farmanimal)[] | Get the animals belonging to a barn or coop. |

## Fish Pond

| field                   | type                        |   | description                                                  |
|-------------------------|-----------------------------|:--|--------------------------------------------------------------|
| `FishCount`             | int                         |   | Get the number of fish in the pond.                          |
| `fishType.Value`        | string                      |   | Get the type of fish.                                        |
| `output.Value`          | [Item](./PatchItems#object) |   | Get the item output of the pond.                             |
| `neededItem.Value`      | [Item](./PatchItems#object) |   | Get the item being requested by the fish.                    |
| `neededItemCount.Value` | int                         |   | Get how many items are still needed to complete the request. |

## Junimo Hut


| field              | type                        | description             |
|--------------------|-----------------------------|-------------------------|
| `GetOutputChest()` | [Chest](./PatchItems#chest) | Get the building chest. |

## Pet Bowl

## Shipping Bin

