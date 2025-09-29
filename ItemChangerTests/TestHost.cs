using ItemChanger;
using ItemChanger.Containers;
using ItemChanger.Events;
using ItemChanger.Internal;
using ItemChanger.Logging;
using Xunit.Abstractions;

namespace ItemChangerTests
{
    internal class TestLogger : ILogger
    {
        private readonly ITestOutputHelper output;

        public TestLogger(ITestOutputHelper output)
        {
            this.output = output;
        }

        public void LogError(string? message)
        {
            output.WriteLine($"[ERROR]: {message}");
        }

        public void LogFine(string? message)
        {
            output.WriteLine($"[FINE]: {message}");
        }

        public void LogInfo(string? message)
        {
            output.WriteLine($"[INFO]: {message}");
        }

        public void LogWarn(string? message)
        {
            output.WriteLine($"[WARN]: {message}");
        }
    }

    internal class TestHost : IItemChangerHost
    {
        public TestHost(ITestOutputHelper output)
        {
            Logger = new TestLogger(output);
        }

        public ILogger Logger { get; }

        Container IItemChangerHost.DefaultSingleItemContainer => throw new NotImplementedException();

        Container IItemChangerHost.DefaultMultiItemContainer => throw new NotImplementedException();

        void IItemChangerHost.PrepareEvents(LifecycleEvents.Invoker lifecycleInvoker, GameEvents.Invoker gameInvoker)
        {
            LifecyleEventsInvoker = lifecycleInvoker;
            GameEventsInvoker = gameInvoker;
        }

        void IItemChangerHost.UnhookEvents(LifecycleEvents.Invoker lifecycleInvoker, GameEvents.Invoker gameInvoker)
        {
            LifecyleEventsInvoker = null;
            GameEventsInvoker = null;
        }

        public LifecycleEvents.Invoker? LifecyleEventsInvoker { get; private set; }
        public GameEvents.Invoker? GameEventsInvoker { get; private set; }
    }
}
