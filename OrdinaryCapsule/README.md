# Ordinary Capsule

Nothing strange about this capsule.

## Contents

* [Configurations](#configurations)
* [Customization](#customization)
* [Translations](#translations)

## Configurations

For ease of use, it is recommended to set config options
from [Generic Mod Config Menu](https://www.nexusmods.com/stardewvalley/mods/5098).

| Config              | Description                                       | Default Value | Other Value(s)    |
|:--------------------|:--------------------------------------------------|---------------|:------------------|
| UnlockAutomatically | Automatically unlock the Ordinary Capsule recipe. | false         | `true` or `false` |

## Customization

Integration with Ordinary Capsule is possible
using [Content Patcher](https://github.com/Pathoschild/StardewMods/blob/develop/ContentPatcher/docs/author-guide.md).

### Production Times

Modify production times for supported items.

**Target** `furyx639.OrdinaryCapsule/ProductionTime`

```jsonc
{
  "Format": "1.25.0",
  "Changes": [
    {
      "Action": "EditData",
      "Target": "furyx639.OrdinaryCapsule/ProductionTime",
      "Entries": {
        // Make Truffle production time 60 minutes
        "item_truffle": "60"
      },
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