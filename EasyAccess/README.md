# Easy Access

Provides easier access to machines and producers in the game.

## Contents

* [Features](#features)
    * [Collect Outputs](#collect-outputs)
    * [Dispense Inputs](#dispense-inputs)
* [Customization](#customization)
    * [Assets](#assets)
* [Translations](#translations)

## Features

### Collect Outputs

Hit a configurable key to instantly collect output items from nearby producers.<sup>1</sup>

| Config Option         | Description                                                                            | Default Value | Other Value(s)                     |
|:----------------------|:---------------------------------------------------------------------------------------|:--------------|:-----------------------------------|
| CollectItems          | Assigns the keybind for collecting items.                                              | `"Delete"`    | Any valid button code.<sup>1</sup> |
| CollectOutputDistance | Limits the distance that a producer can be collected from.                             | 15            | Any positive integer.              |
| DoDigSpots            | Indicates whether Collect Outputs will grab from dig spots.                            | `true`        | `true` or `false`                  |
| DoForage              | Indicates whether Collect Outputs will drop forage as debris.                          | `true`        | `true` or `false`                  |
| DoMachines            | Indicates whether Collect Outputs will collect from machines.                          | `true`        | `true` or `false`                  |
| DoTerrain             | Indicates whether Collect Outputs will interact with terrain such as Trees and Bushes. | `true`        | `true` or `false`                  |

1. See [Button Codes](https://stardewvalleywiki.com/Modding:Player_Guide/Key_Bindings#Button_codes).

### Dispense Inputs

Hit a configurable key to instantly dispense input items into nearby producers.<sup>1</sup>

| Config Option         | Description                                                | Default Value | Other Value(s)                     |
|:----------------------|:-----------------------------------------------------------|:--------------|:-----------------------------------|
| DispenseItems         | Assigns the keybind for dispensing items.                  | `"Insert"`    | Any valid button code.<sup>1</sup> |
| DispenseInputDistance | Limits the distance that a producer can be dispensed into. | 15            | Any positive integer.              |

1. See [Button Codes](https://stardewvalleywiki.com/Modding:Player_Guide/Key_Bindings#Button_codes).

## Customization

### Assets

#### Icons

Replace any or all of the icons for the Collect Output or Dispense Input buttons by editing the image<sup>1</sup>:

`furyx639.EasyAccess/Icons`.

Sample `content.json`:

```jsonc
{
  "Format": "1.24.0",
  "Changes": [
    {
      "Action": "EditImage",
      "Target": "furyx639.EasyAccess/Icons",
      "FromFile": "assets/MyDispenseItemsButton.png",
      "FromArea": {"X": 0, "Y": 0, "Width": 16, "Height": 16},
      "ToArea": {"X": 16, "Y": 0, "Width": 16, "Height": 16}
    },
  ]
}
```

1. See
   the [Edit Image](https://github.com/Pathoschild/StardewMods/blob/develop/ContentPatcher/docs/author-guide/action-editimage.md)
   docs for Content Patcher.

## Translations

See [here](i18n/default.json) for the base translation file.

| Language                   | Status            | Credits  |
|:---------------------------|:------------------|:---------|
| Chinese                    | ❌️ Not Translated |          |
| French                     | ❌️ Not Translated |          |
| German                     | ❌️ Not Translated |          |
| Hungarian                  | ❌️ Not Translated |          |
| [Italian](i18n/it.json)    | ❔ Incomplete      | zomboide |
| Japanese                   | ❌️ Not Translated |          |
| [Korean](i18n/ko.json)     | ✔️ Complete       | wally232 |
| [Portuguese](i18n/pt.json) | ✔️ Complete       | Aulberon |
| Russian                    | ❌️ Not Translated |          |
| Spanish                    | ❌️ Not Translated |          |
| Turkish                    | ❌️ Not Translated |          |
