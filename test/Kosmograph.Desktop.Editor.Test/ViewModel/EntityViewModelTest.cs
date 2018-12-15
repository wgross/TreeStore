using Kosmograph.Desktop.ViewModel;
using Kosmograph.Model;
using System.Linq;
using Xunit;

namespace Kosmograph.Desktop.Editor.Test.ViewModel
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

            var result = new EntityViewModel(model, model.Tags.Single().ToViewModel());

            // ASSERT

            Assert.Equal("entity", result.Name);
            Assert.Equal("tag1", result.Tags.ElementAt(0).Tag.Name);
            Assert.Equal("p1", result.Tags.ElementAt(0).Properties.Single().Property.Name);
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

            var viewModel = new EntityViewModel(model, model.Tags.Single().ToViewModel());

            // ACT

            viewModel.Name = "changed";
            viewModel.Tags.Single().Properties.Single().Value = "changed";

            // ASSERT

            Assert.Equal("changed", viewModel.Name);
            Assert.Equal("changed", model.Name);
            Assert.Equal("changed", model.Values[tag1.Facet.Properties.Single().Id.ToString()]);
        }

        [Fact]
        public void EntityViewModel_adds_Tag_to_Model()
        {
            // ARRANGE

            var tag1 = new Tag("tag2", new Facet("f", new FacetProperty("p2")));
            var model = new Entity("entity");
            var viewModel = new EntityViewModel(model);

            // ACT

            viewModel.Tags.Add(new AssignedTagViewModel(tag1.ToViewModel(), viewModel.Model.Values));

            // ASSERT

            Assert.Equal(tag1, viewModel.Tags.ElementAt(0).Tag.Model);
            Assert.Equal(tag1, model.Tags.ElementAt(0));
        }

        [Fact]
        public void EntityViewModel_removes_Tag_from_Model()
        {
            // ARRANGE

            var tag1 = new Tag("tag1", new Facet("f", new FacetProperty("p1")));
            var model = new Entity("entity", tag1);
            var viewModel = new EntityViewModel(model, model.Tags.Single().ToViewModel());

            // ACT

            viewModel.Tags.Remove(viewModel.Tags.Single());

            // ASSERT

            Assert.Empty(viewModel.Tags);
            Assert.Empty(model.Tags);
        }
    }
}