# Sprite Patcher

Patch sprites based on the object's attributes.

# Contents

* [Key Concepts](#key-concepts)
* [Object Types](#object-types)
* [Main Fields](#main-fields)
    * [Target](#target)
    * [Draw Methods](#draw-methods)
    * [Patch Mode](#patch-mode)
    * [Priority](#priority)
    * [Tokens](#tokens)
    * [Textures](#textures)
* [Advanced Usage](#advanced-usage)

## Key Concepts

Every supported object type is a direct reference to a Stardew Valley class that
implements IModData and has one or more draw methods.

In layman's terms, this means that most objects in the game can have their
sprites patched by this mod.

When you want to patch an object, you must know a few things about it:

* The SpriteSheet that contains the sprite you want to patch
* The source rectangle of the sprite you want to patch
* The attributes of the object that determine which patches you want to apply

The attributes can be determined by looking at the object's class definition in
the decompiled game code, but I'll include examples of commonly used attributes
in the linked documentation.

## Object Types

* [Buildings](docs/PatchBuildings.md)
* [Characters](docs/PatchCharacters.md)
* [Items](docs/PatchItems.md)
* [Terrain Features](docs/PatchTerrainFeatures.md)
* [Tools](docs/PatchTools.md)

## Main Fields

Refer to the [sample content pack](../SpritePatcherCP) for examples of how to
use these fields.

| name        | type      | description                                                                                                                |
|:------------|:----------|:---------------------------------------------------------------------------------------------------------------------------|
| Id          | string    | A unique string ID for this patch.                                                                                         |
| Target      | string    | The sprite sheet that the patch applies to.                                                                                |
| DrawMethods | string[]  | The draw methods that the patch applies to.                                                                                |
| PatchMode   | string    | The mode that the patch uses.                                                                                              |
| Priority    | int       | Determines the order in which the patch will be applied.                                                                   |
| Tokens      | Token{}   | Defines the attributes that will be used in determining which texture to apply and can be used to define the texture path. |
| Textures    | Texture[] | Defines the textures that can be applied for the patch.                                                                    |

### Target

Examples for common targets can be found in the documentation for each object
type.

### Draw Methods

Each object type supports one or more draw methods. The draw methods that are
supported for each object type can be found in the documentation for that object
type.

| name         | description                                             |
|--------------|---------------------------------------------------------|
| Menu         | Called when an object is being drawn in a menu.         |
| Held         | Called when an object is being held by the player.      |
| World        | Called when an object is being drawn in the game world. |
| Background   | Called for drawing underneath buildings.                |
| Construction | Called while a building is under construction.          |
| Shadow       | Called for drawing a shadow under the object.           |

### Patch Mode

The patch mode affects how the patch will be drawn over the base texture as well
as other patches of a lower priority.

| name    | description                                                               |
|---------|---------------------------------------------------------------------------|
| Overlay | Only non-transparent pixels will be drawn over the previous texture.      |
| Replace | The previous texture will be completely overwritten with the new texture. |

### Priority

Priority can be any number and the patches will be applied in order of greatest
to least.

### Tokens

The `Tokens` field allows you to define the object's attributes, and map it's
values to names that can be used in the `Textures` field.

Example:

```json
{
    "Tokens": {
        "Item": {
            "RefersTo": "preservedParentSheetIndex.Value",
            "Map": {
                "Blueberry": "258",
                "Apple": "613"
            }
        },
        "Type": {
            "RefersTo": "preserve.Value"
        }
    }
}
```

In the example above, two tokens are being defined - Item and Type.

The Item token refers to a property of the object
called `preservedParentSheetIndex.Value`, and maps the values of that property
to Blueberry and Apple.

The Type token refers to a property of the object called preserve.Value, and
maps the values of that property to the names such as Jelly, Pickle, and Wine.

#### Token

Each token must include a RefersTo field, and optionally may include a Map:

| name     | type     | description                                                                  |
|----------|----------|------------------------------------------------------------------------------|
| RefersTo | string   | A pattern for describing an attribute that can be accessed from the object.  |
| Map      | string{} | A list of values and the condition that must be true for the value to apply. |

Token will take on only one of the Map values at any given time for an object.
If Map is not defined, then the actual value of the attribute will be used.

See [Advanced Usage](#advanced-usage) for more information on how to use
the `Map` field.

You'll see how those are used in the next section.

### Textures

The `Textures` field is a list of conditional textures for the patch.
Each `Texture` has its own set of conditions, and only the first one that
matches will be applied.

Example:

```json
{
    "Textures": [
        {
            "Path": "{{ModId}}/{Type}",
            "Conditions": {
                "Type": "Jelly,Wine",
                "Item": "Blueberry"
            },
            "Tint": {
                "R": 6,
                "G": 59,
                "B": 206
            },
            "FromArea": {
                "X": 0,
                "Y": 0,
                "Width": 16,
                "Height": 16
            }
        },
        {
            "Path": "{{ModId}}/{Type}",
            "Conditions": {
                "Type": "Jelly,Wine",
                "Item": "Apple"
            },
            "Tint": {
                "R": 191,
                "G": 0,
                "B": 0
            },
            "FromArea": {
                "X": 0,
                "Y": 0,
                "Width": 16,
                "Height": 16
            }
        },
        {
            "Path": "{{ModId}}/{Type}",
            "Conditions": {
                "Type": "Jelly,Wine"
            },
            "FromArea": {
                "X": 16,
                "Y": 0,
                "Width": 16,
                "Height": 16
            }
        }
    ]
}

```

You can see that `Textures` uses the `Item` and `Type` tokens that were defined
earlier.

If the item is a Blueberry Jelly/Wine, then the first texture will be applied
which will load from the path `{{ModId}}/Jelly` or `{{ModId}}/Wine` depending on
the object's type.
It will use the 16x16 area in the top-left of the SpriteSheet and it will be
tinted blue.

If the item is an Apple Jelly/Wine, then the second texture will be applied,
similar to the first, but it will be tinted red.

Finally, if the item is any other type of Wine or Jelly, then the third texture
will be applied, it will not be tinted, and it will pull from a different area
of the SpriteSheet.

#### Texture

Each texture requires a Path, and optionally may include Conditions, Tint, and
FromArea.

| name       | type        | description                                     |
|------------|-------------|-------------------------------------------------|
| Path       | string      | The game path to load the texture from.         |
| Conditions | string{}    | The conditions for when the texture will apply. |
| Tint       | `Color`     | A Color to tint the texture to.                 |
| FromArea   | `Rectangle` | The source area of the texture.                 |

Token names may be used in the Path and Conditions field by surrounding it with
curly braces. The actual value or mapped value will replace the token.

## Advanced Usage

### Map

The map field contains a friendly name as the key, and an expression as the
value. The simplest map is just a value you want to compare the attribute
against. In this form, the token will be assigned the friendly name if the value
matches.

```json
{
    "Tokens": {
        "Quality": {
            "RefersTo": "Quality",
            "Map": {
                "None": "0",
                "Silver": "1",
                "Gold": "2",
                "Iridium": "4"
            }
        }
    }
}
```

Quality will be None if the object's quality property is 0, Silver if it's 1,
and so on.

In addition to simple values, it's sometimes necessary to make more complex
comparisons for the attribute. For example, if you want to check how full a
Chest is, you would need to use an expression:

```json
{
    "Tokens": {
        "Capacity": {
            "RefersTo": "GetItemsForPlayer().Count",
            "Map": {
                "Empty": "0",
                "Half": "<= 18",
                "Filled": "<= 36"
            }
        }
    }
}
```

Another example is if the attribute you're dealing with is a list of objects, in
that case the expression applies to all items and if any are true then the token
will be assigned the friendly name:

```json
{
    "Tokens": {
        "Animals": {
            "RefersTo": "GetIndoors().getAllFarmAnimals()",
            "Map": {
                "Cow": "type.Value =~ Cow"
            }
        }
    }
}
```

If any of the building's farm animals have a type.Value which contains Cow, then
the token `Animals` will be assigned the friendly name `Cow`.