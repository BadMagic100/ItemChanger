using ItemChanger.Costs;
using ItemChanger.Serialization;

namespace ItemChangerTests.Fixtures;

internal record DollarCost : ThresholdIntCost
{
    public IInteger Source { get; set; } = new BoxedInteger(50);

    public override string GetCostText() => $"Have ${GetValueSource().Value}";

    protected override IInteger GetValueSource() => Source;
}
