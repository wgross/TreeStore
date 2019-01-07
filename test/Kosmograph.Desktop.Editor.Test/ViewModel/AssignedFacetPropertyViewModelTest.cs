using Kosmograph.Desktop.Editors.ViewModel;
using Kosmograph.Model;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Kosmograph.Desktop.Editor.Test.ViewModel
{
    public class AssignedFacetPropertyViewModelTest
    {
        [Fact]
        public void AssignedFacetPropertyViewModel_mirrors_Model()
        {
            var model = new FacetProperty("p");
            var values = new Dictionary<string, object> { { model.Id.ToString(), 1 } };

            // ACT

            var result = new AssignedFacetPropertyViewModel(model.ToViewModel(), new Dictionary<string, object> { { model.Id.ToString(), 1 } });

            // ASSERT

            Assert.Equal("p", result.Property.Name);
            Assert.Equal(1, result.Value);
        }

        [Fact]
        public void AssignedFacetPropertyViewModel_changes_Model()
        {
            var model = new FacetProperty("p");
            var values = new Dictionary<string, object> { { model.Id.ToString(), 1 } };
            var viewModel = new AssignedFacetPropertyViewModel(model.ToViewModel(), values);

            // ACT
            // changing of name is not used.

            viewModel.Value = "value";

            // ASSERT

            Assert.Equal("value", values.Single().Value);
        }
    }
}