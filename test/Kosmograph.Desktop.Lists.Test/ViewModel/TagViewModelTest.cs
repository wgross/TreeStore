using Kosmograph.Desktop.Lists.ViewModel;
using Kosmograph.Model;
using System.Linq;
using Xunit;

namespace Kosmograph.Desktop.Lists.Test.ViewModel
{
    public class TagViewModelTest
    {
        public Tag DefaultTag() => new Tag("tag", new Facet("facet", new FacetProperty("p")));

        [Fact]
        public void TagViewModel_mirrors_Tag()
        {
            // ARRANGE

            var tag = DefaultTag();

            // ACT

            var result = new TagViewModel(tag);

            // ASSERT

            Assert.Equal("tag", result.Name);
            Assert.Equal(tag, result.Model);
            Assert.Single(result.Properties);
            Assert.Equal(tag.Facet.Properties.Single(), result.Properties.Single().Model);
        }

        [Fact]
        public void TagViewModel_are_equal_if_Tags_are_equal()
        {
            // ARRANGE

            var tag = DefaultTag();

            // ASSART

            Assert.Equal(new TagViewModel(tag), new TagViewModel(tag));
            Assert.Equal(new TagViewModel(tag).GetHashCode(), new TagViewModel(tag).GetHashCode());
        }
    }
}