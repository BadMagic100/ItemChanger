namespace ItemChanger.Locations;

/// <summary>
/// Location which places a container at a specified coordinate position.
/// </summary>
public class CoordinateLocation : PlaceableLocation
{
    public required float X { get; init; }
    public required float Y { get; init; }
    public required float Z { get; init; }

    protected override void OnLoad()
    {
        Events.AddSceneChangeEdit(UnsafeSceneName, OnActiveSceneChanged);
    }

    protected override void OnUnload()
    {
        Events.RemoveSceneChangeEdit(UnsafeSceneName, OnActiveSceneChanged);
    }

    public void OnActiveSceneChanged(Scene to)
    {
        if (!Managed && to.name == UnsafeSceneName)
        {
            base.GetContainer(out GameObject obj, out string containerType);
            PlaceContainer(obj, containerType);
        }
    }

    public override void PlaceContainer(GameObject obj, string containerType)
    {
        Container.GetContainer(containerType)!.ApplyTargetContext(obj, X, Y, Z);
        if (!obj.activeSelf)
        {
            obj.SetActive(true);
        }
    }
}
