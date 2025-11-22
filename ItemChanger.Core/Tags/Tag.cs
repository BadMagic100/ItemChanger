using System;
using ItemChanger.Enums;
using ItemChanger.Logging;
using Newtonsoft.Json;

namespace ItemChanger.Tags;

/// <summary>
/// Base class for lightweight metadata objects that can be attached to placements, items, locations, and other taggable objects.
/// </summary>
public abstract class Tag
{
    /// <summary>
    /// Whether the tag has been loaded
    /// </summary>
    [JsonIgnore]
    public bool Loaded { get; private set; }

    /// <summary>
    /// Method to implement optional loading logic, called once during loading.
    /// </summary>
    /// <param name="parent">The object this tag is applied to</param>
    protected virtual void DoLoad(TaggableObject parent) { }

    /// <summary>
    /// Method to implement optional unloading logic, called once during unloading.
    /// </summary>
    /// <param name="parent">The object this tag is applied to</param>
    protected virtual void DoUnload(TaggableObject parent) { }

    /// <summary>
    /// Loads the tag. If the tag is already loaded, does nothing.
    /// </summary>
    public void LoadOnce(TaggableObject parent)
    {
        if (!Loaded)
        {
            try
            {
                DoLoad(parent);
            }
            catch (Exception e)
            {
                LoggerProxy.LogError($"Error loading {GetType().Name}:\n{e}");
            }
            Loaded = true;
        }
    }

    /// <summary>
    /// Unloads the tag. If the tag is not loaded, does nothing.
    /// </summary>
    public void UnloadOnce(TaggableObject parent)
    {
        if (Loaded)
        {
            try
            {
                DoUnload(parent);
            }
            catch (Exception e)
            {
                LoggerProxy.LogError($"Error unloading {GetType().Name}:\n{e}");
            }
            Loaded = false;
        }
    }

    /// <summary>
    /// Additional information for serialization and other tag handling purposes.
    /// </summary>
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    public virtual TagHandlingFlags TagHandlingProperties { get; set; }
}
