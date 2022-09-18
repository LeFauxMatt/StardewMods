# Toolbar Icons

Framework for adding icons to the toolbar.

* [API](#api)
* [Assets](#assets)
* [Integrations](#integrations)
* [Credits](#credits)

## API

Add toolbar icons using the [Toolbar Icons API](../Common/Integrations/ToolbarIcons/IToolbarIconsApi.cs).

## Assets

Integration is possible via data paths using
[SMAPI](https://stardewvalleywiki.com/Modding:Modder_Guide/APIs/Content#Edit_a_game_asset) or
[Content Patcher](https://github.com/Pathoschild/StardewMods/blob/develop/ContentPatcher/docs/author-guide.md).

`furyx639.ToolbarIcons/Toolbar`

Sample `content.json`:

```jsonc
{
  "Format": "1.24.0",
  "ConfigSchema": {
    "EventLookupHotkey": {
      "AllowBlank": true,
      "Default": "N"
    }
  },
  "Changes": [
    // Load Texture Icons
    {
      "Action": "Load",
      "Target": "example.ModId/Icons",
      "FromFile": "assets/icon.png"
    },

    // Add Event Lookup Icon using Keybind
    {
      "Action": "EditData",
      "Target": "furyx639.ToolbarIcons/Toolbar",
      "Entries": {
        "shekurika.EventLookup/LookupEvents": "{{i18n: button.EventLookup}}/example.ModId\\Icons/0/keybind/{{EventLookupHotkey}}"
      },
      "When": {
        "HasMod": "shekurika.EventLookup"
      }
    },

    // Add Lookup Anything Icon using method
    {
      "Action": "EditData",
      "Target": "furyx639.ToolbarIcons/Toolbar",
      "Entries": {
        "Pathoschild.LookupAnything/ToggleSearch": "{{i18n: button.LookupAnything}/example.ModId\\Icons/1/method/TryToggleSearch"
      },
      "When": {
        "HasMod": "Pathoschild.LookupAnything"
      }
    },

    // Replace texture for Stardew Aquarium icon
    {
      "Action": "EditImage",
      "Target": "furyx639.ToolbarIcons/Icons",
      "FromFile": "assets/aquarium-icon.png",
      "FromArea": {"X": 0, "Y": 0, "Width": 16, "Height": 16},
      "ToArea" {"X": 16, "Y": 0, "Width": 16, "Height": 16}
    }
  ]
}
```

The data entry is as follows:

| Entry                  | Description                                                     |
|:-----------------------|:----------------------------------------------------------------|
| Hover Text             | The text to display when hovering over an icon.<sup>1</sup>     |
| Texture Path           | Path to the icon texture.<sup>2</sup>                           |
| Texture Index          | The position of the texture for this icon.<sup>3</sup>          |
| Integration Type       | The type of action for this icon.<sup>4</sup>                   |
| Integration Params     | Additional parameters depending on the action type.<sup>5</sup> |                                                                           |

1. Preferably localized text describing the icon's action.
2. Path must be to a loaded texture asset.
3. Index goes from left to right for each 16x16 icon.
4. Supported actions are `method` or `keybind`.
5. Parameters depend on the action type:
    * `method` the method name, such as `TryToggleSearch`
    * `keybind` must include one or more buttons, such as `B`

## Integrations

Some integrations are handled directly by the Toolbar Icons mod which means icons are automatically added for them.

### Supported mods

* [Always Scroll Map](https://www.nexusmods.com/stardewvalley/mods/2733)
* [CJB Cheats Menu](https://www.nexusmods.com/stardewvalley/mods/4)
* [CJB Item Spawner](https://www.nexusmods.com/stardewvalley/mods/93)
* [Dynamic Game Assets](https://www.nexusmods.com/stardewvalley/mods/9365)
* [Generic Mod Config Menu](https://www.nexusmods.com/stardewvalley/mods/5098)
* [Stardew Aquarium](https://www.nexusmods.com/stardewvalley/mods/6372)
* [To-Dew](https://www.nexusmods.com/stardewvalley/mods/7409)

## Translations

| Language               | Status            | Credits  |
|:-----------------------|:------------------|:---------|
| Chinese                | ❌️ Not Translated |          |
| French                 | ❌️ Not Translated |          |
| German                 | ❌️ Not Translated |          |
| Hungarian              | ❌️ Not Translated |          |
| Italian                | ❌️ Not Translated |          |
| Japanese               | ❌️ Not Translated |          |
| [Korean](i18n/ko.json) | ❔ Incomplete      | wally232 |
| Portuguese]            | ❌️ Not Translated |          |
| Russian                | ❌️ Not Translated |          |
| Spanish                | ❌️ Not Translated |          |
| Turkish                | ❌️ Not Translated |          |

## Credits

Icons created by [Tai](https://www.nexusmods.com/stardewvalley/users/92060238):

* Always Scroll Map
* To-Dew
* Daily Quests
* Special Orders