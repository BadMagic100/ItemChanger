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
        // A model in which there are 3 items: L,R,S. (from Hollow Knight, Left_Mothwing_Cloak, Right_Mothwing_Cloak, Split_Shade_Cloak).
        // S cannot be given before one of L or R. If S is collected first, it must be replaced. There are no other constraints.
        // Say we are left-biased: S is replaced by L if collected first.
        // Then the condition that items are commutative determines what the items give when collected in any order, in the table below:
        [Theory]
        [InlineData((string[])["L", "R", "S"], (string[])["L", "R", "S"])]
        [InlineData((string[])["L", "S", "R"], (string[])["L", "S", "R"])]
        [InlineData((string[])["S", "L", "R"], (string[])["L", "S", "R"])]
        [InlineData((string[])["S", "R", "L"], (string[])["L", "R", "S"])]
        [InlineData((string[])["R", "L", "S"], (string[])["R", "L", "S"])]
        [InlineData((string[])["R", "S", "L"], (string[])["R", "L", "S"])]
        public void LeftBiasedSplitCloakProgressionTest(string[] input, string[] expectedOutput)
        {
            // item/placement setup
            Item l = CreateTaggedItem("L");
            Item r = CreateTaggedItem("R");
            Item s = CreateTaggedItem("S");
            Dictionary<string, Item> items = ((Item[])[l, r, s]).ToDictionary(i => i.name);
            Placement p = CreatePlacement(input.Select(i => items[i].Clone()));
            // profile setup
            using ItemChangerProfile profile = CreateProfile(out TestHost host);
            profile.AddPlacement(p);
            profile.Modules.Add(new ProgressiveItemGroupModule
            {
                GroupID = "test",
                OrderedMemberList = ["L", "R", "S"],
                OrderedTransitivePredecessorsLookup = new Dictionary<string, List<string>>
                {
                    ["L"] = [],
                    ["R"] = [],
                    ["S"] = ["L"],
                },
            });
            // prepare to monitor item order
            List<string> resultOrder = [];
            void AddToResult(ReadOnlyGiveEventArgs args) => resultOrder.Add(args.Item.name);
            foreach (Item i in p.Items) i.AfterGive += AddToResult;
            // start IC w/o errors
            Assert.True(host.RunStartNewLifecycle());
            // give items
            GiveInfo gi = new();
            p.GiveAll(gi);
            // assert
            Assert.Equal(expectedOutput, resultOrder);
        }

        // A model in which there are 3 items: N,C,E. (from Silksong, Needolin, Beastling's Call, Elegy of the Deep).
        // N must be given before C or E. There are no other constraints.
        // Say we are Call-biased. If C and E are the first two items collected (in either order), we give Needolin and Call.
        // Then the condition that items are commutative determines what the items give when collected in any order, in the table below:
        // 
        [Theory]
        [InlineData((string[])["N", "C", "E"], (string[])["N", "C", "E"])]
        [InlineData((string[])["N", "E", "C"], (string[])["N", "E", "C"])]
        [InlineData((string[])["C", "N", "E"], (string[])["N", "C", "E"])]
        [InlineData((string[])["C", "E", "N"], (string[])["N", "C", "E"])]
        [InlineData((string[])["E", "N", "C"], (string[])["N", "E", "C"])]
        [InlineData((string[])["E", "C", "N"], (string[])["N", "C", "E"])]
        public void NRO_ProgressionTest(string[] input, string[] expectedOutput)
        {
            // item/placement setup
            Item n = CreateTaggedItem("N");
            Item c = CreateTaggedItem("C");
            Item e = CreateTaggedItem("E");
            Dictionary<string, Item> items = ((Item[])[n, c, e]).ToDictionary(i => i.name);
            Placement p = CreatePlacement(input.Select(i => items[i].Clone()));
            // profile setup
            using ItemChangerProfile profile = CreateProfile(out TestHost host);
            profile.AddPlacement(p);
            profile.Modules.Add(new ProgressiveItemGroupModule 
            { 
                GroupID = "test", 
                OrderedMemberList = ["N", "E", "C"],
                OrderedTransitivePredecessorsLookup = new Dictionary<string, List<string>>
                {
                    ["N"] = [],
                    ["C"] = ["N"],
                    ["E"] = ["N"],
                },
            });
            // prepare to monitor item order
            List<string> resultOrder = [];
            void AddToResult(ReadOnlyGiveEventArgs args) => resultOrder.Add(args.Item.name);
            foreach (Item i in p.Items) i.AfterGive += AddToResult;
            // start IC w/o errors
            Assert.True(host.RunStartNewLifecycle());
            // give items
            GiveInfo gi = new();
            p.GiveAll(gi);
            // assert
            Assert.Equal(expectedOutput, resultOrder);
        }

        // an abstract model with four items: M,S,H1,H2.
        // In this model, the first two items must be given before either H1 or H2 can be given. There are no other constraints.
        // We give H1 and H2 different preferences for how to be replaced; H1 prefers to be replaced by M, and H2 prefers to be replaced by S.
        // We test a sampling of permutations of the four items, along with a scenario where duplicates of H1 and H2 are included.
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
            Item m = CreateTaggedItem("M");
            Item s = CreateTaggedItem("S");
            Item h1 = CreateTaggedItem("H1");
            Item h2 = CreateTaggedItem("H2");
            Dictionary<string, Item> items = ((Item[])[m, s, h1, h2]).ToDictionary(i => i.name);
            Placement p = CreatePlacement(input.Select(i => items[i].Clone()));
            // profile setup
            using ItemChangerProfile profile = CreateProfile(out TestHost host);
            profile.AddPlacement(p);
            profile.Modules.Add(new ProgressiveItemGroupModule 
            { 
                GroupID = "test", 
                OrderedMemberList = ["M", "S", "H1", "H2"],
                OrderedTransitivePredecessorsLookup = new Dictionary<string, List<string>>
                {
                    ["M"] = [],
                    ["S"] = [],
                    ["H1"] = ["M", "S"],
                    ["H2"] = ["S", "M"],
                },
            });
            // prepare to monitor item order
            List<string> resultOrder = [];
            void AddToResult(ReadOnlyGiveEventArgs args) => resultOrder.Add(args.Item.name);
            foreach (Item i in p.Items) i.AfterGive += AddToResult;
            // start IC w/o errors
            Assert.True(host.RunStartNewLifecycle());
            // give items
            GiveInfo gi = new();
            p.GiveAll(gi);
            // assert
            Assert.Equal(expectedOutput, resultOrder);
        }

        [Fact]
        public void MissingMemberTest1()
        {
            // profile setup
            using ItemChangerProfile profile = CreateProfile(out TestHost host);
            profile.Modules.Add(new ProgressiveItemGroupModule 
            { 
                GroupID = "test", 
                OrderedMemberList = ["X", "Y", "Z"],
                OrderedTransitivePredecessorsLookup = new Dictionary<string, List<string>>
                {
                    ["X"] = [],
                    ["Y"] = ["X"],
                },
            });
            // start IC w/ errors
            Assert.False(host.RunStartNewLifecycle());
            // retrieve error message
            string err = Assert.Single(host.ErrorMessages)!;
            Output.WriteLine(err);
            Assert.StartsWith("Error initializing module ProgressiveItemGroupModule:\n" +
                "System.InvalidOperationException: " +
                "Item Z appears in data of ProgressiveItemGroupModule with GroupID test, " +
                "but item is not both an entry of the member list and a key of the predecessor lookup.", err);
        }

        [Fact]
        public void MissingMemberTest2()
        {
            // profile setup
            using ItemChangerProfile profile = CreateProfile(out TestHost host);
            profile.Modules.Add(new ProgressiveItemGroupModule
            {
                GroupID = "test",
                OrderedMemberList = ["X", "Y"],
                OrderedTransitivePredecessorsLookup = new Dictionary<string, List<string>>
                {
                    ["X"] = [],
                    ["Y"] = ["X"],
                    ["Z"] = ["X"],
                },
            });
            // start IC w/ errors
            Assert.False(host.RunStartNewLifecycle());
            // retrieve error message
            string err = Assert.Single(host.ErrorMessages)!;
            Output.WriteLine(err);
            Assert.StartsWith("Error initializing module ProgressiveItemGroupModule:\n" +
                "System.InvalidOperationException: " +
                "Item Z appears in data of ProgressiveItemGroupModule with GroupID test, " +
                "but item is not both an entry of the member list and a key of the predecessor lookup.", err);
        }

        [Fact]
        public void MissingMemberTest3()
        {
            // profile setup
            using ItemChangerProfile profile = CreateProfile(out TestHost host);
            profile.Modules.Add(new ProgressiveItemGroupModule
            {
                GroupID = "test",
                OrderedMemberList = ["X", "Y"],
                OrderedTransitivePredecessorsLookup = new Dictionary<string, List<string>>
                {
                    ["X"] = ["Z"],
                    ["Y"] = ["X"],
                },
            });
            // start IC w/ errors
            Assert.False(host.RunStartNewLifecycle());
            // retrieve error message
            string err = Assert.Single(host.ErrorMessages)!;
            Output.WriteLine(err);
            Assert.StartsWith("Error initializing module ProgressiveItemGroupModule:\n" +
                "System.InvalidOperationException: " +
                "Item Z appears in data of ProgressiveItemGroupModule with GroupID test, " +
                "but item is not both an entry of the member list and a key of the predecessor lookup.", err);
        }

        [Fact]
        public void TransitivityTest()
        {
            // profile setup
            using ItemChangerProfile profile = CreateProfile(out TestHost host);
            profile.Modules.Add(new ProgressiveItemGroupModule 
            { 
                GroupID = "test", 
                OrderedMemberList = ["X", "Y", "Z"],
                OrderedTransitivePredecessorsLookup = new Dictionary<string, List<string>>
                {
                    ["X"] = [],
                    ["Y"] = ["X"],
                    ["Z"] = ["Y"],
                },
            });
            // start IC w/ errors
            Assert.False(host.RunStartNewLifecycle());
            // retrieve error message
            string err = Assert.Single(host.ErrorMessages)!;
            Output.WriteLine(err);
            Assert.StartsWith("Error initializing module ProgressiveItemGroupModule:\n" +
                "System.InvalidOperationException: " +
                "ProgressiveItemGroupTag for Z with GroupID test is missing the transitive predecessor X of Y.", err);
        }

        [Fact]
        public void IrreflexivityTest()
        {
            // profile setup
            using ItemChangerProfile profile = CreateProfile(out TestHost host);
            profile.Modules.Add(new ProgressiveItemGroupModule 
            { 
                GroupID = "test", 
                OrderedMemberList = ["X"],
                OrderedTransitivePredecessorsLookup = new Dictionary<string, List<string>>
                {
                    ["X"] = ["X"],
                },
            });
            // start IC w/ errors
            Assert.False(host.RunStartNewLifecycle());
            // retrieve error message
            string err = Assert.Single(host.ErrorMessages)!;
            Output.WriteLine(err);
            Assert.StartsWith("Error initializing module ProgressiveItemGroupModule:\n" +
                "System.InvalidOperationException: " +
                "ProgressiveItemGroupTag for X with GroupID test declares X as its own predecessor.", err);
        }

        [Fact]
        public void OrderConsistencyTest()
        {
            // profile setup
            using ItemChangerProfile profile = CreateProfile(out TestHost host);
            profile.Modules.Add(new ProgressiveItemGroupModule 
            { 
                GroupID = "test", 
                OrderedMemberList = ["Y", "X"],
                OrderedTransitivePredecessorsLookup = new Dictionary<string, List<string>>
                {
                    ["X"] = [],
                    ["Y"] = ["X"],
                },
            });
            // start IC w/ errors
            Assert.False(host.RunStartNewLifecycle());
            // retrieve error message
            string err = Assert.Single(host.ErrorMessages)!;
            Output.WriteLine(err);
            Assert.StartsWith("Error initializing module ProgressiveItemGroupModule:\n" +
                "System.InvalidOperationException: " +
                "Y is declared as a predecessor of X, but Y occurs after X in the OrderedMemberList for ProgressiveItemGroupModule with GroupID test.", err);
        }

        [Fact]
        public void UnexpectedMemberTest()
        {
            // item/placement setup
            Item x = CreateTaggedItem("X");
            Item y = CreateTaggedItem("Y");
            Item z = CreateTaggedItem("Z");
            Placement p = CreatePlacement([x, y, z]);
            // profile setup
            using ItemChangerProfile profile = CreateProfile(out TestHost host);
            profile.AddPlacement(p);
            profile.Modules.Add(new ProgressiveItemGroupModule 
            { 
                GroupID = "test", 
                OrderedMemberList = ["X", "Y"] ,
                OrderedTransitivePredecessorsLookup = new Dictionary<string, List<string>>
                {
                    ["X"] = [],
                    ["Y"] = ["X"],
                },
            });
            // start IC w/ errors
            Assert.False(host.RunStartNewLifecycle());
            // retrieve error message
            string err = Assert.Single(host.ErrorMessages)!;
            Output.WriteLine(err);
            Assert.StartsWith("Error loading ProgressiveItemGroupTag:\n" +
                "System.InvalidOperationException: " +
                "Item Z tagged with ProgressiveItemGroupTag with GroupID test was not declared on the module.", err);
        }


        private Item CreateTaggedItem(string name)
        {
            Item i = new NullItem { name = name, };
            i.AddTag(new ProgressiveItemGroupTag { GroupID = "test", });
            Finder.DefineItem(i, overwrite:true);
            return i.Clone();
        }

        private Placement CreatePlacement(IEnumerable<Item> items)
        {
            return new AutoPlacement("Test placement") { Location = new EmptyLocation { Name = "Test location" } }.Add(items);
        }

        private ItemChangerProfile CreateProfile(out TestHost host)
        {
            host = new(Output);
            return host.Profile;
        }
    }
}