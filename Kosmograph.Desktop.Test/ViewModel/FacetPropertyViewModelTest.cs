using Kosmograph.Desktop.ViewModel;
using Kosmograph.Model;
using Xunit;

namespace Kosmograph.Desktop.Test.ViewModel
{
    public class FacetPropertyViewModelTest
    {
        [Fact]
        public void FacetPropertyViewModel_mirrors_FacetProperty()
        {
            // ARRANGE

            var property = new FacetProperty("p1");

            // ACT

            var result = new FacetPropertyViewModel(property);

            // ASSERT

            Assert.Equal("p1", result.Name);
            Assert.Equal(property, result.Model);
        }

        [Fact]
        public void FacetPropertyViewModel_changes_FacetProperty()
        {
            // ARRANGE

            var property = new FacetProperty("p1");
            var viewProperty = new FacetPropertyViewModel(property);

            // ACT

            viewProperty.Name = "changed";

            // ASSERT

            Assert.Equal("changed", property.Name);
        }
    }
}