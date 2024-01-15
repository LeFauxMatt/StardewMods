# Tools

* [FishingRod](#fishingrod)
* [MeleeWeapon](#meleeweapon)
* [Slingshot](#slingshot)
* [Tool](#tool)
* [WateringCan](#wateringcan)

## FishingRod

### Common Attributes

| field         | type                            | description              |
|---------------|---------------------------------|--------------------------|
| `GetBait()`   | [Object](./PatchItems#object)[] | Get the equipped bait.   |
| `GetTackle()` | [Object](./PatchItems#object)[] | Get the equipped tackle. |

## MeleeWeapon

Refer to `Data/Weapons.json` in
the [unpacked Content folder](https://stardewvalleywiki.com/Modding:Editing_XNB_files#Unpack_game_files)
for a complete list of the weapon targets/areas.

## Slingshot

### Common Attributes

| field                     | type                            | description     |
|---------------------------|---------------------------------|-----------------|
| `attachments[0].getOne()` | [Object](./PatchItems#object)[] | Get ammunition. |

## Tool

Refer to `Data/Tools.json` in
the [unpacked Content folder](https://stardewvalleywiki.com/Modding:Editing_XNB_files#Unpack_game_files)
for a complete list of the tool targets/areas.

## WateringCan

### Common Attributes

| field       | type | description                   |
|-------------|------|-------------------------------|
| `WaterLeft` | int  | Get the amount of water left. |