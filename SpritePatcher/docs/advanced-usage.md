## Advanced Usage

* [Fields](#fields)
* [Helper](#helper)
* [Animating](#animating)
* [Expanding](#expanding)
* [Scaling](#scaling)
* [Tinting](#tinting)
* [Updating](#updating)

## Fields

| name            | description                                                        |
|-----------------|--------------------------------------------------------------------|
| `Id`            | The unique identifier for the mod.                                 |
| `ContentPack`   | The content pack associated with the mod.                          |
| `Target`        | The target sprite sheet being patched.                             |
| `SourceArea`    | The source rectangle of the sprite sheet being patched.            |
| `DrawMethods`   | The draw methods where the patch will be applied.                  |
| `PatchMode`     | The mode that the patch will be applied.                           |
| `Texture`       | The raw texture data of the patch.                                 |
| `Area`          | The area of the patch's texture that will be used.                 |
| `Tint`          | Any tinting that will be applied to the patch's texture.           |
| `Scale`         | How the patch will be scaled relative to the original texture.     |
| `Frames`        | How many animation frames the texture has.                         |
| `TicksPerFrame` | How many ticks will cycle the texture to the next animation frame. |
| `Offset`        | An offset that determines where the patch will be drawn to.        |

## Helper

The mod provides a `Helper` class with some useful methods for patching.

You can view the [source](../Framework/PatchHelper.cs) for more information, or
refer to the table below:

| method                                                                    | description                                                                                                                     |
|---------------------------------------------------------------------------|---------------------------------------------------------------------------------------------------------------------------------|
| `InvalidateCacheOnChanged(object field, string eventName)`                | Causes all patches to be regenerated whenever a NetField event is triggered.                                                    |
| `GetIndexFromString(string input, string value, char separator = ',')`    | Splits a string out by `separator`, and then find the index of `value` in that split string.                                    |
| `SetTexture(Texture2D texture)`                                           | Set the patch to the texture of a vanilla sprite sheet.                                                                         |
| `SetTexture(string path, int index = 0, int width = 16, int height = 16)` | Set the patch to a texture in the mod's folder, optionally for the given index based on a given width and height of the sprite. |
| `Log(string message)`                                                     | Send a message to SMAPI's console for debugging or information purposes.                                                        |

### Sample Code

```js
if (entity is not SObject obj) return;
if (obj.preserve.Value != SObject.PreserveType.Honey || obj.preservedParentSheetIndex.Value == null) return;
var preserve = ItemRegistry.GetDataOrErrorItem(`(O)` + obj.preservedParentSheetIndex.Value).InternalName;
var index = Helper.GetIndexFromString(`{{Flowers}}`, preserve);
Helper.SetTexture(`{{Honey}}`, index);
```

This will split a string of Flower names by space and then find the index of the
flower that matches the honey's preserve type.  
Then it will assign the texture of the honey to the texture of the flower at
that index.

## Animating

Textures can be animated by specifying an area that includes multiple frames,
and assigning values to the `Frames` and `TicksPerFrame` fields.

As the texture is drawn, it will cycle through the frames at the given rate.

### Sample Code

```js
Helper.SetTexture(`assets/animated.png`);
Area = new Rectangle(0, 0, 64, 16);
Frames = 4;
```

The default `TicksPerFrame` is 10, so this will cycle through the 4 frames of a
16x16 texture every 10 ticks. Which is about 2 frames per second.

## Expanding

The patch can expand the area of the original sprite, given one of the following
conditions are met:

* The `Offset` field has a negative X or Y value.
* The `Area` multiplied by `Scale` is larger than the original sprite.

By playing around with these fields, you can draw to the top, left, right,
and/or bottom of the original sprite.

### Sample Code

```js
Helper.SetTexture(`assets/expanded.png`);
Area = new Rectangle(0, 0, 20, 20);
Offset = new Vector2(-2, -2);
```

This will draw a 20x20 texture centered on a sprite which was originally 16x16.

## Scaling

You can apply higher definition textures to the original sprite by scaling the
patch's texture.

### Sample Code

```js
Helper.SetTexture(`assets/scaled.png`, 0, 32, 32);
Scale = 0.5f;
```

The 32x32 texture will drawn at half the size of the original 16x16 sprite. So
it occupies the same space, but will be double the resolution.

## Tinting

Your patch can dynamically change the color of the sprite by assigning a value
to `Tint`.

### Sample Code

```js
if (entity is not Crop crop) return;
Helper.SetTexture(`assets/tinted.png`);
Tint = crop.tintColor.Value;
```

This will tint the texture to match the crop's current tint color.

## Updating

For optimization purposes, textures are cached and only regenerated either when
the patch itself is changed, or in response to a NetField event.

To find an event to trigger the regeneration, you may need
to [decompile the game code](https://stardewvalleywiki.com/Modding:Modder_Guide/Get_Started#How_do_I_decompile_the_game_code.3F).

### Sample Code

```js
if (entity is not SObject { bigCraftable.Value: true } obj) return;
Helper.InvalidateCacheOnChanged(obj.heldObject, `fieldChangeVisibleEvent`);
if (obj.heldObject.Value == null || obj.lastInputItem.Value == null) return;
var item = ItemRegistry.GetDataOrErrorItem(obj.lastInputItem.Value.QualifiedItemId);
Helper.SetTexture(item.GetTexture());
Area = item.GetSourceRect();
Scale = 0.5f;
```

This will overlay the texture of the big craftable to match the texture of the
last item that was placed in it.

The `InvalidateCacheOnChanged` method will cause the patch to be regenerated
every time another item is placed into or removed from the big craftable.