using System;

namespace ItemChanger.Enums;

/// <summary>
/// Enum for adding special behvaior to the respawn marker tied to a StartDef.
/// </summary>
[Flags]
public enum SpecialStartEffects
{
    None = 0,
    DelayedWake = 1,
    SlowSoulRefill = 1 | 1 << 1,
    ExtraInvincibility = 1 << 2,

    Default = DelayedWake | ExtraInvincibility,
}
