# Toolbar Icons

Framework for adding icons to the toolbar.

* [API](#api)
* [Assets](#assets)
* [Integrations](#integrations)

## API

Add toolbar icons using the [Toolbar Icons API](../Common/Integrations/ToolbarIcons/IToolbarIconsApi.cs).

## Assets

Integration is possible via data paths using
[SMAPI](https://stardewvalleywiki.com/Modding:Modder_Guide/APIs/Content#Edit_a_game_asset) or
[Content Patcher](https://github.com/Pathoschild/StardewMods/blob/develop/ContentPatcher/docs/author-guide.md).

`furyx639.FuryCore\\Toolbar`

Sample `content.json`:

```jsonc
{
  "Format": "1.24.0",
  "Changes": [
    // Load Texture Icons
    {
      "Action": "Load",
      "Target": "example.ModId/Icons",
      "FromFile": "assets/icon.png"
    },

    // Add Icon to launch Chests Anywhere
    {
      "Action": "EditData",
      "Target": "furyx639.FuryCore/Toolbar",
      "Entries": {
        "Chests Anywhere": "{{i18n: icon.chests-anywhere.name}}/example.ModId\\Icons/0/Left/keybind: B",
      },
      "When": {
        "HasMod": "Pathoschild.ChestsAnywhere"
      }
    },
  ]
}
```

## Integrations

Some mods are directly integrated which means icons are automatically added for them.

### Supported mods

* [Always Scroll Map](https://www.nexusmods.com/stardewvalley/mods/2733)
* [Chests Anywhere](https://www.nexusmods.com/stardewvalley/mods/518)
* [CJB Cheats Menu](https://www.nexusmods.com/stardewvalley/mods/4)
* [CJB Item Spawner](https://www.nexusmods.com/stardewvalley/mods/93)
* [Dynamic Game Assets](https://www.nexusmods.com/stardewvalley/mods/9365)
* [Instant Buildings From Farm](https://www.nexusmods.com/stardewvalley/mods/2070)
* [Lookup Anything](https://www.nexusmods.com/stardewvalley/mods/541)
* [Reset Terrain Features for .NET 5](https://www.nexusmods.com/stardewvalley/mods/9350)
* Stardew Aquarium

## Translations

| Language               | Status            | Credits  |
|:-----------------------|:------------------|:---------|
| Chinese                | ❌️ Not Translated |          |
| French                 | ❌️ Not Translated |          |
| German                 | ❌️ Not Translated |          |
| Hungarian              | ❌️ Not Translated |          |
| Italian                | ❌️ Not Translated |          |
| Japanese               | ❌️ Not Translated |          |
| [Korean](i18n/ko.json) | ✔️ Complete       | wally232 |
| Portuguese]            | ❌️ Not Translated |          |
| Russian                | ❌️ Not Translated |          |
| Spanish                | ❌️ Not Translated |          |
| Turkish                | ❌️ Not Translated |          |