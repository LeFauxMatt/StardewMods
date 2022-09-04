# Better Chests Change Log

## 2.7.0 (Unreleased)

### Added

* Added Chest Info feature.

## 2.6.3 (September 4, 2022)

### Changed

* Added support for Better Crafting from the inventory menu.

## 2.6.1 (September 2, 2022)

### Added

* Added Craft From Workbench feature.
* Added extra logging for Storages types and individual storages.

### Fixed

* Fixed ingredients getting multiplied every time Crafting Page is reopened.
* Fixed SearchBar not spanning full width of ItemSelectionMenu.

### Changed

* Changed LockSlot key to a KeybindList.
* Prevent CustomColorPicker from showing up on unsupported Storage Types.

## 2.6.0 (September 1, 2022)

### Added

* Added configurable SaddleBag type when Horse Overhaul is installed.

### Fixed

* Completely disable Shipping Bin features when a conflicting mod is installed.

### Changed

* Disallow any Chest from being automatically stashed into another chest.
* Craft From Chest will only lock chests when crafting items.
* Allow stacking of empty chests.
* When opening a held chest, gray out that chest in inventory.
* UnloadChest only activates when the target chest is clicked on.

## 2.5.5 (August 27, 2022)

### Fixed

* Fixed Unlimited Chests freezing the game.

## 2.5.4 (August 27, 2022)

### Changed

* Moved menu ChestLabel to not overlap with the vanilla Color Picker.

### Fixed

* Fixed ResizeChest and ResizeChestMenu not being Disabled.
* Fixed ChestLabel not supporting Zoom levels.
* Fixed TransferItems not saving config from GMCM menu.
* Fixed LabelChest not saving config from GMCM menu.
* Fixed config options not applying to Junimo Chests.

## 2.5.3 (August 21, 2022)

### Changed

* Added sound on picking up chests.
* Added sound on combining chests.
* Chests picked up from broken can be opened while held.

### Fixed

* Fixed vanilla color picker rendering under menu.
* Fixed AutoGrabber inventory menu overlapping with backpack inventory.

## 2.5.2 (August 16, 2022)

### Fixed

* Fixed CustomColorPicker being drawn over other menus.
* Fixed StashToChest not working for currently opened chest.

## 2.5.1 (August 15, 2022)

### Changed

* Assign actual values to default config options.

## 2.5.0 (August 14, 2022)

### Added

* Added multiple options for the Configure Menu.
    * Simple (Default) only shows Chest Label, Categorize, and Stash to Chest Priority/Stacks.
    * Categorize will load the Categorize Menu directly.
    * Full will show all the config options.
    * Advanced is full, and some options will be open text fields.

### Fixed

* Fixed CustomColorPicker drawing even when disabled.

### Changed

* Disabled features are now hidden from the configure menu.

## 2.4.0 (August 13, 2022)

### Added

* Added back context tag extensions.
    * category_artifact
    * category_furniture
    * donate_bundle
    * donate_museum
* UnloadChestCombine adds held chest capacity to target chest.

### Fixed

* Fixed features not being disabled when BetterShippingBin is disabled.
* Fixed tabs using non-localized hover text.
* Fixed CarryChestSlow debuff activating for empty chests.

### Changed

* Default Storage options are now part of the main config.
* The following features are now configurable individually or by type:
    * Configurator
    * HideItems
    * LabelChest
    * TransferItems
* Improved controller support.

## 2.3.1 (August 11, 2022)

### Added

* Added Chest preview to Color Picker.

### Fixed

* Fixed graphical glitches in menus.

## 2.3.0 (August 7, 2022)

### Added

* Added TransferItems feature to transfer items in/out of a chest.
* Added option to hide items instead of fading them out.
* Added negate search for categorizing items (hold shift).

### Fixed

* Fixed tabs being drawn over hover text.

### Changed

* Improvements to the Color Picker for reliability.
* Added localization support for storage names/tooltips.
* Require GMCM for Configurator feature.
* Optimizations for performance.

## 2.2.0 (July 15, 2022)

### Added

* Added config options for each vanilla chest type.

## 2.1.3 (July 13, 2022)

### Added

* Added support for matching modded interface colors.

### Changed

* CarryChest will not activate if holding a tool.

## 2.1.2 (July 12, 2022)

### Added

* Added an extra indicator for when you're in a tab.

### Fixed

* Fixed color changing everytime items are moved to/from a chest.

### Changed

* Updated tabs/search so they only fade items instead of hide them.
* Moved config icon back to top of side buttons.

## 2.1.1 (July 11, 2022)

### Fixed

* Fixed ChestMenuTabs appearing over some hover elements.
* Fixed Configure button not having background.
* Fixed Island Shipping Bin not launching correctly.
* Fixed cursor moving around on the Shipping Bin.
* Fixed AutoOrganize crash before saving.

## 2.1.0 (July 9, 2022)

### Added

* Added Chest Finder.
* Added localized tags for search and categorization.

### Fixed

* Fixed Shipping Bin not detected when launched by Chests Anywhere.
* Fixed items not updating when you add or remove items from a chest.
* Fixed AutoOrganize ignoring priority when moving items.

### Changed

* Configure chest now happens from the chest inventory menu.
* Improved integration with BetterCrafting for multiplayer.

## 2.0.3 (July 5, 2022)

### Added

* Added alert if FuryCore is installed.
* Added additional logging of config.
* Added additional logging of AutoOrganize.

### Fixed

* Fixed displayed items turning invisible in chests.

## 2.0.2 (July 2, 2022)

### Fixed

* Fixed CraftFromChest and StashToChest not working with Sheds/Fridges.
* Fixed wall of red text occurring when leaving farm.
* Fixed items not being in sync after organizing.
* Fixed error when opening up chests from ChestsAnywhere hotkey.

## 2.0.1 (July 1, 2022)

### Fixed

* Fixed Configure button being placed over community center.

## 2.0.0 (July 1, 2022)

### Added

* Added LabelChest feature.

### Fixed

* Fixed integration with BetterCrafting.
* Fixed crash when installed with BetterShippingBin.

### Changed

* Updated to SMAPI 3.15.0.
* Removed dependency on FuryCore.

### Changed

* Added Chinese Translation
* Added support for ModManifestBuilder.

### Fixed

* Updated BetterCrafting integration for v1.1.0+.

## 1.7.3 (March 25, 2022)

### Changed

* CarryChest will only work when no item is currently being held.
* FilterItems now handles negative only filters slightly differently.
    * Stashing requires at least one non-negative item to work.
    * Negative-only filters will continue to block disallowed items.

## 1.7.2 (February 26, 2022)

### Fixed

* Further optimized code to prevent looping over all items on each click.

## 1.7.1 (February 26, 2022)

* Updated to FuryCore 1.6.1

### Fixed

* Fixed lag associated with UnloadChests feature.
* Fixed FilterItems not patching Automate correctly.

## 1.7.0 (February 25, 2022)

* Updated to FuryCore 1.6.0

### Changed

* If BetterCrafting is loaded, use BetterCrafting API to open crafting page.

### Fixed

* Fixed disabled features being enabled by individual settings.
    * Disabled is intended to take precedence from default and/or chest types.

## 1.6.0 (February 23, 2022)

### Added

* Added integration with [Better Crafting](https://www.nexusmods.com/stardewvalley/mods/11115).

### Fixed

* Fixed controller getting stuck when switching tabs.

## 1.5.2 (February 23, 2022)

### Fixed

* Fixed Chests locking after activating Craft from Chest.

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