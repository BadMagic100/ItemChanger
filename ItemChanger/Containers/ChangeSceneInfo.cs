using ItemChanger.Tags;

namespace ItemChanger.Containers;

/// <summary>
/// Instructions for a container to change scene.
/// </summary>
public class ChangeSceneInfo
{
    public const string door_dreamReturn = "door_dreamReturn";

    public Transition transition;
    public bool dreamReturn;
    public bool deactivateNoCharms;

    public bool applied;

    public ChangeSceneInfo() { }
    public ChangeSceneInfo(Transition transition)
    {
        this.transition = transition;
        dreamReturn = deactivateNoCharms = transition.GateName == door_dreamReturn;
    }
    public ChangeSceneInfo(Transition transition, bool dreamReturn)
    {
        this.transition = transition;
        this.dreamReturn = deactivateNoCharms = dreamReturn;
    }
    public ChangeSceneInfo(Transition transition, bool dreamReturn, bool deactivateNoCharms)
    {
        this.transition = transition;
        this.dreamReturn = dreamReturn;
        this.deactivateNoCharms = deactivateNoCharms;
    }
    public ChangeSceneInfo(ChangeSceneTag tag)
    {
        transition = tag.changeTo;
        dreamReturn = tag.dreamReturn;
        deactivateNoCharms = tag.deactivateNoCharms;
    }
}
