using ItemChanger.Internal;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ItemChanger.Modules;

public class ModuleCollection : IEnumerable<Module>
{

    [JsonConverter(typeof(ModuleListDeserializer))]
    [JsonProperty]
    private readonly List<Module> modules = [];

    private readonly ItemChangerProfile backingProfile;

    /// <inheritdoc/>
    public int Count => modules.Count;

    internal ModuleCollection(ItemChangerProfile backingProfile)
    {
        this.backingProfile = backingProfile;
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
        if (backingProfile.state >= ItemChangerProfile.LoadState.ModuleLoadCompleted)
        {
            m.LoadOnce();
        }

        return m;
    }

    public T Add<T>() where T : Module, new()
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
            LoggerProxy.LogError($"Unable to instantiate module of type {T.Name} through reflection:\n{e}");
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

    public T GetOrAdd<T>() where T : Module, new()
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
        if (modules.Remove(m) && backingProfile.state >= ItemChangerProfile.LoadState.ModuleLoadCompleted)
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
        if (backingProfile.state >= ItemChangerProfile.LoadState.ModuleLoadCompleted)
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

    internal class ModuleListSerializer : JsonConverter<List<Module>>
    {
        public override bool CanRead => false;
        public override bool CanWrite => true;
        public bool RemoveNewProfileModules;
        public override List<Module> ReadJson(JsonReader reader, Type objectType, List<Module>? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
        public override void WriteJson(JsonWriter writer, List<Module>? value, JsonSerializer serializer)
        {
            if (value is null)
            {
                writer.WriteNull();
                return;
            }

            if (RemoveNewProfileModules)
            {
                value = [.. value.Where(t => !t.ModuleHandlingProperties.HasFlag(ModuleHandlingFlags.RemoveOnNewProfile))];
            }

            serializer.Serialize(writer, value.ToArray());
        }
    }

    internal class ModuleListDeserializer : JsonConverter<List<Module>>
    {
        public override bool CanRead => true;
        public override bool CanWrite => false;

        public override List<Module>? ReadJson(JsonReader reader, Type objectType, List<Module>? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            JToken jt = JToken.Load(reader);
            if (jt.Type == JTokenType.Null)
            {
                return null;
            }
            else if (jt.Type == JTokenType.Array)
            {
                JArray ja = (JArray)jt;
                List<Module> list = new(ja.Count);
                foreach (JToken jModule in ja)
                {
                    Module t;
                    try
                    {
                        t = jModule.ToObject<Module>(serializer)!;
                    }
                    catch (Exception e)
                    {
                        ModuleHandlingFlags flags = ((JObject)jModule).GetValue(nameof(Module.ModuleHandlingProperties))?.ToObject<ModuleHandlingFlags>(serializer) ?? ModuleHandlingFlags.None;
                        if (flags.HasFlag(ModuleHandlingFlags.AllowDeserializationFailure))
                        {
                            t = new InvalidModule
                            {
                                JSON = jModule,
                                DeserializationError = e,
                            };
                        }
                        else
                        {
                            throw;
                        }
                    }
                    list.Add(t);
                }
                return list;
            }
            throw new JsonSerializationException("Unable to handle tag list pattern: " + jt.ToString());
        }

        public override void WriteJson(JsonWriter writer, List<Module>? value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
