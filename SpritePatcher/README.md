# Sprite Patcher

Sprite Patcher is a framework mod for Stardew Valley that allows content packs
to apply texture patches to just about anything in the game based on that
object's individual attributes.

See [Templates](#templates) below for some pre-made examples of how this mod can
be used, and for an easy way to get started using this mod to create your own
content for it.

The patches are applied as layers, so multiple patches can affect the same
object.

Additionally, it is possible for patches to extend textures in the following
ways:

* Increase the resolution of textures.
* Apply tinting to textures.
* Load vanilla textures on top of other textures.
* Animate textures.
* Expand textures beyond the original sprite.

## Contents

* [Key Concepts](#key-concepts)
* [Main Fields](#main-fields)
    * [Sprite Sheet](#sprite-sheet)
    * [Area](#area)
    * [Draw Methods](#draw-methods)
    * [Patch Mode](#patch-mode)
    * [Code](#code)
    * [Priority](#priority)

## Key Concepts

These are the fields that can be defined for a patch:

| name                          | description                                                               |
|-------------------------------|---------------------------------------------------------------------------|
| [Sprite Sheet](#sprite-sheet) | The texture that you want to apply patches to.                            |
| [Area](#area)                 | The area of the sprite sheet.                                             |
| [Draw Methods](#draw-methods) | Which draw methods the patches are applied to.                            |
| [Patch Mode](#patch-mode)     | Determines how the patch will be applied.                                 |
| [Code](#code)                 | This is where you can apply conditional logic and apply the actual patch. |
| [Priority](#priority)         | Determines the order in which patches are applied to the same target.     |

I'll go into each of these in more details below as well as link to additional
documentation for specific examples.

## Main Fields

### Sprite Sheet

[Unpack the game's content](https://stardewvalleywiki.com/Modding:Editing_XNB_files#Unpack_game_files),
to browse vanilla sprite sheets that can be targeted for patches. Additionally,
some mods have support for editing their content which sometimes is documented
on the respective mod's page
on [Nexus Mods](https://www.nexusmods.com/stardewvalley).

Some commonly targeted sprite sheets are:

<table>
<thead>
<th>name</th>
<th>path</th>
</thead>
<tr>
<td>

[Animals](docs/characters.md#farmanimal)

</td>
<td>

`Animals/[FarmAnimal]`

</td>
</tr>
<tr>
<td>

[Buildings](docs/buildings.md)

</td>
<td>

`Buildings/[Building]`

</td>
</tr>
<tr>
<td>

[Characters](docs/characters.md)

</td>
<td>

`Characters/[Character]`

</td>
</tr>
<tr>
<td>

[Craftables](docs/items.md#craftables)

</td>
<td>

`TileSheets/[Craftables]`

</td>
</tr>
<tr>
<td>

[Crops](docs/crops.md)

</td>
<td>

`TileSheets/crops`

</td>
</tr>
<tr>
<td>

[Fruit Trees](docs/terrain-features.md#fruittree)

</td>
<td>

`TileSheets/fruitTrees`

</td>
</tr>
<tr>
<td>

[Furniture](docs/items.md#furniture)

</td>
<td>

`TileSheets/furniture`

</td>
</tr>
<tr>
<td>

[Grass](docs/terrain-features.md#grass)

</td>
<td>

`TerrainFeatures/grass`

</td>
</tr>
<tr>
<td>

[Items](docs/items.md)

</td>
<td>

`Maps/springobjects`

</td>
</tr>
<tr>
<td>

[Monsters](docs/characters.md#monster)

</td>
<td>

`Characters/Monsters/[Monster]`

</td>
</tr>
<tr>
<td>

[Tools](docs/tools.md)

</td>
<td>

`TileSheets/tools`

</td>
</tr>
<tr>
<td>

[Trees](docs/terrain-features.md#tree)

</td>
<td>

`TerrainFeatures/[Tree]`

</td>
</tr>
<tr>
<td>

[Weapons](docs/tools.md#weapons)

</td>
<td>

`TileSheets/weapons`

</td>
</tr>
<tr>
<td>

[UI](docs/ui.md)

</td>
<td>

`LooseSprites/Cursors`

</td>
</tr>
</table>

### Area

The area is the specific part of the sprite sheet that you want to edit. You do
not have to target the entire sprite, you can target the specific area that you
want to make edits to. For example, if you want to add an overlay to the
top-right corner of a sprite, then you can target just that area.

The area is specified as a rectangle with the following format:

```json
{
    "X": 99,
    "Y": 49,
    "Width": 10,
    "Height": 9
}
```

To easily find the area of a sprite, I recommend using an image editor such
as [Paint.NET](https://www.getpaint.net/) which allows you to select an area and
see the coordinates of the selection.

![Find area using Paint.NET](docs/screenshots/find-area-using-paint-dot-net.png)

### Draw Methods

You can select any combination of draw methods to apply your patch to.

| name         | description                                      |
|--------------|--------------------------------------------------|
| Background   | Used for drawing the underlay of buildings.      |
| Construction | Used for buildings that are under construction.  |
| Held         | Used when an object is being held by the farmer. |
| Menu         | Used by objects being displayed in a menu.       |
| Shadow       | Used to draw a shadow texture beneath an object. |
| World        | Used for objects that are placed in the world.   |

The draw methods are specified as an array of strings:

```json
{
    "DrawMethods": [
        "Background",
        "Construction",
        "Held",
        "Menu",
        "Shadow",
        "World"
    ]
}
```

### Patch Mode

The patch mode affects how the patch will be drawn over the base texture as well
as other patches of a lower priority.

| name    | description                                                               |
|---------|---------------------------------------------------------------------------|
| Overlay | Only non-transparent pixels will be drawn over the previous texture.      |
| Replace | The previous texture will be completely overwritten with the new texture. |

### Code

This is where things can get advanced, but to help simplify things, there are
templates you can use to get started:

#### Templates

| name         | description                                                               |
|--------------|---------------------------------------------------------------------------|
| Crystalarium | Add an overlay to the Crystalarium depending on what mineral it produces. |
| Furnace      | Add an overlay to the Furnace depending on what is being smelted.         |
| Honey        | Replace the honey texture depending on the flower.                        |
| Jelly        | Replace the jelly texture depending on the fruit.                         |
| Juice        | Replace the juice texture depending on the vegetable.                     |
| Pickles      | Replace the pickle texture depending on the vegetable.                    |
| Quality      | Customize a texture depending on the object's quality.                    |
| Wine         | Replace the wine texture depending on the fruit.                          |

If you want to write your own code or template, then you can refer
to the [Advanced Usage](docs/advanced-usage.md) guide.

### Priority

Priority can be any number and the patches will be applied in order of greatest
to least.