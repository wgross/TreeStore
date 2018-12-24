using Kosmograph.Desktop.Lists.ViewModel;
using Kosmograph.Model;
using Xunit;

namespace Kosmograph.Desktop.Lists.Test.ViewModel
{
    public class FacetPropertyViewModelTest
    {
        [Fact]
        public void FacetPropertyViewModel_mirrors_Model()
        {
            // ARRANGE

            var model = new FacetProperty("p1");

            // ACT

            var result = new FacetPropertyViewModel(model);

            // ASSERT

            Assert.Equal("p1", result.Name);
            Assert.Equal(model, result.Model);
        }
    }
}