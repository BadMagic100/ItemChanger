using ItemChanger.Enums;
using ItemChanger.Placements;
using System;

namespace ItemChanger.Events.Args;

public class VisitStateChangedEventArgs(Placement placement, VisitState newFlags) : EventArgs
{
    public Placement Placement { get; } = placement;
    public VisitState Orig { get; } = placement.Visited;
    public VisitState NewFlags { get; } = newFlags;
    public bool NoChange => (NewFlags & Orig) == NewFlags;
}
