using Elementary.Compare;
using Kosmograph.Desktop.ViewModel;
using Kosmograph.Model;
using Xunit;

namespace Kosmograph.Desktop.Test.ViewModel
{
    public class TagEditModelTest
    {
        private readonly Tag tag;
        private readonly TagEditModel editTag;

        public TagEditModelTest()
        {
        }

        [Fact]
        public void TagEditModel_mirrors_TagViewModel()
        {
            // ARRANGE

            var tag = new TagViewModel(
                new Tag("tag", new Facet("facet", new FacetProperty("p"))));
            var editTag = new TagEditModel(tag, delegate { }, delegate { });

            // ASSERT

            var comp = this.tag.DeepCompare(this.editTag);

            Assert.Empty(comp.Different);
        }

        [Fact]
        public void TagEditModel_delays_changes_at_Tag()
        {
            // ACT

            this.editTag.Name = "changed";

            // ASSERT

            Assert.Equal("tag", this.tag.Name);
            Assert.Equal("changed", editTag.Name);
        }

        [Fact]
        public void TagEditModel_commits_changes_to_Tag()
        {
            // ARRANGE

            editTag.Name = "changed";

            // ACT

            editTag.Commit();

            // ASSERT

            Assert.Equal("changed", this.tag.Name);
            Assert.Equal("changed", editTag.Name);
        }

        [Fact]
        public void TagEditModel_reverts_changes_at_Tag()
        {
            // ARRANGE

            editTag.Name = "changed";

            // ACT

            editTag.Rollback();

            // ASSERT

            Assert.Equal("tag", this.tag.Name);
            Assert.Equal("tag", editTag.Name);
        }
    }
}