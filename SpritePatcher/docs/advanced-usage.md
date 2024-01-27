## Advanced Usage

* [Fields](#fields)
* [Helper](#helper)
* [State](#state)
* [Animating](#animating)
* [Expanding](#expanding)
* [Scaling](#scaling)
* [Tinting](#tinting)
* [Updating](#updating)

## Fields

| name          | description                                                    |
|---------------|----------------------------------------------------------------|
| `Id`          | The unique identifier for the mod.                             |
| `ContentPack` | The content pack associated with the mod.                      |
| `Target`      | The target sprite sheet being patched.                         |
| `SourceArea`  | The source rectangle of the sprite sheet being patched.        |
| `DrawMethods` | The draw methods where the patch will be applied.              |
| `PatchMode`   | The mode that the patch will be applied.                       |
| `Texture`     | The raw texture data of the patch.                             |
| `Area`        | The area of the patch's texture that will be used.             |
| `Tint`        | Any tinting that will be applied to the patch's texture.       |
| `Alpha`       | The alpha that will be applied to the patch's texture.         |
| `Scale`       | How the patch will be scaled relative to the original texture. |
| `Frames`      | How many animation frames the texture has.                     |
| `Animate`     | How fast an animated sprite will cycle through it's frames.    |
| `Offset`      | An offset that determines where the patch will be drawn to.    |

## Helper

The mod provides Helper methods to assist with common tasks used for patches.

You can view the [source](../Framework/Interfaces/IPatchHelper.cs) for more information, or
refer to the table below:

| method                                                                                              | description                                                                                                                     |
|-----------------------------------------------------------------------------------------------------|---------------------------------------------------------------------------------------------------------------------------------|
| `Log(string message)`                                                                               | Send a message to SMAPI's console for debugging or information purposes.                                                        |
| `InvalidateCacheOnChanged(object field, string eventName)`                                          | Causes all patches to be regenerated whenever a NetField event is triggered.                                                    |
| `GetIndexFromString(string input, string value, char separator = ',')`                              | Splits a string out by `separator`, and then find the index of `value` in that split string.                                    |
| `SetAnimation(Animate animate, int frames)`                                                         | Set the animation and frames.                                                                                                   |
| `SetTexture(ParsedItemData data, float scale = 1f)`                                                 | Set the texture and area of a patch to an item based on its data.                                                               |
| `SetTexture(string path, int index = 0, int width = 16, int height = 16, float scale = 1f)`         | Set the patch to a texture in the mod's folder, optionally for the given index based on a given width and height of the sprite. |
| `WithHeldObject(IHaveModData entity, Action<SObject, ParsedItemData> action, bool monitor = false)` | Perform an action on an object's heldObject if it has one.                                                                      |
| `WithLastInputItem(IHaveModData entity, Action<Item, ParsedItemData> action, bool monitor = false)` | Perform an action on an object's lastInputItem if it has one.                                                                   | 
| `WithPreserve(IHaveModData entity, Action<SObject, ParsedItemData> action, bool monitor = false)`   | Perform an action on an object's preserve if it has one.                                                                        | 

### Sample Code

```js
WithPreserve(entity, preserve => {
    var index = GetIndexFromString(`{{Flowers}}`, preserve.InternalName);
    SetTexture(`{{Honey}}`, index);
});
```

This will split a string of Flower names by space and then find the index of the
flower that matches the honey's preserve type.  
Then it will assign the texture of the honey to the texture of the flower at
that index.

## State

You can save and access state for an `entity` by using it's `modData` field.
It's recommended that you prefix any keys with your mod's unique identifier to
prevent a collision.

### Sample Code

```js
// Storing state
entity.modData[`{{ModId}}.myKey`] = `myValue`;

// Retrieving state
if (entity.modData.TryGetValue(`{{ModId}}.myKey`, out var value)) {
    // Do something with value
}
```

## Animating

Textures can be animated by specifying an area that includes multiple frames,
and assigning values to the `Animate` and `Frames` fields.

As the texture is drawn, it will cycle through the frames at a given rate.

![Frames of an animated sprite](screenshots/advanced-usage-animating.png)

### Sample Code

```js
SetTexture(`assets/animated.png`);
SetAnimation(Animate.Medium, 4);
Area = new Rectangle(0, 0, 96, 17);
```

For Animate, you have the following choices:

| name             | description                          |
|------------------|--------------------------------------|
| `Animate.None`   | The default value is a static frame. |
| `Animate.Fast`   | Roughly 4 frames per second.         |
| `Animate.Medium` | Roughly 2 frames per second.         |
| `Animate.Slow`   | Roughly 1 frame per second.          |

It's recommended that if you use `Animate.Fast`, you have `Frames` that are a
multiple of 4. If you use `Animate.Medium`, use an even number of `Frames`. this
helps to optimize when multiple layers are animated at different rates.

## Expanding

The patch can expand the area of the original sprite, given one of the following
conditions are met:

* The `Offset` field has a negative X or Y value.
* The `Area` multiplied by `Scale` is larger than the original sprite.

By playing around with these fields, you can draw to the top, left, right,
and/or bottom of the original sprite.

![An expanded sprite](screenshots/advanced-usage-expanding.png)

### Sample Code

```js
SetTexture(`assets/expanded.png`);
Area = new Rectangle(0, 0, 24, 32);
Offset = new Vector2(-4, 0);
```

The furnace is a 16x32 sprite, but the patch is 20x32 and has an offset of -4.
This will result in 4 extra pixels to the left of the furnace and 4 extra pixels
to the right of the furnace.

## Scaling

You can apply higher definition textures to the original sprite by scaling the
patch's texture.

![A sprite with an HD patch](screenshots/advanced-usage-scaling.png)

### Sample Code

```js
SetTexture(`assets/scaled.png`, 0, 128, 280);
Offset = new Vector2(0, -3);
Scale = 0.125f;
```

The 128x280 texture is added as an overlay to a 16x32 sprite. It is offset 3
pixels up from the original sprite, and scaled at 1/8th the size of the original
sprite.

## Tinting

Your patch can dynamically change the color of the sprite by assigning a value
to `Tint`.

### Sample Code

```js
if (entity is not Crop crop) return;
SetTexture(`assets/tinted.png`);
Tint = crop.tintColor.Value;
```

This will tint the texture to match the crop's current tint color.

## Updating

For optimization purposes, textures are cached and only regenerated either when
the patch itself is changed, or in response to a NetField event.

To find an event to trigger the regeneration, you may need
to [decompile the game code](https://stardewvalleywiki.com/Modding:Modder_Guide/Get_Started#How_do_I_decompile_the_game_code.3F).
A lot of objects have a method `initNetFields` where all of it's net fields are
initialized.

### Sample Code

```js
// Automatic updates when held object changes
if (entity is not SObject { bigCraftable.Value: true } obj) return;
InvalidateCacheOnChanged(obj.heldObject, `fieldChangeVisibleEvent`);
if (obj.heldObject.Value == null || obj.lastInputItem.Value == null) return;
var item = ItemRegistry.GetDataOrErrorItem(obj.lastInputItem.Value.QualifiedItemId);
SetTexture(item, scale: 0.5f);
```

The `InvalidateCacheOnChanged` method will cause the patch to be regenerated
every time another item is placed into or removed from the big craftable.

```js
// Automatic updates using a Helper method.
WithHeldObject(entity,
    monitor: true,
    action: (obj, data) => SetTexture(data, scale: 0.5f));
```

Alternatively, some helper methods include a `monitor` parameter that will
handle the event for you. The code above does the exact same thing as the
previous example.