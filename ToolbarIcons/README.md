# Toolbar Icons

Framework for adding icons to the toolbar.

* [API](#api)
* [Assets](#assets)

#### API

Add toolbar icons using the [Toolbar Icons API](../Common/Integrations/ToolbarIcons/IToolbarIconsApi.cs).

#### Assets

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