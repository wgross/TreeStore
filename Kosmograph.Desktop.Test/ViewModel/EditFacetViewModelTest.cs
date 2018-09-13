using Kosmograph.Desktop.ViewModel;
using Kosmograph.Model;
using System.Linq;
using Xunit;

namespace Kosmograph.Desktop.Test.ViewModel
{
    public class EditFacetViewModelTest
    {
        private readonly Facet facet;
        private readonly Tag tag;
        private readonly EditTagViewModel editTag;

        public EditFacetViewModelTest()
        {
            this.facet = new Facet("f", new FacetProperty("p1"));
            this.tag = new Tag("tag", facet);
            this.editTag = new EditTagViewModel(tag, delegate { });
        }

        [Fact]
        public void EditFacetViewModelTest_mirrors_Facet()
        {
            // ASSERT

            Assert.Single(editTag.Facet.Properties);
            Assert.Equal("f", editTag.Facet.Name);
        }

        [Fact]
        public void EditFacetViewModelTest_delays_changes_to_Facet()
        {
            // ACT

            this.editTag.Facet.CreatePropertyCommand.Execute("p2");
            this.editTag.Facet.RemovePropertyCommand.Execute(editTag.Facet.Properties.First());

            // ASSERT

            Assert.Single(this.facet.Properties);
            Assert.Equal("p1", this.facet.Properties.Single().Name);
            Assert.Single(this.editTag.Facet.Properties);
            Assert.Equal("p2", this.editTag.Facet.Properties.Single().Name);
        }

        [Fact]
        public void EditFacetViewModel_commits_changes_to_Facet()
        {
            // ARRANGE

            this.editTag.Facet.CreatePropertyCommand.Execute("p2");
            this.editTag.Facet.RemovePropertyCommand.Execute(editTag.Facet.Properties.First());

            // ACT

            this.editTag.Commit();

            // ASSERT

            Assert.Single(this.facet.Properties);
            Assert.Equal("p2", this.facet.Properties.Single().Name);
            Assert.Single(this.editTag.Facet.Properties);
            Assert.Equal("p2", this.facet.Properties.Single().Name);
        }

        [Fact]
        public void EditFacetViewModel_reverts_changes_to_Facet()
        {
            // ARRANGE

            this.editTag.Facet.CreatePropertyCommand.Execute("p2");
            this.editTag.Facet.RemovePropertyCommand.Execute(editTag.Facet.Properties.First());

            // ACT

            this.editTag.Rollback();

            // ASSERT

            Assert.Single(this.facet.Properties);
            Assert.Equal("p1", this.facet.Properties.Single().Name);
            Assert.Single(this.editTag.Facet.Properties);
            Assert.Equal("p1", this.editTag.Facet.Properties.Single().Name);
        }
    }
}