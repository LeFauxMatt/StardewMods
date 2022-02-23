# FuryCore

Provides additional APIs for my other mods.

## Contents

* [Mod Integration](#mod-integration)
    * [Direct Integration](#direct-integration)
    * [API](#api)
    * [Data](#data)
* [Helpers](#helpers)
    * [Item Matcher](#item-matcher)
* [Events](#events)
    * [ClickableMenu Changed](#clickablemenu-changed)
    * [Configuring GameObject](#configuring-gameobject)
    * [GameObjects Removed](#gameobjects-removed)
    * [HudComponent Pressed](#hudcomponent-pressed)
    * [MenuComponent Pressed](#menucomponent-pressed)
    * [MenuComponents Loading](#menucomponents-loading)
    * [MenuItems Changed](#menuitems-changed)
    * [Rendered ClickableMenu](#rendered-clickablemenu)
    * [Rendering ClickableMenu](#rendering-clickablemenu)
    * [Resetting Config](#resetting-config)
    * [Saving Config](#saving-config)
* [Services](#services)
    * [Configure Game Object](#configure-game-object)
    * [Custom Events](#custom-events)
    * [Custom Tags](#custom-tags)
    * [Game Objects](#game-objects)
    * [Harmony Helper](#harmony-helper)
    * [HUD Components](#hud-components)
    * [Menu Components](#menu-components)
    * [Menu Items](#menu-items)
    * [Mod Services](#mod-services)
* [UI](#ui)
    * [DropDown Menu](#dropdown-menu)
    * [Gradient Bar](#gradient-bar)
    * [HSL Color Picker](#hsl-color-picker)
    * [Item Selection Menu](#item-selection-menu)
* [Configure](#configure)
    * [Add Custom Tags](#add-custom-tags)
    * [Scroll Menu Overflow](#scroll-menu-overflow)
* [Translations](#translations)

### Mod Integration

FuryCore Services can be integrated through a direct reference or through an API.

#### Direct Integration

You can directly integrate by adding a reference to your csproj file.

Sample `mod.csproj` file:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <AssemblyName>EXAMPLE_MOD_NAME</AssemblyName>
    <Version>1.0.0</Version>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Pathoschild.Stardew.ModBuildConfig" Version="3.3.0" />
    <Reference Include="FuryCore">
      <HintPath>$(GameModsPath)\FuryCore\FuryCore.dll</HintPath>
      <Private>false</Private>
    </Reference>
  </ItemGroup>
</Project>
```

You'll need to create your own instance of [Mod Services](#mod-services) and then you can add FuryCore Services to your
own using the [API](#api) from the GameLaunched SMAPI event.

Sample `ModEntry.cs` file:

```cs
public class ModEntry : Mod
{
  private ModServices Services { get; } = new();

  private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
  {
    if (this.Helper.ModRegistry.IsLoaded("furyx639.FuryCore"))
    {
      var furyCoreApi = this.Helper.ModRegistry.GetApi<IFuryCoreApi>("furyx639.FuryCore");
      furyCoreApi.AddFuryCoreServices(this.Services);
    }
  }
}
```

#### API

Get basic access to FuryCore services using the [Fury Core API](../Common/Integrations/FuryCore/IFuryCoreApi.cs).

#### Data

Some integration is possible via data paths using
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

### Helpers

#### Item Matcher

Stores a list of search phrases to test against an Item. It's able to make exact or partial searches and can use the
name or any of the item's context tags. Also adds some custom context tags for searching items based on if they are
furniture, an artifact, can be donated to a bundle, and/or can be donated to the museum.

[Source](Helpers/ItemMatcher.cs)

### Events

#### ClickableMenu Changed

Raised after a supported menu is constructed or whenever the Active Menu switches to/from an supported menu. On
construction, this event triggers as a postfix to the vanilla constructor so any changes made are before the menu is
displayed to the screen.

[ [Source](Events/ClickableMenuChanged.cs) | [EventArgs](Interfaces/CustomEvents/IClickableMenuChangedEventArgs.cs) ]

#### Configuring GameObject

Raised before a Mod Config Menu will be shown for the current game object.

[ [Source](Events/ConfiguringGameObject.cs) | [EventArgs](Interfaces/CustomEvents/IConfiguringGameObjectEventArgs.cs) ]

#### GameObjects Removed

Raised after inaccessible game objects are purged from the cache.

[ [Source](Events/GameObjectsRemoved.cs) | [EventArgs](Interfaces/CustomEvents/IGameObjectsRemovedEventArgs.cs) ]

#### HudComponent Pressed

Raised after a custom toolbar icon is pressed.

[ [Source](Events/HudComponentPressed.cs) | [EventArgs](Interfaces/CustomEvents/IClickableComponentPressedEventArgs.cs) ]

#### MenuComponent Pressed

Raised after a vanilla or custom component is pressed on a supported menu.

[ [Source](Events/MenuComponentPressed.cs) | [EventArgs](Interfaces/CustomEvents/IClickableComponentPressedEventArgs.cs) ]

#### MenuComponents Loading

Raised before components are added to the current menu.

[ [Source](Events/MenuComponentsLoading.cs) | [EventArgs](Interfaces/CustomEvents/IMenuComponentsLoadingEventArgs.cs) ]

#### MenuItems Changed

Raised before items are displayed on the current menu.

[ [Source](Events/MenuItemsChanged.cs) | [EventArgs](Interfaces/CustomEvents/IMenuItemsChangedEventArgs.cs) ]

#### Rendered ClickableMenu

Identical to RenderingActiveMenu except for anything drawn to the SpriteBatch will be above the background fade but
below the actual menu graphics.

[ [Source](Events/RenderedClickableMenu.cs) ]

#### Rendering ClickableMenu

Identical to RenderedActiveMenu except for anything drawn to the SpriteBatch will be above the menu but below the cursor
and any hover elements such as text or item.

[ [Source](Events/RenderingClickableMenu.cs) ]

#### Resetting Config

Raised after the Reset button is hit on the current mod config menu.

[ [Source](Events/ResettingConfig.cs) | [EventArgs](Interfaces/CustomEvents/IResettingConfigEventArgs.cs) ]

#### Saving Config

Raised after the Save button is hit on the current mod config menu.

[ [Source](Events/SavingConfig.cs) | [EventArgs](Interfaces/CustomEvents/ISavingConfigEventArgs.cs) ]

### Services

#### Configure Game Object

Allows an anonymous mod config menu to be opened for the current game object which other mods can add config options to.

[ [Interface](Interfaces/IConfigureGameObject.cs) | [Source](Services/ConfigureGameObject.cs) ]

#### Custom Events

Provides access to custom events.

[ [Interface](Interfaces/ICustomEvents.cs) | [Source](Services/CustomEvents.cs) ]

#### Custom Tags

Allows adding dynamic custom context tags to items that can use realtime conditions.

[ [Interface](Interfaces/ICustomTags.cs) | [Source](Services/CustomTags.cs) ]

#### Game Objects

Provides access to most objects in the game including items, buildings, and locations.

[ [Interface](Interfaces/IGameObjects.cs) | [Source](Services/GameObjects.cs) ]

#### Harmony Helper

Saves a list of Harmony Patches, and allows them to be applied or reversed at any time.

[ [Interface](Interfaces/IHarmonyHelper.cs) | [Source](Services/HarmonyHelper.cs) ]

#### Hud Components

Add icons to the left or right of the player items toolbar.

[ [Interface](Interfaces/IHudComponents.cs) | [Source](Services/HudComponents.cs) ]

#### Menu Components

Add custom components to the ItemGrabMenu which can optionally automatically align to certain areas of the screen. In
this case neighboring components are automatically assigned for controller support.

[ [Interface](Interfaces/IMenuComponents.cs) | [Source](Services/MenuComponents.cs) ]

#### Menu Items

Allows displayed items to be handled separately from actual items. This enables support for such things as filtering
displayed items or scrolling an overflow of items without affecting the source inventory.

[ [Interface](Interfaces/IMenuItems.cs) | [Source](Services/MenuItems.cs) ]

#### Mod Services

All of FuryCores APIs are access through this service.

[ [Interface](Interfaces/IModService.cs) | [Source](Services/ModServices.cs) ]

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

## Configure

### Configure Key

Keybind that brings up a config menu for the item that you're carrying.

### Add Custom Tags

Choose whether to allow adding custom tags to items.

* `category_artifact` added to all items that are Artifacts.
* `category_furniture` added to all items that are Furniture.
* `donate_bundle` added to all items that can be donated to a Community Center bundle.
* `donate_museum` added to all items that can be donated to the Museum.

### Scroll Menu Overflow

Choose whether to handle scrolling items that overflow an ItemGrabMenu.

Enabling this option will capture the MouseWheelScrolled event and add up/down arrow buttons to scroll items.

### Toolbar Icons

Allow other mods to add clickable icons above/below the toolbar.

## Translations

| Language                   | Status            | Credits  |
|:---------------------------|:------------------|:---------|
| Chinese                    | ❌️ Not Translated |          |
| French                     | ❌️ Not Translated |          |
| German                     | ❌️ Not Translated |          |
| Hungarian                  | ❌️ Not Translated |          |
| Italian                    | ❌️ Not Translated |          |
| Japanese                   | ❌️ Not Translated |          |
| [Korean](i18n/ko.json)     | ✔️ Complete       | wally232 |
| [Portuguese](i18n/pt.json) | ✔️ Complete       | Aulberon |
| Russian                    | ❌️ Not Translated |          |
| Spanish                    | ❌️ Not Translated |          |
| Turkish                    | ❌️ Not Translated |          |