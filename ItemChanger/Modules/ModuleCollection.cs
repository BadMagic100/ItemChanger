using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ItemChanger.Serialization.Converters;
using Newtonsoft.Json;

namespace ItemChanger.Modules;

[JsonConverter(typeof(ModuleCollectionConverter))]
public class ModuleCollection : IEnumerable<Module>
{
    private readonly List<Module> modules;

    /// <inheritdoc/>
    public int Count => modules.Count;

    internal ModuleCollection()
    {
        this.modules = [];
    }

    public void Load()
    {
        for (int i = 0; i < modules.Count; i++)
        {
            modules[i].LoadOnce();
        }
    }

    public void Unload()
    {
        for (int i = 0; i < modules.Count; i++)
        {
            modules[i].UnloadOnce();
        }
    }

    public Module Add(Module m)
    {
        if (m == null)
        {
            throw new ArgumentNullException(nameof(m));
        }

        modules.Add(m);
        if (
            ItemChangerHost.Singleton.ActiveProfile != null
            && ItemChangerHost.Singleton.ActiveProfile.state
                >= ItemChangerProfile.LoadState.ModuleLoadCompleted
        )
        {
            m.LoadOnce();
        }

        return m;
    }

    public T Add<T>()
        where T : Module, new()
    {
        T t = new();
        return (T)Add(t);
    }

    public Module Add(Type T)
    {
        try
        {
            Module m = (Module)Activator.CreateInstance(T)!;
            return Add(m);
        }
        catch (Exception e)
        {
            LoggerProxy.LogError(
                $"Unable to instantiate module of type {T.Name} through reflection:\n{e}"
            );
            throw;
        }
    }

    /// <summary>
    /// Returns the first module of type T, or default.
    /// </summary>
    public T? Get<T>()
    {
        return modules.OfType<T>().FirstOrDefault();
    }

    public T GetOrAdd<T>()
        where T : Module, new()
    {
        T? t = modules.OfType<T>().FirstOrDefault();
        t ??= Add<T>();

        return t;
    }

    public Module GetOrAdd(Type T)
    {
        Module? m = modules.FirstOrDefault(m => T.IsInstanceOfType(m));
        m ??= Add(T);

        return m;
    }

    public void Remove(Module m)
    {
        if (
            modules.Remove(m)
            && ItemChangerHost.Singleton.ActiveProfile != null
            && ItemChangerHost.Singleton.ActiveProfile.state
                >= ItemChangerProfile.LoadState.ModuleLoadCompleted
        )
        {
            m.UnloadOnce();
        }
    }

    public void Remove<T>()
    {
        if (modules.OfType<T>().FirstOrDefault() is Module m)
        {
            Remove(m);
        }
    }

    public void Remove(Type T)
    {
        if (
            ItemChangerHost.Singleton.ActiveProfile != null
            && ItemChangerHost.Singleton.ActiveProfile.state
                >= ItemChangerProfile.LoadState.ModuleLoadCompleted
        )
        {
            foreach (Module m in modules.Where(m => m.GetType() == T))
            {
                m.UnloadOnce();
            }
        }
        modules.RemoveAll(m => m.GetType() == T);
    }

    public void Remove(string name)
    {
        if (modules.Where(m => m.Name == name).FirstOrDefault() is Module m)
        {
            Remove(m);
        }
    }

    /// <inheritdoc/>
    public IEnumerator<Module> GetEnumerator() => modules.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
