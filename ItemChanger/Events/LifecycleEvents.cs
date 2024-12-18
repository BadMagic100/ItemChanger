﻿using ItemChanger.Extensions;
using System;

namespace ItemChanger.Events;

public static class LifecycleEvents
{
    public static event Action? BeforeStartNewGame;
    public static event Action? AfterStartNewGame;
    public static event Action? AfterContinueGame;
    public static event Action? OnEnterGame;
    public static event Action? OnSafeToGiveItems;
    public static event Action? OnLeaveGame;
    public static event Action? OnErrorMessageRequested;

    internal static void NotifyError()
    {
        OnErrorMessageRequested?.GetInvocationList().InvokeAllSafely();
    }
}
