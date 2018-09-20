using Kosmograph.Desktop.ViewModel;
using Kosmograph.Model;
using Xunit;

namespace Kosmograph.Desktop.Test.ViewModel
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

        [Fact]
        public void FacetPropertyViewModel_changes_Model()
        {
            // ARRANGE

            var model = new FacetProperty("p1");
            var viewModel = new FacetPropertyViewModel(model);

            // ACT

            viewModel.Name = "changed";

            // ASSERT

            Assert.Equal("changed", model.Name);
        }
    }
}