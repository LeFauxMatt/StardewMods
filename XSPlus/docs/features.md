## eXpanded Storage Features

### Features

* [Access Carried](#access-carried)
* [Capacity](#capacity)
* [Categorize Chest](#categorize-chest)
* [Color Picker](#color-picker)
* [Craft From Chest](#craft-from-chest)
* [Expanded Menu](#expanded-menu)
* [Filter Items](#filter-items)
* [Inventory Tabs](#inventory-tabs)
* [Search Items](#search-items)
* [Stash to Chest](#stash-to-chest)
* [Unbreakable](#unbreakable)
* [Unplaceable](#unplaceable)
* [Vacuum Items](#vacuum-items)

feature         | parameter(s)
----------------|--------------
AccessCarried   | null
CraftFromChest  | Distance <sup>[1](#distance)</sup>
ExpandedMenu    | Rows <sup>[2](#rows)</sup>
FilterItems     | Filters <sup>[3](#filters)</sup>
InventoryTabs   | Tabs <sup>[4](#tabs)</sup>
SearchItems     | null
StashToChest    | Distance <sup>[1](#distance)</sup>
Unbreakable     | null
Unplaceable     | null
VacuumItems     | null

<span id="distance">1.</span> Distance - Inventory, Location, or World.  
<span id="rows">2.</span> Rows - Max rows to show on expanded menu.  
<span id="filters">3.</span> Filters - List of items/tags to allow or block.  
<span id="tabs">4.</span> Tabs - List of tabs to show.

### Access Carried

Allows chest inventory to be accessed while held.

### Capacity

Allows chest to hold additional items.

### Categorize Chest

Assign categories to chests used with the stash to chest feature, and with Automate to only accept items that are part
of the chest's assigned categories.

### Color Picker

Adds a HSL-based color picker to the chest menu.

### Craft From Chest

Allows chest to be crafted from when the crafting hotkey is pressed.

#### Parameter(s)

Distance - The distance to the player that the chest's inventory will be accessible to the player.

Distance    | Description
------------|-------------
Inventory   | Accessible when player is carrying the chest.
Location    | Accessible when chest is placed in same location as the player.
World       | Accessible anywhere the player has access to.

### Expanded Menu

Enables resized chest inventory menu, and scrolling items.

#### Parameter(s)

Rows - Menu will expand up to a maximum number of rows from 3 (vanilla) up to 6.

### Filter Items

Restricts what items can be added to the
chest [context tags](https://github.com/ImJustMatt/StardewMods/blob/develop/XSLite/docs/content-pack.md#context-tags).

#### Parameter(s)

Filters - What items are allowed/blocked.

```json
{
  "ItemName": true,
  "AnotherItem": false
}
```

`false` means the item will be blocked, while `true` means the item is allowed.

If there are no allowed items specified, then all items are allowed by default.  
If there are no blocked items specified, then no items are blocked by default.

### Inventory Tabs

Adds tabs to chest inventory menu for quick filtering by category. Tabs can be customized in XSPlus by editing
tabs.json, by default these are:

#### Parameter(s)

```json
[
  "Clothing",
  "Cooking",
  "Crops",
  "Equipment",
  "Fish",
  "Minerals",
  "Misc",
  "Seeds"
]
```

Tabs in tabs.json filter items based
on [context tags](https://github.com/ImJustMatt/StardewMods/blob/develop/XSLite/docs/content-pack.md#context-tags).

### Search Items

Adds a search bar to find items by name or context tag.

### Stash to Chest

Distance - The distance to the player that the chest's inventory will be accessible to the player.

Distance    | Description
------------|-------------
Inventory   | Accessible when player is carrying the chest.
Location    | Accessible when chest is placed in same location as the player.
World       | Accessible anywhere the player has access to.

### Unbreakable

Prevent chest from breaking when hit by the player.

### Unplaceable

Prevent chest from being placed anywhere in the world.

### Vacuum Items

Causes dropped items (debris) to get pulled directly into chest while beind held by the player.