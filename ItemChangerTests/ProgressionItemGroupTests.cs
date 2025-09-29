using ItemChanger;
using ItemChanger.Internal;
using ItemChanger.Items;
using ItemChanger.Locations;
using ItemChanger.Modules;
using ItemChanger.Placements;
using ItemChanger.Tags;
using Xunit.Abstractions;


namespace ItemChangerTests
{
    public class ProgressionItemGroupTests(ITestOutputHelper Output)
    {
        [Theory]
        [InlineData((string[])["L", "R", "S"], (string[])["L", "R", "S"])]
        [InlineData((string[])["L", "S", "R"], (string[])["L", "S", "R"])]
        [InlineData((string[])["S", "L", "R"], (string[])["L", "S", "R"])]
        [InlineData((string[])["S", "R", "L"], (string[])["L", "R", "S"])]
        [InlineData((string[])["R", "L", "S"], (string[])["R", "L", "S"])]
        [InlineData((string[])["R", "S", "L"], (string[])["R", "L", "S"])]
        public void RLS_ProgressionTest(string[] input, string[] expectedOutput)
        {
            // item/placement setup
            Item l = CreateItem("L", "test", []);
            Item r = CreateItem("R", "test", []);
            Item s = CreateItem("S", "test", ["L"]);
            Dictionary<string, Item> items = ((Item[])[l, r, s]).ToDictionary(i => i.name);
            Placement p = CreatePlacement(input.Select(i => items[i]));
            // profile setup
            using ItemChangerProfile profile = CreateProfile(out TestHost host);
            profile.AddPlacement(p);
            profile.Modules.Add(new ProgressiveItemGroupModule { GroupID = "test", OrderedMemberList = ["L", "R", "S"] });
            // prepare to monitor item order
            List<string> resultOrder = [];
            void AddToResult(ReadOnlyGiveEventArgs args) => resultOrder.Add(args.Item.name);
            foreach (Item i in (Item[])[l, r, s]) i.AfterGive += AddToResult;
            // start IC
            host.LifecyleEventsInvoker!.NotifyBeforeStartNewGame();
            profile.Load();
            host.LifecyleEventsInvoker.NotifyOnEnterGame();
            host.LifecyleEventsInvoker!.NotifyAfterStartNewGame();
            host.LifecyleEventsInvoker!.NotifyOnSafeToGiveItems();
            // give items
            GiveInfo gi = new();
            p.GiveAll(gi);
            // assert
            Assert.Equal(expectedOutput, resultOrder);
        }

        [Theory]
        [InlineData((string[])["N", "R", "O"], (string[])["N", "R", "O"])]
        [InlineData((string[])["N", "O", "R"], (string[])["N", "O", "R"])]
        [InlineData((string[])["R", "N", "O"], (string[])["N", "R", "O"])]
        [InlineData((string[])["R", "O", "N"], (string[])["N", "R", "O"])]
        [InlineData((string[])["O", "N", "R"], (string[])["N", "O", "R"])]
        [InlineData((string[])["O", "R", "N"], (string[])["N", "R", "O"])]
        public void NRO_ProgressionTest(string[] input, string[] expectedOutput)
        {
            // item/placement setup
            Item n = CreateItem("N", "test", []);
            Item r = CreateItem("R", "test", ["N"]);
            Item o = CreateItem("O", "test", ["N"]);
            Dictionary<string, Item> items = ((Item[])[n, r, o]).ToDictionary(i => i.name);
            Placement p = CreatePlacement(input.Select(i => items[i]));
            // profile setup
            using ItemChangerProfile profile = CreateProfile(out TestHost host);
            profile.AddPlacement(p);
            profile.Modules.Add(new ProgressiveItemGroupModule { GroupID = "test", OrderedMemberList = ["N", "O", "R"] });
            // prepare to monitor item order
            List<string> resultOrder = [];
            void AddToResult(ReadOnlyGiveEventArgs args) => resultOrder.Add(args.Item.name);
            foreach (Item i in (Item[])[n, r, o]) i.AfterGive += AddToResult;
            // start IC
            host.LifecyleEventsInvoker!.NotifyBeforeStartNewGame();
            profile.Load();
            host.LifecyleEventsInvoker.NotifyOnEnterGame();
            host.LifecyleEventsInvoker!.NotifyAfterStartNewGame();
            host.LifecyleEventsInvoker!.NotifyOnSafeToGiveItems();
            // give items
            GiveInfo gi = new();
            p.GiveAll(gi);
            // assert
            Assert.Equal(expectedOutput, resultOrder);
        }

        [Theory]
        [InlineData((string[])["M", "S", "H1", "H2"], (string[])["M", "S", "H1", "H2"])]
        [InlineData((string[])["H1", "S", "M", "H2"], (string[])["M", "S", "H1", "H2"])]
        [InlineData((string[])["H2", "H1", "S", "M"], (string[])["S", "M", "H2", "H1"])]
        [InlineData((string[])["H1", "H2", "S", "M"], (string[])["M", "S", "H2", "H1"])]
        [InlineData((string[])["H1", "H2", "M", "S"], (string[])["M", "S", "H2", "H1"])]
        [InlineData((string[])["H2", "H2", "H1", "H1", "M", "S", "H1", "H2"], (string[])["S", "M", "H2", "H2", "H1", "H1", "H1", "H2"])]
        public void MSH1H2_ProgressionTest(string[] input, string[] expectedOutput)
        {
            // item/placement setup
            Item m = CreateItem("M", "test", []);
            Item s = CreateItem("S", "test", []);
            Item h1 = CreateItem("H1", "test", ["M", "S"]);
            Item h2 = CreateItem("H2", "test", ["S", "M"]);
            Dictionary<string, Item> items = ((Item[])[m, s, h1, h2]).ToDictionary(i => i.name);
            Placement p = CreatePlacement(input.Select(i => items[i]));
            // profile setup
            using ItemChangerProfile profile = CreateProfile(out TestHost host);
            profile.AddPlacement(p);
            profile.Modules.Add(new ProgressiveItemGroupModule { GroupID = "test", OrderedMemberList = ["M", "S", "H1", "H2"] });
            // prepare to monitor item order
            List<string> resultOrder = [];
            void AddToResult(ReadOnlyGiveEventArgs args) => resultOrder.Add(args.Item.name);
            foreach (Item i in (Item[])[m, s, h1, h2]) i.AfterGive += AddToResult;
            // start IC
            host.LifecyleEventsInvoker!.NotifyBeforeStartNewGame();
            profile.Load();
            host.LifecyleEventsInvoker.NotifyOnEnterGame();
            host.LifecyleEventsInvoker!.NotifyAfterStartNewGame();
            host.LifecyleEventsInvoker!.NotifyOnSafeToGiveItems();
            // give items
            GiveInfo gi = new();
            p.GiveAll(gi);
            // assert
            Assert.Equal(expectedOutput, resultOrder);
        }


        [Fact]
        public void TransitivityTest()
        {
            // item/placement setup
            Item x = CreateItem("X", "test", []);
            Item y = CreateItem("Y", "test", ["X"]);
            Item z = CreateItem("Z", "test", ["Y"]);
            Placement p = CreatePlacement([x, y, z]);
            // profile setup
            using ItemChangerProfile profile = CreateProfile(out TestHost host);
            profile.AddPlacement(p);
            profile.Modules.Add(new ProgressiveItemGroupModule { GroupID = "test", OrderedMemberList = ["X", "Y", "Z"] });
            // start IC
            host.LifecyleEventsInvoker!.NotifyBeforeStartNewGame();
            profile.Load();
            host.LifecyleEventsInvoker.NotifyAfterStartNewGame();
            host.LifecyleEventsInvoker.NotifyOnEnterGame();
            // retrieve error message
            string err = Assert.Single(host.ErrorMessages)!;
            Output.WriteLine(err);
            Assert.StartsWith("Error thrown by a subscriber during NotifyOnEnterGame:\n" +
                "System.InvalidOperationException: " +
                "ProgressiveItemGroupTag for Z with GroupID test is missing the transitive predecessor X of Y.", err);
        }

        [Fact]
        public void IrreflexivityTest()
        {
            // item/placement setup
            Item x = CreateItem("X", "test", ["X"]);
            Placement p = CreatePlacement([x]);
            // profile setup
            using ItemChangerProfile profile = CreateProfile(out TestHost host);
            profile.AddPlacement(p);
            profile.Modules.Add(new ProgressiveItemGroupModule { GroupID = "test", OrderedMemberList = ["X"] });
            // start IC
            host.LifecyleEventsInvoker!.NotifyBeforeStartNewGame();
            profile.Load();
            host.LifecyleEventsInvoker.NotifyAfterStartNewGame();
            host.LifecyleEventsInvoker.NotifyOnEnterGame();
            // retrieve error message
            string err = Assert.Single(host.ErrorMessages)!;
            Output.WriteLine(err);
            Assert.StartsWith("Error thrown by a subscriber during NotifyOnEnterGame:\n" +
                "System.InvalidOperationException: " +
                "ProgressiveItemGroupTag for X with GroupID test declares X as its own predecessor.", err);
        }

        [Fact]
        public void OrderConsistencyTest()
        {
            // item/placement setup
            Item x = CreateItem("X", "test", []);
            Item y = CreateItem("Y", "test", ["X"]);
            Placement p = CreatePlacement([x, y]);
            // profile setup
            using ItemChangerProfile profile = CreateProfile(out TestHost host);
            profile.AddPlacement(p);
            profile.Modules.Add(new ProgressiveItemGroupModule { GroupID = "test", OrderedMemberList = ["Y", "X"] });
            // start IC
            host.LifecyleEventsInvoker!.NotifyBeforeStartNewGame();
            profile.Load();
            host.LifecyleEventsInvoker.NotifyAfterStartNewGame();
            host.LifecyleEventsInvoker.NotifyOnEnterGame();
            // retrieve error message
            string err = Assert.Single(host.ErrorMessages)!;
            Output.WriteLine(err);
            Assert.StartsWith("Error thrown by a subscriber during NotifyOnEnterGame:\n" +
                "System.InvalidOperationException: " +
                "Y is declared as a predecessor of X, but Y occurs after X in the OrderedMemberList for ProgressiveItemGroupModule with GroupID test.", err);
        }

        [Fact]
        public void MissingMemberTest()
        {
            // item/placement setup
            Item x = CreateItem("X", "test", []);
            Item y = CreateItem("Y", "test", ["X"]);
            Placement p = CreatePlacement([x, y]);
            // profile setup
            using ItemChangerProfile profile = CreateProfile(out TestHost host);
            profile.AddPlacement(p);
            profile.Modules.Add(new ProgressiveItemGroupModule { GroupID = "test", OrderedMemberList = ["X", "Y", "Z"] });
            // start IC
            host.LifecyleEventsInvoker!.NotifyBeforeStartNewGame();
            profile.Load();
            host.LifecyleEventsInvoker.NotifyAfterStartNewGame();
            host.LifecyleEventsInvoker.NotifyOnEnterGame();
            // retrieve error message
            string err = Assert.Single(host.ErrorMessages)!;
            Output.WriteLine(err);
            Assert.StartsWith("Error thrown by a subscriber during NotifyOnEnterGame:\n" +
                "System.Collections.Generic.KeyNotFoundException: " +
                "Item Z was not loaded with a ProgressiveItemGroupTag with GroupID test.", err);
        }

        [Fact]
        public void UnexpectedMemberTest()
        {
            // item/placement setup
            Item x = CreateItem("X", "test", []);
            Item y = CreateItem("Y", "test", ["X"]);
            Item z = CreateItem("Z", "test", ["Y"]);
            Placement p = CreatePlacement([x, y, z]);
            // profile setup
            using ItemChangerProfile profile = CreateProfile(out TestHost host);
            profile.AddPlacement(p);
            profile.Modules.Add(new ProgressiveItemGroupModule { GroupID = "test", OrderedMemberList = ["X", "Y"] });
            // start IC
            host.LifecyleEventsInvoker!.NotifyBeforeStartNewGame();
            profile.Load();
            // retrieve error message
            string err = Assert.Single(host.ErrorMessages)!;
            Output.WriteLine(err);
            Assert.StartsWith("Error loading ProgressiveItemGroupTag:\n" +
                "System.InvalidOperationException: " +
                "Item Z tagged with ProgressiveItemGroupTag with GroupID test was not declared on the module.", err);
        }


        private Item CreateItem(string name, string groupID, List<string> predecessors)
        {
            Item i = new NullItem { name = name, };
            i.AddTag(new ProgressiveItemGroupTag { GroupID = groupID, OrderedTransitivePredecessors = predecessors });
            i.AfterGive += args => args.Orig.RefreshObtained(); // convenience for testing duplicate items
            return i;
        }

        private Placement CreatePlacement(IEnumerable<Item> items) => 
            new AutoPlacement("Test placement") { Location = new EmptyLocation { Name = "Test location" } }.Add(items);
        private ItemChangerProfile CreateProfile(out TestHost host) => new(host = new TestHost(Output));
    }
}