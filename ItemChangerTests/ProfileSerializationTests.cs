using System.Text;
using ItemChanger;
using ItemChanger.Items;
using ItemChanger.Locations;
using ItemChanger.Modules;
using ItemChanger.Placements;
using ItemChanger.Tags;
using ItemChangerTests.Fixtures;
using Snapshooter.Xunit3;

namespace ItemChangerTests;

[Collection(RequiresHostCollection.NAME)]
public class ProfileSerializationTests : IDisposable
{
    private readonly TestHost host;
    private ItemChangerProfile profile;

    public ProfileSerializationTests()
    {
        host = new TestHost();
        profile = host.Profile;
    }

    public void Dispose()
    {
        profile.Dispose();
        host.Dispose();
    }

    [Fact]
    public void FullWashSerDeIsConsistent()
    {
        profile.Modules.Add(new InteropModule() { Message = "foo" });
        profile.Modules.Add(new InteropModule() { Message = "bar" });

        Item a = CreateTaggedItem("A");
        Item b = CreateTaggedItem("B");
        Item c = CreateTaggedItem("C");
        Placement p = CreatePlacement([a, b, c]);
        profile.AddPlacement(p);

        // new game
        host.RunStartNewLifecycle();

        // first save
        using MemoryStream ms = new();
        host.RunLeaveLifecycle();
        profile.ToStream(ms);
        profile.Dispose();
        byte[] firstSaveSnapshot = ms.ToArray();

        // load from file and continue game
        using MemoryStream ms2 = new(firstSaveSnapshot);
        profile = ItemChangerProfile.FromStream(host, ms2);
        host.RunContinueLifecycle();

        // second save
        using MemoryStream ms3 = new();
        host.RunLeaveLifecycle();
        profile.ToStream(ms3);
        profile.Dispose();
        byte[] secondSaveSnapshot = ms3.ToArray();

        // they'd better be the same!
        string firstJson = Encoding.UTF8.GetString(firstSaveSnapshot);
        string secondJson = Encoding.UTF8.GetString(secondSaveSnapshot);

        Assert.Equal(firstJson, secondJson);
    }

    [Fact]
    public void NonTrivialSerializationMatchesSnapshot()
    {
        profile.Modules.Add(new InteropModule() { Message = "foo" });
        profile.Modules.Add(new InteropModule() { Message = "bar" });

        Item a = CreateTaggedItem("A");
        Item b = CreateTaggedItem("B");
        Item c = CreateTaggedItem("C");
        Placement p = CreatePlacement([a, b, c]);
        profile.AddPlacement(p);

        // new game
        host.RunStartNewLifecycle();

        // save
        using MemoryStream ms = new();
        host.RunLeaveLifecycle();
        profile.ToStream(ms);
        profile.Dispose();
        byte[] snapshot = ms.ToArray();
        string snapshotJson = Encoding.UTF8.GetString(snapshot);

        Snapshot.Match(snapshotJson);
    }

    private Item CreateTaggedItem(string name)
    {
        Item i = new NullItem { name = name };
        i.AddTag(new InteropTag { Message = "test" });
        return i.Clone();
    }

    private Placement CreatePlacement(IEnumerable<Item> items)
    {
        return new AutoPlacement("Test placement")
        {
            Location = new EmptyLocation { Name = "Test location" },
        }.Add(items);
    }
}
