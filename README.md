# Pokemon-Style Game 2D (Unity)

This repository and README is currently under construction.

The goal of this project is to make a playable Pokemon-style game that speeds the process
of training, catching pokemon and eventually implementing a consecutive battle challenge.

## Acknowledgements
* Much credit to [Game Dev Experiments YouTube Tutorial](https://www.youtube.com/playlist?list=PLLf84Zj7U26kfPQ00JVI2nIoozuPkykDX) for in depth tutorials
* Through him, also credit to GameDev.Tv for the saving system implementation.

I DO NOT CLAIM ANY SPRITES AS MY OWN. ALL SPRITES BELONG TO POKEMON COMPANY AND ARE USED FOR NON-COMMERCIAL, PERSONAL USE.

## TODO
As of 10/7/2022, this project has followed along with Game Dev Experiment's tutorial up to and including #61. In addition to this progression, personal TODO's include
* ~~Implementing Flinch condition~~ (Implemented 2022/10/5)
* ~~Implementing Leech Seed condition~~ (Implemented 2022/10/7)
* ~~Update volatile effects to allow to be affected together like leech seed and confusion~~ (Implemented 2022/10/7)
* Implementing Pokemon abilities
* Implementing weather conditions
* Including Yes/No dialog box for new moves for confirmation
* Including a settings menu item
* Update opponent pokemon move loader logic
* Baby Maker (No eggs, but transform current pokemon into a level 1 version of either itself or it's lowest evolution)
* Update TM Party Screen UI to leverage colors instead of text (TBD)
* Make a smooth update for HP bar when item used in party screen

## Pokemon Universe Resource Expansion
* Caterpie (2022/10/7)
* Metapod (2022/10/7)
* Weedle (2022/10/7)
* Kakuna (2022/10/7)
* Beedrill (2022/10/7)
* Pidgey (2022/10/7)

## Move Universe Resource Expansion
* Added Leech Seed  (2022/10/7)
* Added Seed Bomb   (2022/10/7)
* Added Fire Fang (2022/10/7)
* Added Water Pulse (2022/10/7)
* String Shot (2022/10/7)
* Bug Bite (2022/10/7)
* Harden (2022/10/7)
* Poison Sting (2022/10/7)
* Gust (2022/10/7)
* Sand Attack (2022/10/7)

## Known Issues
* Fix bug where two characters can walk through each other if the motion is active before collision detection occurs
* ~~Fix bug where action selection box is still active when using a pokeball (and possibly item)~~ (Fixed 2022/10/10)
* ~~Fix bug where using a pokeball during a trainer battle decrements the pokeball count and skips to the opponents turn~~ (Fixed 2022/10/10)
* ~~Fix bug where spamming continue key upon learning TM move creates and error~~ (2022/10/10: possibly fixed, unable to recreate issue)
* Fix bug where player cannot cancel out of TM learn move state (with escape key) (2022/10/10: Deemed Low Priority since player can select the new move to cancel out, would want to allow escape key to leave as well though)
* ~~Fix bug where the name of TMs (possibly items in general) are cut off in bag~~ (Fixed 2022/10/10)
* Fix bug where TMs are not sorted by name (currently via pickup)

## Known Inconsistencies with Pokemon Games
* ~~Fix where statuses should be checked after all moves are completed~~ (Fixed 2022/10/7)
* ~~Fix where volatile status conditions should be cleared when switching pokemon~~ (Fixed 2022/10/7)

## Maintained Discrepancies
* As with new version of the games, there will be no HMs. TMs will also be reusable.

## Planning

1. Generation 1 Home - Starter Pokemon
2. Generation 1 Safari : Maybe catch one of each to improve catch rate?
    * Maybe break into 3 tiers
    * Tier 1 - Easy pokemon - Ex: Weedle, Pidgey
    * Tier 2 - Medium pokemon - Ex: Lapars, Dratini
    * Tier 3 - Legendary pokemon - Ex: Mewtwo, Zapdos
3. Generation 1 Battle Tower : 50? trainers
    * Maybe break the safari into 3 battle towers based on the 3 tiers
4. Generation 2 Home - Starter Pokemon : Decide what to do with gen 1 pokemon
4. Generation 2 Safari
5. Generation 2 Battle Tower : 50? trainers
6. Generation 3 Home - Starter Pokemon
7. Generation 3 Safari
8. Generation 3 Battle Tower