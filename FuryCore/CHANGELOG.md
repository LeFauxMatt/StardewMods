# FuryCore Change Log

## 1.6.4 (Unreleased)

* Updated to SMAPI 3.14.0

### Changed

* Added support for matching localized names.
* Added Hungarian Translation.
* Added support for ModManifestBuilder.

## 1.6.3 (March 20, 2022)

### Changed

* Removed support for TerrainFeatures.

## 1.6.2 (February 26, 2022)

### Fixed

* Further optimized code to prevent looping over all items on each click.

## 1.6.1 (February 26, 2022)

### Fixed

* Fixed some lag issues from the last update.

## 1.6.0 (February 25, 2022)

### Added

* Added support for TerrainFeatures.

### Fixed

* Fixed Toolbar Icons being clickable even when not displayed.
* Fixed objects being initiated with their unmapped context.

## 1.5.0 (February 22, 2022)

### Added

* Added IConfigureGameObject service.
* Added MenuComponentsLoading Event.
* Added MenuItemsChanged Event.
* Added StorageFridge as an IStorageContainer.
* Added StorageJunimoHut as an IStorageContainer.
* Added StorageObject as an IStorageContainer.
* Added ComponentLayer Enum for IClickableComponent.
* Added support for the PurchaseAnimalsMenu.

### Changed

* Renamed ItemGrabMenuChanged to ClickableMenuChanged.
* Renamed RenderedItemGrabMenu to RenderedClickableMenu.
* Renamed RenderingItemGrabMenu to RenderedClickableMenu.
* Renamed ToolbarIconPressed to HudComponentPressed.
* Renamed ToolbarIcons to HudComponents.
* Renamed IMenuComponent to IClickableComponent.
* Renamed IToolbarIcon to IHudComponent.

## 1.4.1 (February 16, 2022)

### Fixed

* Fixed ToolbarIcons config not working.
* Fixed icons being pinned to top when toolbar is in a locked position.

## 1.4.0 (February 15, 2022)

### Added

* Added IFuryCoreApi for SMAPI integration.
* Added IToolbarIcons service.
* Added special handling of Shipping Bin containers.

### Changed

* Purge inaccessible cached objects.

## 1.3.0 (February 12, 2022)

### Added

* Added IGameObjects service.

## Changed

* Refactor to handle different types of storages.

## 1.2.0 (February 6, 2022)

## Changed

* Item Selection Menu now lists most context tags on the bottom menu.

## 1.1.0 (February 5, 2022)

### Added

* Added new ICustomTags service.
* Added GMCM integration for new config options.
    * Option to add some custom context tags (enabled by default).
        * `category_artifact` for items that are Artifact.
        * `category_furniture` for items that are Furniture.
        * `donate_bundle` for items that can be donated to a Community Center bundle.
        * `donate_museum` for items that can be donated to the Museum.

## 1.0.0 (February 3, 2022)

* Initial Version