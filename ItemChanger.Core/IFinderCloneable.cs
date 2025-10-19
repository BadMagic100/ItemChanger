using System;
using ItemChanger.Serialization;
using Newtonsoft.Json.Linq;

namespace ItemChanger;

/// <summary>
/// Marker interface for types that can be deep copied when retrieved from <see cref="Finder"/>
/// </summary>
public interface IFinderCloneable { }

internal static class FinderCloneableExtensions
{
    public static T? DeepClone<T>(this T? t)
        where T : IFinderCloneable
    {
        if (t == null)
        {
            return default;
        }
        Type runtimeType = t.GetType();
        JObject ser = JObject.FromObject(t, SerializationHelper.Serializer);
        return (T)ser.ToObject(runtimeType, SerializationHelper.Serializer)!;
    }
}
