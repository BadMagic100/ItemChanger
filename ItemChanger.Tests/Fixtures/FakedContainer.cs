using ItemChanger.Containers;

namespace ItemChanger.Tests.Fixtures;

internal class FakedContainer : Container
{
    public override string Name => "Fake";

    public override uint SupportedCapabilities => uint.MaxValue;

    protected override void Load() { }

    protected override void Unload() { }
}
