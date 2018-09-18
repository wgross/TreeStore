using Kosmograph.Desktop.ViewModel;
using Kosmograph.Model;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Kosmograph.Desktop.Test.ViewModel
{
    public class AssignedTagViewModelTest
    {
        [Fact]
        public void AssignedTagViewModel_mirrors_Model()
        {
            // ARRANGE

            var model = new Tag("t", new Facet("f", new FacetProperty("p")));
            var values = new Dictionary<string, object>
            {
                { model.Facet.Properties.Single().Id.ToString(), 1 }
            };

            // ACT

            var viewModel = new AssignedTagViewModel(model, values);

            // ASSERT

            Assert.Equal("t", viewModel.Name);
            Assert.Equal(1, viewModel.Properties.Single().Value);
        }
    }
}