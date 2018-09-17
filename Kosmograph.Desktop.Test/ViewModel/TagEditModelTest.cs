using Kosmograph.Desktop.ViewModel;
using Kosmograph.Model;
using System;
using System.Linq;
using Xunit;

namespace Kosmograph.Desktop.Test.ViewModel
{
    public class TagEditModelTest
    {
        [Fact]
        public void TagEditModel_mirrors_TagViewModel()
        {
            // ARRANGE

            var tag = new TagViewModel(
                new Tag("tag", new Facet("facet", new FacetProperty("p"))));
            var editTag = new TagEditModel(tag, delegate { }, delegate { });

            // ASSERT

            Assert.Equal("tag", editTag.Name);
            Assert.Single(editTag.Properties);
        }

        [Fact]
        public void TagEditModel_delays_changes_at_TagViewModel()
        {
            // ARRANGE

            var p2 = new FacetProperty("p2");
            var tag = new TagViewModel(new Tag("tag", new Facet("facet", new FacetProperty("p"))));
            var editTag = new TagEditModel(tag, delegate { }, delegate { });

            // ACT

            editTag.Name = "changed";
            editTag.RemovePropertyCommand.Execute(editTag.Properties.Single());
            editTag.CreatePropertyCommand.Execute(null);

            // ASSERT

            Assert.Equal("tag", tag.Name);
            Assert.Equal("changed", editTag.Name);
            Assert.Equal("new property", editTag.Properties.Single().Name);
            Assert.Single(tag.Properties);
        }

        [Fact]
        public void TagEditModel_commits_changes_to_TagViewModel()
        {
            // ARRANGE

            var p2 = new FacetProperty("p2");
            var tag = new TagViewModel(new Tag("tag", new Facet("facet", new FacetProperty("p"))));

            Tag committed = null;
            Action<Tag> commitCB = t => committed = t;

            Tag rolledback = null;
            Action<Tag> rollbackCB = t => rolledback = t;

            var editTag = new TagEditModel(tag, commitCB, rollbackCB);
            editTag.Name = "changed";
            editTag.RemovePropertyCommand.Execute(editTag.Properties.Single());
            editTag.CreatePropertyCommand.Execute(null);

            // ACT

            editTag.CommitCommand.Execute(null);

            // ASSERT

            Assert.Equal(tag.Model, committed);
            Assert.Null(rolledback);
            Assert.Equal("changed", tag.Name);
            Assert.Equal("new property", tag.Properties.Single().Name);
        }

        [Fact]
        public void TagEditModel_reverts_notfies_of_rollback()
        {
            // ARRANGE

            var tag = new TagViewModel(new Tag("tag", new Facet("facet", new FacetProperty("p"))));

            Tag committed = null;
            Action<Tag> commitCB = t => committed = t;

            Tag rolledback = null;
            Action<Tag> rollbackCB = t => rolledback = t;
            var editTag = new TagEditModel(tag, commitCB, rollbackCB);

            editTag.Name = "changed";

            // ACT

            editTag.RollbackCommand.Execute(null);

            // ASSERT

            Assert.Equal(tag.Model, rolledback);
            Assert.Null(committed);
            Assert.Equal("tag", tag.Name);
            Assert.Equal("tag", editTag.Name);
        }
    }
}