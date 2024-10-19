namespace ItemChanger.Internal;

public static class Ref
{
    public static Settings Settings => ItemChangerMod.SET;


    public static void QuickSave(params AbstractPlacement[] placements) => Settings.SavePlacements(placements);
}
