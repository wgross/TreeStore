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

            Assert.Equal("f", result.Name);
            Assert.Equal(property, result.Model);
        }
    }
}