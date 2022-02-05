# More Chests

Allows custom items to be created as chests.

## Contents
* [Usage](#usage)
  * [API](#api)
  * [Data Model](#data-model)
    * [Chest Attributes](#chest-attributes)
    * [Animation Frames](#animation-frames)
* [Translations](#translations)

## Usage

### API

The API method ensures that any Item added to the game will be instantiated as
a Chest.

### Data Model

Similar to the API method, you can also register custom chests by editing the
content data.

`furyx639.MoreChests/Chests`

Sample `content.json`:

```jsonc
{
  "Format": "1.24.0",
  "Changes": [
    // Add new item to the game 
    {
        "Action": "EditData",
        "Target": "Data/BigCraftablesInformation",
        "Entries": {
            "Example.Mod_NewChest": "New Chest/0/-300/Crafting -9/{{i18n: new-chest.description}}/true/true/0//{{i18n: new-chest.name}}/0/Example.Mod\\NewChest",
            "Example.Mod_NewChest": "Cardboard Box/0/-300/Crafting -9/{{i18n: cardboard-box.description}}/true/true/0//{{i18n: cardboard-box.name}}/0/Example.Mod\\CardboardBox",
        }
    },
    
    // Load textures for new items
    {
      "Action": "Load",
      "Target": "example.ModId/NewChest, example.ModId/CardboardBox",
      "FromFile": "assets/{{TargetWithoutPath}}j.png",
    },

    // Register items as chests with the More Chests mod
    {
      "Action": "EditData",
      "Target": "furyx639.MoreChests\\Chests",
      "Entries": {
        "Example.Mod_NewChest": "16/32/0/0/doorCreak/doorCreakReverse",
        "Example.Mod_CardboardBox": "16/32/0..4/5..9/doorCreak/doorCreakReverse",
      }
    },
  ]
}
```

For more on adding custom items to the game, see the [modding docs](https://stardewvalleywiki.com/Modding:Migrate_to_Stardew_Valley_1.6#Custom_items).

#### Chest Attributes

The More Chests data looks for a key matching the items qualified item id, and the following fields split out by a forward slash:

| Field             | Description                                                       |
|:------------------|:------------------------------------------------------------------|
| Width             | The pixel width of the Chest. Vanilla chests are 16 pixels wide.  |
| Height            | The pixel height of the Chest. Vanilla chests are 32 pixels wide. |
| Opening Animation | The animation frames to play while the chest is opening.          |
| Closing Animation | The animation frames to play while the chest is opening.          |
| Opening Sound     | The sound to play while the chest is opening.                     |
| Closing Sound     | The sound to play while the chest is closing.                     |

#### Animation Frames

Animation frames represent a sprite index to draw from the Chests texture.

The syntax for an animated sequence allows you to specify frames individually,
or as a consecutive sequence of frames using the spread syntax.

The frame refers to a part of the Chest texture starting at the top-left, whose
width and height match the Chest dimensions, moves over to the right until
it reaches the end (or the remaining portion is not enough to make a full-sized
sprite), and then proceeds down to the next row.

Frames start at index 0 and refer to any area of the overall Texture as long as
it can produce a cell that matches the width/height of the Chest.

| 16px | 16px | 16px | 16px |
|:----:|:----:|:----:|:----:|
|  0   |  1   |  2   |  3   |
|  4   |  5   |  6   |  7   |
|  8   |  9   |  10  |  11  |

Note - The texture should have the first cell (index 0) represent the Chest at
rest. (i.e. no animation is playing).

For example:

`1,2,3` would result in the 2nd through 4th frames of the texture to be drawn.

`1..3` is the same as above, using the spread syntax.

These can be mixed and matched:

`1,3..5,2` would draw the 2nd frame, followed by the 4th through 6th, and finally the 3rd.

## Translations

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