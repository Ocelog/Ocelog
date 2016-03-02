using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Ocelog.Test
{
    public class ObjectMergingTests
    {
        private class Blob
        {
            public Blob Child { get; set; }
            public Blob[] Children { get; set; }
            public IEnumerable<Blob> ChildrenList { get; set; }
        }

        [Fact]
        private void should_replace_circular_refs_with_a_flag()
        {
            var blob = new Blob();
            blob.Child = blob;

            var dictionary = ObjectMerging.ToDictionary(blob);

            var child = (Dictionary<string, object>)(dictionary["Child"]);

            Assert.Equal(child["OcelogWarning"], "Found a Circular Reference");
        }

        [Fact]
        private void should_replace_deep_circular_refs_with_a_flag()
        {
            var parentblob = new Blob();

            var deepchild = new Blob() { Child = parentblob };

            parentblob.Child = new Blob() { Child = new Blob() { Child = new Blob() { Child = deepchild } } };

            var dictionary = ObjectMerging.ToDictionary(parentblob);

            var child = (Dictionary<string, object>)(dictionary["Child"]);
            child = (Dictionary<string, object>)(child["Child"]);
            child = (Dictionary<string, object>)(child["Child"]);
            child = (Dictionary<string, object>)(child["Child"]);
            child = (Dictionary<string, object>)(child["Child"]);

            Assert.Equal(child["OcelogWarning"], "Found a Circular Reference");
        }

        [Fact]
        private void should_replace_circular_refs_in_array_with_a_flag()
        {
            var blob = new Blob();
            blob.Children = new[] { blob };

            var dictionary = ObjectMerging.ToDictionary(blob);

            var children = (IEnumerable<object>)dictionary["Children"];
            var child = (Dictionary<string, object>)children.First();

            Assert.Equal(child["OcelogWarning"], "Found a Circular Reference");
        }

        [Fact]
        private void should_replace_circular_refs_in_enumerable_with_a_flag()
        {
            var blob = new Blob();
            blob.ChildrenList = Enumerable.Repeat(blob, 1);

            var dictionary = ObjectMerging.ToDictionary(blob);

            var children = (IEnumerable<object>)dictionary["ChildrenList"];
            var child = (Dictionary<string, object>)children.First();

            Assert.Equal(child["OcelogWarning"], "Found a Circular Reference");
        }
    }
}
