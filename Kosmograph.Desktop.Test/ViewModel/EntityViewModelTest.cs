using Kosmograph.Desktop.ViewModel;
using Kosmograph.Model;
using System.Linq;
using Xunit;

namespace Kosmograph.Desktop.Test.ViewModel
{
    public class EntityViewModelTest
    {
        [Fact]
        public void EntityViewModel_mirrors_Model()
        {
            // ARRANGE

            var tag1 = new Tag("tag1", new Facet("f", new FacetProperty("p1")));
            var model = new Entity("entity", tag1);

            model.SetFacetProperty(tag1.Facet.Properties.Single(), 1);

            // ACT

            var result = new EntityViewModel(model);

            // ASSERT

            Assert.Equal("entity", result.Name);
            Assert.Equal("tag1", result.Tags.ElementAt(0).Name);
            Assert.Equal("p1", result.Tags.ElementAt(0).Properties.Single().Name);
            Assert.Equal(1, result.Tags.ElementAt(0).Properties.Single().Value);
        }

        [Fact]
        public void EntityViewModel_changes_Model()
        {
            // ARRANGE

            var tag1 = new Tag("tag1", new Facet("f", new FacetProperty("p1")));
            var tag2 = new Tag("tag2", new Facet("f", new FacetProperty("p2")));
            var model = new Entity("entity", tag1);

            model.SetFacetProperty(tag1.Facet.Properties.Single(), 1);

            var viewModel = new EntityViewModel(model);

            // ACT

            viewModel.Name = "changed";
            viewModel.Tags.Remove(viewModel.Tags.Single());
            viewModel.Tags.Add(new AssignedTagViewModel(tag2, viewModel.Model.Values));

            // ASSERT

            Assert.Equal("changed", viewModel.Name);
            Assert.Equal("changed", model.Name);

            Assert.Equal("tag2", viewModel.Tags.ElementAt(0).Name);
            Assert.Equal("tag2", model.Tags.ElementAt(0).Name);
        }
    }
}