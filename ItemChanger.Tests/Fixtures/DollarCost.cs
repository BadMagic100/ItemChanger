using ItemChanger.Costs;
using ItemChanger.Serialization;

namespace ItemChanger.Tests.Fixtures;

internal class DollarCost : ConsumableIntCost
{
    public IWritableInteger Source { get; set; } = new BoxedInteger(50);

    public override string GetCostText() => $"Pay ${GetValueSource().Value}";

    protected override IWritableInteger GetValueSource() => Source;
}
