using System;
using System.Collections;
using System.Collections.Generic;

namespace ItemChanger.Containers;

/// <summary>
/// Central lookup for all container definitions and defaults.
/// </summary>
public class ContainerRegistry : IEnumerable<Container>
{
    /// <summary>
    /// Default container type name when no specific type is available.
    /// </summary>
    public const string UnknownContainerType = "Unknown";

    private readonly Dictionary<string, Container> containers = [];

    /// <summary>
    /// The default container to be used when a single item needs a container without
    /// any preference being specified by an item, location, or capability requirement.
    /// DefaultSingleItemContainer is expected to support all capabilities defined by ItemChanger
    /// as well as the game.
    /// </summary>
    public required Container DefaultSingleItemContainer
    {
        get => field;
        init
        {
            if (!containers.ContainsKey(value.Name))
            {
                DefineContainer(value);
            }

            field = value;
        }
    }

    /// <summary>
    /// The default container to be used when multiple items need a container with
    /// any preference being specified by an item, location, or capability requirement.
    /// </summary>
    /// <remarks>
    /// DefaultMultiItemContainer need not necessarily support all capabilities, but if it does
    /// not, DefaultSingleItemContainer will be used instead.
    /// </remarks>
    public required Container DefaultMultiItemContainer
    {
        get => field;
        init
        {
            if (!containers.ContainsKey(value.Name))
            {
                DefineContainer(value);
            }

            field = value;
        }
    }

    /// <summary>
    /// Gets the container definition for the given string. Returns null if no such container has been defined.
    /// </summary>
    public Container? GetContainer(string containerType)
    {
        if (string.IsNullOrEmpty(containerType))
        {
            return null;
        }

        if (containers.TryGetValue(containerType, out Container? value))
        {
            return value;
        }

        return null;
    }

    /// <summary>
    /// Adds the container definition to the internal lookup.
    /// </summary>
    public void DefineContainer(Container container)
    {
        if (containers.ContainsKey(container.Name))
        {
            throw new ArgumentException(
                $"It is not allowed to redefine the existing container type {container.Name}",
                nameof(container)
            );
        }
        containers[container.Name] = container;
    }

    /// <inheritdoc/>
    public IEnumerator<Container> GetEnumerator() => containers.Values.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
