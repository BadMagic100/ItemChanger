namespace ItemChanger.Items;

public class NullItem : Item
{
    public static NullItem Create()
    {
        return new NullItem
        {
            name = "Nothing",
        };
    }

    public override void GiveImmediate(GiveInfo info)
    {
        // intentional no-op
    }
}
