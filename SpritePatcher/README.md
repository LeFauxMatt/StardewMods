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

| name        | type       | description                                                                                      |
|:------------|:-----------|:-------------------------------------------------------------------------------------------------|
| Target      | string     | The sprite sheet that the patch applies to.                                                      |
| Area        | Rectangle? | The area of the sprite sheet that the patch applies to.                                          |
| DrawMethods | string[]   | The draw methods that the patch applies to.                                                      |
| PatchMode   | string?    | The mode that the patch uses.                                                                    |
| Priority    | int?       | Determines the order in which the patch will be applied.                                         |
| Code        | string     | The code containing logic for whether the patch applies, and if so what texture and area to use. |

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

### Code

The `Code` field is C# code which returns `true` if the patch should be applied;
otherwise, it should return false. When it returns true, it should additionally
assign `Path` which is the texture to load and may also include `Area`
and `Tint`.

Example:

```js
{
    "Code": "
    if (entity is not SObject obj) return false;
    if (obj.preserve.Value != SObject.PreserveType.Honey || obj.preservedParentSheetIndex.Value == null) return false;
    this.Path = `{{InternalAssetKey: assets/honey.png}}`;
    return true;
    "
}
```

In the example above, the texture will only apply to Honey. When the item has a
preserve of Honey, the texture will be patched with the image
in `assets/honey.png`.

Code examples are shared in the documentation for each object type.