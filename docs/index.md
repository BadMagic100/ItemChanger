# About ItemChanger.Core

[![NuGet Version](https://img.shields.io/nuget/v/ItemChanger.Core)](https://www.nuget.org/packages/ItemChanger.Core)
[![GitHub License](https://img.shields.io/github/license/BadMagic100/ItemChanger.Core?logo=github)](https://github.com/BadMagic100/ItemChanger.Core)

ItemChanger.Core is a library to aid in the creation of randomizers and similar mods for Unity games by simplifying the
process of adding items, locations, and other sandboxed world edits to a game file. It provides a refactored and
game-agnostic implementation of
[Hollow Knight's ItemChanger mod](https://github.com/homothetyhk/HollowKnight.ItemChanger) by homothety.

## Why ItemChanger.Core?

Many games on the market are not implemented with a sufficient level of abstraction to implement a fully-featured
randomizer. Maybe the devs are inexperienced, or maybe they just were not thinking about what it would take for a modder
to build a randomizer, or maybe there was some other reason. Whatever that reason is, the result is that randomizer
developers need to implement these abstractions themselves, often requiring significant boilerplate for even a minimal
working solution, let alone a fully-featured one. ItemChanger.Core provides the key abstractions out of the box with a
variety of extension points allowing game-specific behavior to be injected into the framework.

ItemChanger.Core was developed based on lessons learned on a fairly large and popular randomizer. Hollow Knight's
randomizer went through several major revisions as the game was updated. By the time
[Randomizer 4](https://github.com/homothetyhk/RandomizerMod/tree/master) was in development, multiple pain points had
emerged:

- The randomizer was growing large; extensibility was needed to allow add-on mods in order to reduce the maintenance
  burden of the main randomizer.
- Opening vanilla saves with randomizer installed could permanently alter the save. There was a clear need for better
  sandboxing to prevent user error.
- Creating other randomizer-like game modes (e.g. plandos) required heavy modification to the randomizer itself to reuse
  the code.

Hollow Knight's ItemChanger mod was created to help address these issues. ItemChanger.Core further expands on the
lessons learned during Randomizer 4's lifetime when breaking changes could not be easily made.

## Key Features

- Game-agnostic implementation reduces boilerplate for developers
- Sandboxed and serializable world edits simplify the player experience
- Abstractions for a variety of game modifications, including item effects, item placements, and other bespoke behavior
  changes
- Easily extensible system for defining custom items and locations
