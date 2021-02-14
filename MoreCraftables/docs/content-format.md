## Content Format

### Overview

A More Craftables content pack must contain the following files:

- `manifest.json`
- `content-pack.json`
- `more-craftables.json`

Additionally, objects must be added to this folder in the [Json Assets](https://www.nexusmods.com/stardewvalley/mods/1720) Object and/or Big Craftable format:

- `BigCraftables\Craftable Name\big-craftable.json`
- `BigCraftables\Craftable Name\big-craftable.png`
- `Objects\Object Name\object.json`
- `Objects\Object Name\object.png`

**`manifest.json` must specify this is a content pack for More Craftables:**

```json
"ContentPackFor": {
  "UniqueID": "furyx639.MoreCraftables"
}
```

**`content-pack.json` is similar to `manifest.json` but only requires certain fields:**

```json
{
  "UniqueID": "Author.ModName",
  "Name": "Mod Name",
  "Description": "A description of this mod",
  "Author": "Your Name",
  "Version": "1.0.0",
  "UpdateKeys": []
}
```

For full details of `manifest.json` refer to [Modding:Modder Guide/APIs/Manifest](https://stardewcommunitywiki.com/Modding:Modder_Guide/APIs/Manifest).

**`more-craftables.json` must list all objects that will be added, their `Type`, and `Properties`:**

```json
{
  "My Fence": {
    "Type": "Fence"
  },
  "My Cask": {
    "Type": "Cask"
  }
}
```

For full details of `expanded-storage.json` see [More Craftables](#more-craftables).

### More Craftables

More Craftable objects are loaded into the game using [Json Assets](https://www.nexusmods.com/stardewvalley/mods/1720).
It's also possible to load objects using the [More Craftables API]().

Each Type of object has it's own set of properties that can be included to customize them.

**Support Big Craftables**

- [Cask](#cask)
- [Chest](#chest)

**Supported Objects**

- [Fence](#fence)

**Supported Furniture**

-

### Big Craftables

#### Cask

```json
{
  "Type": "Cask",
  "Properties": {
  }
}
```

#### Chest

```json
{
  "Type": "Chest",
  "Properties": {
  }
}
```

### Objects

#### Fence

```json
{
  "Type": "Fence",
  "Properties": {
  }
}
```

### Furniture
