# Better Chests Change Log

## 1.3.0 (Unreleased)

* SlotLock Keybind is now a modifier key.
    * Must be held and left-click to lock a slot.
* LockedSlots are now attached to the item.
* Refactor to handle different types of storages.
* Added support for Junimo Hut.

## 1.2.1 (February 6, 2022)

* Quick hotfix for ModIntegration error.

## 1.2.0 (February 6, 2022)

* The ItemSelectionMenu is now affected by Better Chest features.
    * ResizeChestMenu will expand the ItemSelectionMenu.
    * ChestMenuTabs will filter the ItemSelectionMenu.
    * SearchItems will add the search bar to the ItemSelectionMenu.
* Added CarryChestLimit option to limit the number of chests that can be carried at once.
* Added red text alerts to certain features.
    * When CarryChestLimit is reached and attempting to carry another chest.
    * When attempting to Craft from Chests and no eligible chests were found.
    * When attempting to Stash to Chests and no eligible chests were found.
* Added StashToChestPriority to Chest Data.
* Added more logging on `better_chests_info` command.
    * List out eligible chests for CraftFromChest.
    * List out eligible chests for StashToChest.
* Locked Slots now holds items in place when shifting the toolbar.
* Added CarryChestSlow for speed debuff while carrying a chest.
* Added integration for HorseOverhaul mod.

## 1.1.0 (February 5, 2022)

* Updated method for keeping Chests in sync for multiplayer.

## 1.0.0 (February 3, 2022)

### Added

* Initial Version