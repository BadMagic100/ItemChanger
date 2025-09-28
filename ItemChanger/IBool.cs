using ItemChanger.Extensions;
using ItemChanger.Internal;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace ItemChanger;

/// <summary>
/// Interface which can supply a bool value. Used frequently for serializable bool tests.
/// </summary>
public interface IBool
{
    bool Value { get; }
    IBool Clone();
}

/// <summary>
/// IBool which supports write operations.
/// </summary>
public interface IWritableBool : IBool
{
    new bool Value { get; set; }
}

/// <summary>
/// IBool which represents a constant value.
/// </summary>
public class BoxedBool(bool value) : IWritableBool
{
    public bool Value { get; set; } = value;

    public IBool Clone() => (IBool)MemberwiseClone();
}

/// <summary>
/// IBool which represents comparison on a PlayerData int.
/// <br/>Supports IWritableBool in one direction only (direction depends on comparison operator).
/// </summary>
public class IntComparisonBool(IInteger Int, int Amount, ComparisonOperator op = ComparisonOperator.Ge) : IBool
{
    /// <inheritdoc/>
    [JsonIgnore]
    public bool Value
    {
        get
        {
            return Int.Value.Compare(op, Amount);
        }
    }

    public IBool Clone() => (IBool)MemberwiseClone();
}

/// <summary>
/// IBool which searches for a placement by name and checks whether all items on the placement are obtained.
/// <br/>If the placement does not exist, defaults to the value of missingPlacementTest, or true if missingPlacementTest is null.
/// </summary>
public class PlacementAllObtainedBool(string placementName, IBool? missingPlacementTest = null) : IBool
{
    public string PlacementName => placementName;
    public IBool? MissingPlacementTest => missingPlacementTest;

    [JsonIgnore]
    public bool Value
    {
        get
        {
            if (ItemChangerProfile.ActiveProfile.TryGetPlacement(placementName, out Placement? p) && p != null)
            {
                return p.AllObtained();
            }
            return MissingPlacementTest?.Value ?? true;
        }
    }

    public IBool Clone()
    {
        PlacementAllObtainedBool obj = (PlacementAllObtainedBool)MemberwiseClone();
        return obj;
    }
}

/// <summary>
/// IBool which searches for a placement by name and checks whether its VisitState includes specified flags.
/// <br/>If the placement does not exist, defaults to the value of missingPlacementTest, or true if missingPlacementTest is null.
/// </summary>
public class PlacementVisitStateBool(string placementName, VisitState requiredFlags, IBool missingPlacementTest) : IBool
{
    public string placementName = placementName;
    public VisitState requiredFlags = requiredFlags;
    /// <summary>
    /// If true, requires any flag in requiredFlags to be contained in the VisitState. If false, requires all flags in requiredFlags to be contained in VisitState. Defaults to false.
    /// </summary>
    public bool requireAny;
    public IBool? missingPlacementTest = missingPlacementTest;

    [JsonIgnore]
    public bool Value
    {
        get
        {
            if (ItemChangerProfile.ActiveProfile.TryGetPlacement(placementName, out Placement? p) && p != null)
            {
                return requireAny ? p.CheckVisitedAny(requiredFlags) : p.CheckVisitedAll(requiredFlags);
            }
            return missingPlacementTest?.Value ?? true;
        }
    }

    public IBool Clone()
    {
        PlacementVisitStateBool obj = (PlacementVisitStateBool)MemberwiseClone();
        obj.missingPlacementTest = obj.missingPlacementTest?.Clone();
        return obj;
    }
}

public class Disjunction : IBool
{
    public List<IBool> bools = new();
    public Disjunction() { }
    public Disjunction(IEnumerable<IBool> bools)
    {
        this.bools.AddRange(bools);
    }
    public Disjunction(params IBool[] bools)
    {
        this.bools.AddRange(bools);
    }

    [JsonIgnore]
    public bool Value => bools.Any(b => b.Value);

    public IBool Clone()
    {
        return new Disjunction(bools.Select(b => b.Clone()));
    }

    public Disjunction OrWith(IBool b) => b is Disjunction d ? new([..bools, ..d.bools]) : new([..bools, b]);
}

public class Conjunction : IBool
{
    public List<IBool> bools = new();
    public Conjunction() { }
    public Conjunction(IEnumerable<IBool> bools)
    {
        this.bools.AddRange(bools);
    }
    public Conjunction(params IBool[] bools)
    {
        this.bools.AddRange(bools);
    }

    /// <inheritdoc/>
    [JsonIgnore]
    public bool Value => bools.All(b => b.Value);

    /// <inheritdoc/>
    public IBool Clone()
    {
        return new Conjunction(bools.Select(b => b.Clone()));
    }

    public Conjunction AndWith(IBool b) => b is Conjunction c ? new([..bools, ..c.bools]) : new([..bools, b]);
}

[method: JsonConstructor]
public class Negation(IBool Bool) : IBool
{
    public IBool Bool { get; } = Bool;

    /// <inheritdoc/>
    [JsonIgnore]
    public bool Value => !Bool.Value;

    /// <inheritdoc/>
    public IBool Clone()
    {
        return new Negation(Bool.Clone());
    }
}
