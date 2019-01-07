using Kosmograph.Model;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Kosmograph.Desktop.Editors.ViewModel
{
    public class AssignedTagViewModelTest
    {
        [Fact]
        public void AssignedTagViewModel_mirrors_Model()
        {
            // ARRANGE

            var model = new Tag("t", new Facet("f", new FacetProperty("p")));
            var tagViewModel = new TagViewModel(model);
            var values = new Dictionary<string, object>
            {
                { model.Facet.Properties.Single().Id.ToString(), 1 }
            };

            // ACT

            var result = new AssignedTagViewModel(tagViewModel, values);

            // ASSERT

            Assert.Equal(model, result.Tag.Model);
            Assert.Equal(1, result.Properties.Single().Value);
        }

        [Fact]
        public void AssignedTagViewModel_changes_Model()
        {
            // ARRANGE

            var model = new Tag("t", new Facet("f", new FacetProperty("p")));
            var tagViewModel = new TagViewModel(model);
            var values = new Dictionary<string, object>
            {
                { model.Facet.Properties.Single().Id.ToString(), 1 }
            };
            var viewModel = new AssignedTagViewModel(tagViewModel, values);

            // ACT

            viewModel.Properties.Single().Value = "changed";

            // ASSERT

            Assert.Equal(model, viewModel.Tag.Model);
            Assert.Equal("changed", values[model.Facet.Properties.Single().Id.ToString()]);
        }
    }
}