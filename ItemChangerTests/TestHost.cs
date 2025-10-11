using ItemChanger;
using ItemChanger.Containers;
using ItemChanger.Events;
using ItemChanger.Logging;
using ItemChanger.Modules;

namespace ItemChangerTests;

internal class TestLogger : ILogger
{
    private readonly ITestOutputHelper output;
    public List<string?> ErrorMessages { get; } = [];


    public TestLogger(ITestOutputHelper output)
    {
        this.output = output;
    }

    public void LogError(string? message)
    {
        ErrorMessages.Add(message);
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

internal class TestHost : ItemChangerHost, IDisposable
{
    public TestHost(ITestOutputHelper output) : base()
    {
        Logger = new TestLogger(output);
        Profile = new(this);
    }

    void IDisposable.Dispose()
    {
        DetachSingleton();
    }

    public override ILogger Logger { get; }
    public List<string?> ErrorMessages { get => ((TestLogger)Logger).ErrorMessages; }

    public ItemChangerProfile Profile { get; }

    public override ContainerRegistry ContainerRegistry
    {
        get
        {
            FakedContainer fake = new FakedContainer();
            return field ??= new ContainerRegistry() { DefaultSingleItemContainer = fake, DefaultMultiItemContainer = fake };
        }
    }

    public override IEnumerable<Module> BuildDefaultModules() => [];

    public override void PrepareEvents(LifecycleEvents.Invoker lifecycleInvoker, GameEvents.Invoker gameInvoker)
    {
        LifecycleEventsInvoker = lifecycleInvoker;
        GameEventsInvoker = gameInvoker;
    }

    public override void UnhookEvents(LifecycleEvents.Invoker lifecycleInvoker, GameEvents.Invoker gameInvoker)
    {
        LifecycleEventsInvoker = null;
        GameEventsInvoker = null;
    }

    /// <summary>
    /// Executes lifecycle events in order, stopping early if an error message is recorded to the <see cref="Logger"/>.
    /// </summary>
    /// <returns>False if the execution stopped early, otherwise true.</returns>
    public bool RunStartNewLifecycle()
    {
        if (LifecycleEventsInvoker is null)
        {
            throw new NullReferenceException(nameof(LifecycleEventsInvoker));
        }

        IEnumerable<Action> cycle =
        [
            LifecycleEventsInvoker.NotifyBeforeStartNewGame,
            Profile.Load,
            LifecycleEventsInvoker.NotifyOnEnterGame,
            LifecycleEventsInvoker.NotifyAfterStartNewGame,
            LifecycleEventsInvoker.NotifyOnSafeToGiveItems,
        ];

        foreach (Action a in cycle)
        {
            a();
            if (ErrorMessages.Count > 0)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Executes lifecycle events in order, stopping early if an error message is recorded to the <see cref="Logger"/>.
    /// </summary>
    /// <returns>False if the execution stopped early, otherwise true.</returns>
    public bool RunContinueLifecycle()
    {
        if (LifecycleEventsInvoker is null)
        {
            throw new NullReferenceException(nameof(LifecycleEventsInvoker));
        }

        IEnumerable<Action> cycle =
        [
            LifecycleEventsInvoker.NotifyBeforeContinueGame,
            Profile.Load,
            LifecycleEventsInvoker.NotifyOnEnterGame,
            LifecycleEventsInvoker.NotifyAfterContinueGame,
            LifecycleEventsInvoker.NotifyOnSafeToGiveItems,
        ];

        foreach (Action a in cycle)
        {
            a();
            if (ErrorMessages.Count > 0)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Executes lifecycle events in order, stopping early if an error message is recorded to the <see cref="Logger"/>.
    /// </summary>
    /// <returns>False if the execution stopped early, otherwise true.</returns>
    public bool RunLeaveLifecycle()
    {
        if (LifecycleEventsInvoker is null)
        {
            throw new NullReferenceException(nameof(LifecycleEventsInvoker));
        }

        IEnumerable<Action> cycle =
        [
            LifecycleEventsInvoker.NotifyOnLeaveGame,
            Profile.Unload,
        ];

        foreach (Action a in cycle)
        {
            a();
            if (ErrorMessages.Count > 0)
            {
                return false;
            }
        }

        return true;
    }


    public LifecycleEvents.Invoker? LifecycleEventsInvoker { get; private set; }
    public GameEvents.Invoker? GameEventsInvoker { get; private set; }
}
