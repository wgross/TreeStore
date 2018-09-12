using Elementary.Compare;
using Kosmograph.Desktop.ViewModel;
using Kosmograph.Model;
using Xunit;

namespace Kosmograph.Desktop.Test.ViewModel
{
    public class EditTagViewModelTest
    {
        private readonly Tag tag;
        private readonly EditTagViewModel editTag;

        public EditTagViewModelTest()
        {
            this.tag = new Tag("tag", new Facet("facet", new FacetProperty("p")));
            this.editTag = new EditTagViewModel(this.tag, delegate { });
        }

        [Fact]
        public void EditTagViewModel_mirrors_model_Tag()
        {
            // ASSERT

            var comp = this.tag.DeepCompare(this.editTag);

            Assert.Empty(comp.Different);
        }

        [Fact]
        public void EditTagViewModel_delays_changes_at_Tag()
        {
            // ACT

            this.editTag.Name = "changed";

            // ASSERT

            Assert.Equal("tag", this.tag.Name);
        }

        [Fact]
        public void EditTagViewModel_commits_changes_to_Tag()
        {
            // ARRANGE

            editTag.Name = "changed";

            // ACT

            editTag.Commit();

            // ASSERT

            Assert.Equal("changed", this.tag.Name);
        }
    }
}