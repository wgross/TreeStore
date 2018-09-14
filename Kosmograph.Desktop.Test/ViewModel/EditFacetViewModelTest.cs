using Kosmograph.Desktop.ViewModel;
using Kosmograph.Model;
using System;
using System.Linq;
using Xunit;

namespace Kosmograph.Desktop.Test.ViewModel
{
    public class EditFacetViewModelTest
    {
        private readonly Facet facet;
        private readonly Tag tag;

        public EditFacetViewModelTest()
        {
            this.facet = new Facet("f", new FacetProperty("p1"));
            this.tag = new Tag("tag", facet);
        }

        [Fact]
        public void EditFacetViewModelTest_mirrors_Facet()
        {
            // ARRANGE

            var editTag = new EditTagViewModel(this.tag);

            // ASSERT

            Assert.Single(editTag.Facet.Properties);
            Assert.Equal("f", editTag.Facet.Name);
        }

        [Fact]
        public void EditFacetViewModelTest_delays_changes_to_Facet()
        {
            // ARRANGE

            bool callbackWasUsed = false;
            Action<Tag> callback = t => callbackWasUsed = true;

            var editTag = new EditTagViewModel(this.tag, callback, callback);

            // ACT

            editTag.Facet.CreatePropertyCommand.Execute("p2");
            editTag.Facet.RemovePropertyCommand.Execute(editTag.Facet.Properties.First());

            // ASSERT

            Assert.False(callbackWasUsed);
            Assert.Single(this.facet.Properties);
            Assert.Equal("p1", this.facet.Properties.Single().Name);
            Assert.Single(editTag.Facet.Properties);
            Assert.Equal("p2", editTag.Facet.Properties.Single().Name);
        }

        [Fact]
        public void EditFacetViewModel_commits_changes_to_Facet()
        {
            // ARRANGE

            bool commitCallbackWasUsed = false;
            Action<Tag> commit = t => commitCallbackWasUsed = true;

            bool rollbackCallbackWasUsed = false;
            Action<Tag> rollback = t => commitCallbackWasUsed = true;

            var editTag = new EditTagViewModel(this.tag, commit, rollback);

            editTag.Facet.CreatePropertyCommand.Execute("p2");
            editTag.Facet.RemovePropertyCommand.Execute(editTag.Facet.Properties.First());

            // ACT

            editTag.Commit();

            // ASSERT

            Assert.True(commitCallbackWasUsed);
            Assert.False(rollbackCallbackWasUsed);
            Assert.Single(this.facet.Properties);
            Assert.Equal("p2", this.facet.Properties.Single().Name);
            Assert.Single(editTag.Facet.Properties);
            Assert.Equal("p2", this.facet.Properties.Single().Name);
        }

        [Fact]
        public void EditFacetViewModel_reverts_changes_to_Facet()
        {
            // ARRANGE

            bool commitCallbackWasUsed = false;
            Action<Tag> commit = t => commitCallbackWasUsed = true;

            bool rollbackCallbackWasUsed = false;
            Action<Tag> rollback = t => rollbackCallbackWasUsed = true;

            var editTag = new EditTagViewModel(this.tag, commit, rollback);

            editTag.Facet.CreatePropertyCommand.Execute("p2");
            editTag.Facet.RemovePropertyCommand.Execute(editTag.Facet.Properties.First());

            // ACT

            editTag.Rollback();

            // ASSERT

            Assert.False(commitCallbackWasUsed);
            Assert.True(rollbackCallbackWasUsed);
            Assert.Single(this.facet.Properties);
            Assert.Equal("p1", this.facet.Properties.Single().Name);
            Assert.Single(editTag.Facet.Properties);
            Assert.Equal("p1", editTag.Facet.Properties.Single().Name);
        }
    }
}
;