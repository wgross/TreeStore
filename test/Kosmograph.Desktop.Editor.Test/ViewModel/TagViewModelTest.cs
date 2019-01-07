using Kosmograph.Model;
using System.Linq;
using Xunit;

namespace Kosmograph.Desktop.Editors.ViewModel
{
    public class TagViewModelTest
    {
        [Fact]
        public void TagViewModel_mirrors_Tag()
        {
            // ARRANGE

            var tag = new Tag("tag", new Facet("facet", new FacetProperty("p")));

            // ACT

            var result = new TagViewModel(tag);

            // ASSERT

            Assert.Equal("tag", result.Name);
            Assert.Single(result.Properties);
        }

        [Fact]
        public void TagViewModel_changes_Tag()
        {
            // ARRANGE

            var tag = new Tag("tag", new Facet("facet", new FacetProperty("p")));
            var viewTag = new TagViewModel(tag);

            // ACT

            viewTag.Name = "changed";
            viewTag.Properties.Remove(viewTag.Properties.Single());
            viewTag.Properties.Add(new FacetPropertyViewModel(new FacetProperty("p2")));

            // ASSERT

            Assert.Equal("changed", tag.Name);
            Assert.Single(tag.Facet.Properties);
            Assert.Equal("p2", tag.Facet.Properties.Single().Name);
        }

        [Fact]
        public void TagViewModel_sets_Facet_name_equal_to_Tag_name()
        {
            // ARRANGE

            var tag = new Tag("tag", new Facet("facet", new FacetProperty("p")));
            var viewTag = new TagViewModel(tag);

            // ACT

            viewTag.Name = "changed";

            // ASSERT

            Assert.Equal("changed", tag.Name);
            Assert.Equal("changed", tag.Facet.Name);
        }
    }
}