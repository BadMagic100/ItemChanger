using ItemChanger.Containers;
using ItemChanger.Events;
using ItemChanger.Logging;

namespace ItemChanger.Internal;

public interface IItemChangerHost
{
    public ILogger Logger { get; }

    public Container DefaultSingleItemContainer { get; }

    public Container DefaultMultiItemContainer { get; }

    public void PrepareEvents(LifecycleEvents.Invoker lifecycleInvoker, GameEvents.Invoker gameInvoker);

    public void UnhookEvents(LifecycleEvents.Invoker lifecycleInvoker, GameEvents.Invoker gameInvoker);
}
