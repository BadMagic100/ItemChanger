---
uid: host
---

# The Host Model

As a game-agnostic solution, ItemChanger.Core cannot automatically hook most core game functionality. As such, some
mechanism is needed to allow these hooks to be created, and to "host" the core abstractions from the library within
whatever mod loader the game uses. This is where @"ItemChanger.ItemChangerHost" comes in. The host provides a subset of
the functionality provided by the original Hollow Knight ItemChanger mod, like integrating ItemChanger profiles into a
save file and creating game-specific item, location, and container types, while deferring the bulk of the abstraction
model to the core library.

## Implementing a Host

Architecturally speaking, the intended approach is for each game implementation to have a single host mod. As such, many
users of this library will never have to implement a host; instead, find the one that has already been created and
interact with ItemChanger through that.

For users who do need to implement a host, all that's needed is to create a class derived from
@"ItemChanger.ItemChangerHost". Creating an instance of your host will automatically set the global singleton in
ItemChanger.

### Opinionated Recommendations for Host Implementations

Here are a few opinionated recommendations for host implementations to follow. The word "should" and "may" are meant to
be interpreted in the [RFC2119](https://datatracker.ietf.org/doc/html/rfc2119) sense.

- Implementation recommendations
  - Hosts SHOULD NOT expose any way for a consumer to indirectly call @"ItemChanger.ItemChangerHost.DetachSingleton".
  - Hosts SHOULD provide static access to a narrowly-typed instance of their host implementation to provide
    game-specific API surfaces.
  - Hosts SHOULD NOT provide any static singletons to consumers other than the host instance.
  - Hosts MAY create an API surface to allow add-on mods to inject their own module construction into
    @"ItemChanger.ItemChangerHost.BuildDefaultModules".
  - Hosts MAY create additional event classes to provide game-specific events on the host.
- Distribution recommendations
  - Hosts SHOULD redistribute ItemChanger.Core as part of the mod containing the host implementation.
  - If a host is published to NuGet, the package name SHOULD be `ItemChanger.<GameName>`. This does not need to be the
    name of the mod or assembly exposed to users.
