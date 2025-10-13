﻿using System.Collections.Generic;
using System.Linq;
using ItemChanger.Serialization.Converters;
using ItemChanger.Tags;
using Newtonsoft.Json;

namespace ItemChanger;

public class TaggableObject
{
    [JsonProperty]
    [JsonConverter(typeof(TagListDeserializer))]
    public List<Tag>? tags;

    private bool _tagsLoaded;

    protected void LoadTags()
    {
        _tagsLoaded = true;
        if (tags == null)
        {
            return;
        }

        for (int i = 0; i < tags.Count; i++)
        {
            tags[i].LoadOnce(this);
        }
    }

    protected void UnloadTags()
    {
        _tagsLoaded = false;
        if (tags == null)
        {
            return;
        }

        for (int i = 0; i < tags.Count; i++)
        {
            tags[i].UnloadOnce(this);
        }
    }

    public T AddTag<T>()
        where T : Tag, new()
    {
        tags ??= new();
        T t = new();
        if (_tagsLoaded)
        {
            t.LoadOnce(this);
        }

        tags.Add(t);
        return t;
    }

    public void AddTag(Tag t)
    {
        tags ??= new();
        if (_tagsLoaded)
        {
            t.LoadOnce(this);
        }

        tags.Add(t);
    }

    public void AddTags(IEnumerable<Tag> ts)
    {
        tags ??= new();
        if (_tagsLoaded)
        {
            foreach (Tag t in ts)
            {
                t.LoadOnce(this);
            }
        }

        tags.AddRange(ts);
    }

    public T? GetTag<T>()
    {
        return tags == null ? default : tags.OfType<T>().FirstOrDefault();
    }

    public bool GetTag<T>(out T t)
        where T : class
    {
        t = GetTag<T>()!;
        return t != null;
    }

    public IEnumerable<T> GetTags<T>()
    {
        return tags?.OfType<T>() ?? Enumerable.Empty<T>();
    }

    public T GetOrAddTag<T>()
        where T : Tag, new()
    {
        tags ??= new List<Tag>();
        return tags.OfType<T>().FirstOrDefault() ?? AddTag<T>();
    }

    public bool HasTag<T>()
        where T : Tag
    {
        return tags?.OfType<T>()?.Any() ?? false;
    }

    public void RemoveTags<T>()
    {
        if (_tagsLoaded && tags != null)
        {
            foreach (Tag t in tags.Where(t => t is T))
            {
                t.UnloadOnce(this);
            }
        }
        tags = tags?.Where(t => t is not T)?.ToList();
    }
}
