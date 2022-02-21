# FuryCore Change Log

## 1.5.0 (Unreleased)

### Additions

* Added MenuComponentsLoading Event.
* Added MenuItemsChanged Event.
* Added StorageFridge as an IStorageContainer.
* Added StorageJunimoHut as an IStorageContainer.
* Added StorageObject as an IStorageContainer.
* Added ComponentLayer Enum for IClickableComponent.
* Added support for the PurchaseAnimalsMenu.

### Changes

* Renamed ItemGrabMenuChanged to ClickableMenuChanged.
* Renamed RenderedItemGrabMenu to RenderedClickableMenu.
* Renamed RenderingItemGrabMenu to RenderedClickableMenu.
* Renamed ToolbarIconPressed to HudComponentPressed.
* Renamed ToolbarIcons to HudComponents.
* Renamed IMenuComponent to IClickableComponent.
* Renamed IToolbarIcon to IHudComponent.

## 1.4.1 (February 16, 2022)

### Fixes

* Fixed ToolbarIcons config not working.
* Fixed icons being pinned to top when toolbar is in a locked position.

## 1.4.0 (February 15, 2022)

### Additions

* Added IFuryCoreApi for SMAPI integration.
* Added IToolbarIcons service.
* Added special handling of Shipping Bin containers.

### Changes

* Purge inaccessible cached objects.

## 1.3.0 (February 12, 2022)

### Additions

* Added IGameObjects service.

## Changes

* Refactor to handle different types of storages.

## 1.2.0 (February 6, 2022)

## Changes

* Item Selection Menu now lists most context tags on the bottom menu.

## 1.1.0 (February 5, 2022)

### Additions

* Added new ICustomTags service.
* Added GMCM integration for new config options.
    * Option to add some custom context tags (enabled by default).
        * `category_artifact` for items that are Artifact.
        * `category_furniture` for items that are Furniture.
        * `donate_bundle` for items that can be donated to a Community Center bundle.
        * `donate_museum` for items that can be donated to the Museum.

## 1.0.0 (February 3, 2022)

* Initial Version