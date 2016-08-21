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
            private string PrivateTextProp => privateText;
            private string privateText;

            public void SetPrivates(string text) => privateText = text;
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

            var deepchild = new Blob { Child = parentblob };

            parentblob.Child = new Blob { Child = new Blob { Child = new Blob { Child = deepchild } } };

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

        private class ThrowException
        {
            public string Prop { get { throw null; } }
        }

        [Fact]
        private void should_ignore_exceptions_thrown_by_getters()
        {
            var blob = new ThrowException();

            var dictionary = ObjectMerging.ToDictionary(blob);

            var child = (Dictionary<string, object>)(dictionary["Prop"]);

            Assert.Equal(child["OcelogWarning"], "Exception thrown by invocation");
        }

        [Fact]
        private void should_ignore_properties_that_are_delegate_types()
        {
            var didExecute = false;
            Action execute = () => { didExecute = true; };
            Action methodGroup = this.should_ignore_properties_that_are_delegate_types;
            var blob = new { ThingToExecute = execute, ThingToExecute2 = methodGroup };

            var dictionary = ObjectMerging.ToDictionary(blob);

            Assert.False(didExecute);
            Assert.False(dictionary.ContainsKey("ThingToExecute"));
            Assert.False(dictionary.ContainsKey("ThingToExecute2"));
        }

        [Fact]
        private void should_ignore_privates()
        {
            var blob = new Blob();
            blob.SetPrivates("hello");

            var dictionary = ObjectMerging.ToDictionary(blob);

            Assert.False(dictionary.ContainsKey("PrivateTextProp"));
            Assert.False(dictionary.ContainsKey("privateText"));
        }

        [Fact]
        private void should_serialise_exceptions_targetsite()
        {
            var blob = GetException();

            var dictionary = ObjectMerging.ToDictionary(blob);

            Assert.True(dictionary.ContainsKey("TargetSite"));
            Assert.Equal(blob.TargetSite.ToString(), dictionary["TargetSite"]);
        }

        [Fact]
        private void should_merge_unboxed_arrays()
        {
            var blob = new { Ids = new[] { 12 } };
            var blob2 = new { Ids = new[] { 34 } };

            var dictionary = ObjectMerging.Flatten(new[] { blob, blob2 });

            Assert.True(dictionary.ContainsKey("Ids"));
            Assert.Collection<object>((IEnumerable<object>)dictionary["Ids"],
                id => Assert.Equal(12, id),
                id => Assert.Equal(34, id));
        }

        [Fact]
        private void should_merge_collections()
        {
            var blob = new { Ids = new[] { 12 }.ToList() };
            var blob2 = new { Ids = new[] { 34 }.ToList() };

            var dictionary = ObjectMerging.Flatten(new[] { blob, blob2 });

            Assert.True(dictionary.ContainsKey("Ids"));
            Assert.Collection<object>((IEnumerable<object>)dictionary["Ids"],
                id => Assert.Equal(12, id),
                id => Assert.Equal(34, id));
        }

        [Fact]
        private void should_merge_collections_of_mixed_types()
        {
            var blob = new { Ids = new[] { 12 }.ToList() };
            var blob2 = new { Ids = new[] { "34" }.ToList() };
            var blob3 = new { Ids = new[] { true } };

            var dictionary = ObjectMerging.Flatten(new object[] { blob, blob2, blob3 });

            Assert.True(dictionary.ContainsKey("Ids"));
            Assert.Collection<object>((IEnumerable<object>)dictionary["Ids"],
                id => Assert.Equal(12, id),
                id => Assert.Equal("34", id),
                id => Assert.Equal(true, id));
        }

        private Exception GetException()
        {
            try { throw null; }
            catch (Exception e) { return e; }
        }
    }
}
