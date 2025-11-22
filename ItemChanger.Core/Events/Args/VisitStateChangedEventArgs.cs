using System;
using ItemChanger.Enums;
using ItemChanger.Placements;

namespace ItemChanger.Events.Args;

public class VisitStateChangedEventArgs(Placement placement, VisitState newFlags) : EventArgs
{
    /// <summary>
    /// Placement whose state changed.
    /// </summary>
    public Placement Placement { get; } = placement;
    public VisitState Orig { get; } = placement.Visited;
    public VisitState NewFlags { get; } = newFlags;
    public bool NoChange => (NewFlags & Orig) == NewFlags;
}
