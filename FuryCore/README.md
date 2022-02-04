# FuryCore

Provides additional APIs for my other mods.

## Contents

* Helpers
    * [Item Matcher](#item-matcher)
* Custom Events
    * [ItemGrabMenu Changed](#itemgrabmenu-changed)
    * [MenuComponent Pressed](#menucomponent-pressed)
    * [Rendering ItemGrabMenu](#rendering-itemgrabmenu)
    * [Rendered ItemGrabMenu](#rendered-itemgrabmenu)
* Services
    * [Fury Events](#fury-events)
    * [Harmony Helper](#harmony-helper)
    * [Menu Components](#menu-components)
    * [Menu Items](#menu-items)
    * [Mod Services](#mod-services)
* UI
    * [DropDown Menu](#dropdown-menu)
    * [Gradient Bar](#gradient-bar)
    * [HSL Color Picker](#hsl-color-picker)
    * [Item Selection Menu](#item-selection-menu)

### Helpers

#### Item Matcher

Stores a list of search phrases to test against an Item. It's able to make exact or partial searches and can use the
name or any of the item's context tags. Also adds some custom context tags for searching items based on if they are
furniture, an artifact, can be donated to a bundle, and/or can be donated to the museum.

[Source](Helpers/ItemMatcher.cs)

### Custom Events

#### ItemGrabMenu Changed

Triggered whenever an ItemGrabMenu is constructed, and whenever the Active Menu switches to/from an ItemGrabMenu.On
construction, this event triggers as a postfix to the vanilla ItemGrabMenu constructor so any changes made are before
the menu is displayed to the screen.

See [Fury Events](#fury-events)

[Source](Events/ItemGrabMenuChanged.cs)

#### MenuComponent Pressed

Triggers when a vanilla or custom component is pressed on an ItemGrabMenu.

See [Fury Events](#fury-events)

[Source](Events/MenuComponentPressed.cs)

#### Rendered ItemGrabMenu

Identical to RenderingActiveMenu except for it only triggers for ItemGrabMenu, and anything drawn to the SpriteBatch
will be above the background fade but below the actual menu graphics. Great for menu underlays.

See [Fury Events](#fury-events)

[Source](Events/RenderedItemGrabMenu.cs)

#### Rendering ItemGrabMenu

Identical to RenderedActiveMenu except for it only triggers for ItemGrabMenu, and anything drawn to the SpriteBatch will
be above the menu but below the cursor and any hover elements such as text or item.

See [Fury Events](#fury-events)

[Interface](Events/RenderingItemGrabMenu.cs)

### Services

#### Fury Events

[ [Interface](Interfaces/IFuryEvents.cs) | [Source](Services/CustomEvents.cs) ]

#### Harmony Helper

Saves a list of Harmony Patches, and allows them to be applied or reversed at any time.

[ [Interface](Interfaces/IHarmonyHelper.cs) | [Source](Services/HarmonyHelper.cs) ]

#### Menu Components

Add custom components to the ItemGrabMenu which can optionally automatically align to certain areas of the screen. In
this case neighboring components are automatically assigned for controller support.

[ [Interface](Interfaces/IMenuComponent.cs) | [Source](Services/MenuComponents.cs) ]

#### Menu Items

Allows displayed items to be handled separately from actual items. This enables support for such things as filtering
displayed items or scrolling an overflow of items without affecting the source inventory.

[ [Interface](Interfaces/IMenuItems.cs) | [Source](Services/MenuItems.cs) ]

#### Mod Services

All of FuryCores APIs are access through this service.

[ [Interface](Interfaces/IModService) | [Source](Services/ModServices.cs) ]

### UI

#### DropDown Menu

A simple menu that will display a list of string values, and calls an action on the selected value.

[Source](UI/DropDownMenu.cs)

#### Gradient Bar

A vertical or horizontal bar that can represent a color gradient using a function which returns a color from a float
between 0 and 1, with intervals that depend on the resolution.

[Source](UI/GradientBar.cs)

#### HSL Color Picker

A child class of DiscreteColorPicker that includes a Hue, Saturation, and Lightness bar for more precise color
selections.

[Source](UI/HslColorPicker.cs)

#### Item Selection Menu

A menu that displays all items in the game, with search functionality by name, and will add/remove item context tags in
an ItemMatcher.

[Source](UI/ItemSelectionMenu.cs)