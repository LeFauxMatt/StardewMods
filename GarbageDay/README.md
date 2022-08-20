# Garbage Day

Stardew Valley mod which turns garbage cans into storages. Trash will build up in them randomly throughout the week.
Once a week on garbage day, they'll be cleared out.

## Contents

* [Configurations](#configurations)
* [Customization](#customization)
* [Translations](#translations)
* [Credits](#credits)

## Configurations

For ease of use, it is recommended to set config options
from [Generic Mod Config Menu](https://www.nexusmods.com/stardewvalley/mods/5098).

| Config     | Description                      | Default Value | Other Value(s)       |
|:-----------|:---------------------------------|---------------|:---------------------|
| GarbageDay | The day that garbage is cleared. | `"Monday"`    | Any day of the week. |

## Customization

Integration with GarbageDay is possible using [Content Patcher](docs/mod-integration.md).

### Map

Add Garbage Cans to maps.

Map Property: `Action: Garbage [Which Can]`

`[Which Can]` needs a loot entry for items.

### Loot

Customize loot items for each Garbage Can.

**Target** `furyx639.GarbageDay/Loot`

TBD:

### Texture

Replace the Garbage Can texture.

**Target** `furyx639.GarbageDay/Texture`

```jsonc
{
  "Format": "1.25.0",
  "Changes": [
    {
      "Action": "EditImage",
      "Target": "furyx639.GarbageDay/Texture",
      "FromFile": "assets/MyTexture.png",
    },
  ]
}
```

## Translations

See [here](i18n/default.json) for the base translation file.

| Language   | Status            | Credits |
|:-----------|:------------------|:--------|
| Chinese    | ❌️ Not Translated |         |
| French     | ❌️ Not Translated |         |
| German     | ❌️ Not Translated |         |
| Hungarian  | ❌️ Not Translated |         |
| Italian    | ❌️ Not Translated |         |
| Japanese   | ❌️ Not Translated |         |
| Korean     | ❌️ Not Translated |         |
| Portuguese | ❌️ Not Translated |         |
| Russian    | ❌️ Not Translated |         |
| Spanish    | ❌️ Not Translated |         |
| Turkish    | ❌️ Not Translated |         |

# Credits

Item Repository code was sourced from [CJB Item Spawner](https://github.com/CJBok/SDV-Mods/tree/master/CJBItemSpawner).

Garbage Can spritesheet was created by 6480.