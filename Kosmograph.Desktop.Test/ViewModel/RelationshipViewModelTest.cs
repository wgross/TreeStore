using Kosmograph.Desktop.ViewModel;
using Kosmograph.Model;
using System.Linq;
using Xunit;

namespace Kosmograph.Desktop.Test.ViewModel
{
    public class RelationshipViewModelTest
    {
        [Fact]
        public void RelationshipViewModel_mirrors_Model()
        {
            // ARRANGE

            var model = new Relationship("r", new Entity(), new Entity(), new Tag());

            // ACT

            var result = new RelationshipViewModel(model, model.From.ToViewModel(), model.To.ToViewModel(), model.Tags.Single().ToViewModel());

            // ASSERT

            Assert.Equal("r", result.Name);
            Assert.Equal(model.From, result.From.Model);
            Assert.Equal(model.To, result.To.Model);
            Assert.Equal(model.Tags.Single(), result.Tags.Single().Tag.Model);
        }

        [Fact]
        public void RelationshipViewModel_changes_Model()
        {
            // ARRANGE

            var entity1 = new EntityViewModel(new Entity());
            var entity2 = new EntityViewModel(new Entity());
            var model = new Relationship("r", new Entity(), new Entity(), new Tag("t", new Facet("f", new FacetProperty("p"))));
            var viewModel = new RelationshipViewModel(model, model.From.ToViewModel(), model.To.ToViewModel(), model.Tags.Single().ToViewModel());

            // ACT

            viewModel.Name = "changed";
            viewModel.From = entity1;
            viewModel.To = entity2;
            viewModel.Tags.Single().Properties.Single().Value = "changed";

            // ASSERT

            Assert.Equal("changed", viewModel.Name);
            Assert.Equal("changed", model.Name);
            Assert.Equal("changed", model.Values[model.Tags.Single().Facet.Properties.Single().Id.ToString()]);

            Assert.Equal(entity1, viewModel.From);
            Assert.Equal(entity1.Model, model.From);
            Assert.Equal(entity2, viewModel.To);
            Assert.Equal(entity2.Model, model.To);
        }

        [Fact]
        public void RelationshipViewModel_assigns_tag_to_Model()
        {
            // ARRANGE

            var tag2 = new TagViewModel(new Tag("t2"));
            var entity1 = new EntityViewModel(new Entity());
            var entity2 = new EntityViewModel(new Entity());
            var model = new Relationship("r", new Entity(), new Entity(), new Tag());
            var viewModel = new RelationshipViewModel(model, model.From.ToViewModel(), model.To.ToViewModel(), model.Tags.Single().ToViewModel());

            // ACT

            viewModel.Tags.Add(new AssignedTagViewModel(tag2, viewModel.Model.Values));

            // ASSERT

            Assert.Equal(2, model.Tags.Count());
            Assert.Equal(tag2.Model, viewModel.Tags.ElementAt(1).Tag.Model);
        }

        [Fact]
        public void RelationshipViewModel_removes_tag_from_Model()
        {
            // ARRANGE

            var entity1 = new EntityViewModel(new Entity());
            var entity2 = new EntityViewModel(new Entity());
            var model = new Relationship("r", new Entity(), new Entity(), new Tag());
            var viewModel = new RelationshipViewModel(model, model.From.ToViewModel(), model.To.ToViewModel(), model.Tags.Single().ToViewModel());

            // ACT

            viewModel.Tags.Remove(viewModel.Tags.Single());

            // ASSERT

            Assert.Empty(viewModel.Tags);
            Assert.Empty(model.Tags);
        }
    }
}