# Better Chests Change Log

## 1.5.1 (February 22, 2022)

### Fixed

* Fixed AutoOrganize not working for any chest.

## 1.5.0 (February 22, 2022)

### Added

* Added AutoOrganize feature.
* Shipping Bin on Island is now recognized by Better Chests and uses Shipping Bin as it's chest type.
* Integrate Configurator using new FuryCore service.

### Changed

* Allow ModData override for storage names in locations and buildings
* Chests are now sorted in GMCM.
* More detailed logging when items are being stashed.

### Fixed

* Fixed StashToChest using CraftFromChest config for Default Chest.
* Fixed Fridge sometimes using default chest config.
* StashToChest will once again use StashToChestPriority.

## 1.4.2 (February 16, 2022)

### Fixed

* Fixed an error resulting from Chests that had an exact capacity of 72.

## 1.4.1 (February 15, 2022)

### Fixed

* Fixed CollectItems error.

## 1.4.0 (February 15, 2022)

### Added

* Added Organize Chest feature.
* Added Toolbar icons for Stash to Chest and Craft from Chest.
* Added Chest Menu for Shipping Bins.

### Changed

* Purge inaccessible cached objects.
* Optimized CollectItems code.

## 1.3.0 (February 12, 2022)

### Added

* Added support for Auto-Grabber.
* Added support for Junimo Hut.
* Added support for Shipping Bin.
* Added manual compatibility for XSLite chests.
    * Custom chest types must be defined from chests folder.

### Changed

* SlotLock Keybind is now a modifier key.
    * Must be held and left-click to lock a slot.
* LockedSlots are now attached to the item.
* Refactor to handle different types of storages.
* Refactor enumerating game objects into FuryCore service.

## 1.2.1 (February 6, 2022)

### Fixed

* Quick hotfix for ModIntegration error.

## 1.2.0 (February 6, 2022)

### Added

* Added CarryChestLimit option to limit the number of chests that can be carried at once.
* Added red text alerts to certain features.
    * When CarryChestLimit is reached and attempting to carry another chest.
    * When attempting to Craft from Chests and no eligible chests were found.
    * When attempting to Stash to Chests and no eligible chests were found.
* Added StashToChestPriority to Chest Data.
* Added more logging on `better_chests_info` command.
    * List out eligible chests for CraftFromChest.
    * List out eligible chests for StashToChest.
* Added CarryChestSlow for speed debuff while carrying a chest.
* Added integration for HorseOverhaul mod.

### Changed

* The ItemSelectionMenu is now affected by Better Chest features.
    * ResizeChestMenu will expand the ItemSelectionMenu.
    * ChestMenuTabs will filter the ItemSelectionMenu.
    * SearchItems will add the search bar to the ItemSelectionMenu.
* Locked Slots now holds items in place when shifting the toolbar.

## 1.1.0 (February 5, 2022)

### Fixed

* Updated method for keeping Chests in sync for multiplayer.

## 1.0.0 (February 3, 2022)

* Initial Version