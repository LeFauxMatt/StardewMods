# Helpful Spouses

Helpful Spouses (and Roommates) will perform one or more customizable chores every morning. By default this all happens in a balanced/lore-friendly way.

## Contents

* [Configurations](#configurations)
* [Customization](#customization)
* [Translations](#translations)
* [Credits](#credits)

## Configurations

For ease of use, it is recommended to set config options
from [Generic Mod Config Menu](https://www.nexusmods.com/stardewvalley/mods/5098).


| Config       | Description                                                                         | Default Value | 
|:-------------|:------------------------------------------------------------------------------------|---------------|
| DailyLimit   | The maximum number of chores a spouse will perform in one day.                      | 1             |
| GlobalChance | The chance that a spouse will perform any chore that day.                           | 1.0           |
| HeartsNeeded | The minimum number of hearts required before a spouse will begin performing chores. | 12            |

Additionally, each chore has some configuration options to modify its behavior.

### Birthday Gift

| Config           | Description                                    | Default Value | 
|:-----------------|:-----------------------------------------------|---------------|
| ChanceForDislike | The chance that a disliked item will be given. | 0             |
| ChanceForHate    | The chance that a hated item will be given.    | 0             |
| ChanceForLike    | The chance that a liked item will be given.    | 0.5           |
| ChanceForLove    | The chance that a loved item will be given.    | 0.2           |
| ChanceForNeutral | The chance that a neutral item will be given.  | 0.1           |

### Feed the Animals

| Config             | Description                                                                         | Default Value    | 
|:-------------------|:------------------------------------------------------------------------------------|------------------|
| AnimalLimit        | The maximum number of animals that will be fed.                                     | 0                |
| ValidOccupantTypes | The type of Animal Houses that will be chosen.                                      | ["Barn", "Coop"] |

Note - Setting AnimalLimit to 0 will make all animals get fed.

### Love the Pets

| Config        | Description                                                     | Default Value | 
|:--------------|:----------------------------------------------------------------|---------------|
| EnablePetting | Whether pets will be petted, increasing their friendship level. | true          |
| FillWaterBowl | Whether pet water bowls will be filled.                         | true          |

### Pet the Animals

| Config             | Description                                        | Default Value    | 
|:-------------------|:---------------------------------------------------|------------------|
| AnimalLimit        | The maximum number of animals that will be petted. | 0                |
| ValidOccupantTypes | The type of Animal Houses that will be chosen.     | ["Barn", "Coop"] |

Note - Setting AnimalLimit to 0 will make all animals get fed.

### Repair the Fences

| Config     | Description                                         | Default Value | 
|:-----------|:----------------------------------------------------|---------------|
| FenceLimit | The maximum number of fences that will be repaired. | 0             |

Note - Setting FenceLimit to 0 will make all fences get repaired.

### Water the Crops

| Config    | Description                                       | Default Value | 
|:----------|:--------------------------------------------------|---------------|
| CropLimit | The maximum number of crops that will be watered. | 0             |

Note - Setting CropLimit to 0 will make all crops get watered.

### Water the Slimes

| Config     | Description                                               | Default Value | 
|:-----------|:----------------------------------------------------------|---------------|
| SlimeLimit | The maximum number of slime troughs that will be watered. | 0             |

Note - Setting SlimeLimit to 0 will make all slime troughs get watered.

## Customization

Spouse rules determine which chores a spouse will do, and what the chance is that they'll pick that core.

See [here](assets/spouseRules.json) for the default spouse rules.

## Translations

Translations are supported using [Project Fluent](https://www.nexusmods.com/stardewvalley/mods/12638).


See [here](i18n/en.ftl) for the base translation file.

| Language   | Status            | Credits |
|:-----------|:------------------|:--------|
| Chinese    | ❔ Incomplete      |         |
| French     | ❌️ Not Translated |         |
| German     | ❌️ Not Translated |         |
| Hungarian  | ❌️ Not Translated |         |
| Italian    | ❌️ Not Translated |         |
| Japanese   | ❌️ Not Translated |         |
| Korean     | ❌️ Not Translated |         |
| Portuguese | ❌️ Not Translated |         |
| Russian    | ❌️ Not Translated |         |
| Spanish    | ❌️ Not Translated |         |
| Turkish    | ❌️ Not Translated |         |

## Credits

Shane and Emily dialogue was written by alekrrau.

Elliott dialogue was written by janamods01.

Spouse rules for SVE, RSV, ES, Zuzu, Lunna - Astray, and others were provided by Monthy. 