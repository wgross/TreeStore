using Kosmograph.Desktop.ViewModel;
using Kosmograph.Model;
using System.Linq;
using Xunit;

namespace Kosmograph.Desktop.Test.ViewModel
{
    public class RelationshipViewModelTest
    {
        [Fact]
        public void RelationshipViewModel_mirrors_Relationship()
        {
            // ARRANGE

            var model = new Relationship("r", new Entity(), new Entity(), new Tag());

            // ACT

            var result = new RelationshipViewModel(model);

            // ASSERT

            Assert.Equal("r", result.Name);
            Assert.Equal(model.From, result.From.Model);
            Assert.Equal(model.To, result.To.Model);
            Assert.Equal(model.Tags.Single(), result.Tags.Single().Model);
        }

        [Fact]
        public void RelationshipViewModel_changes_Relationship()
        {
            // ARRANGE

            var tag2 = new TagViewModel(new Tag("t2", Facet.Empty));
            var entity1 = new EntityViewModel(new Entity());
            var entity2 = new EntityViewModel(new Entity());
            var model = new Relationship("r", new Entity(), new Entity(), new Tag());
            var viewModel = new RelationshipViewModel(model);

            // ACT

            viewModel.Name = "changed";
            viewModel.Tags.Remove(viewModel.Tags.Single());
            viewModel.Tags.Add(new AssignedTagViewModel(tag2.Model, viewModel.Model.Values));
            viewModel.From = entity1;
            viewModel.To = entity2;

            // ASSERT

            Assert.Equal("changed", viewModel.Name);
            Assert.Equal("changed", model.Name);

            Assert.Equal(entity1, viewModel.From);
            Assert.Equal(entity1.Model, model.From);
            Assert.Equal(entity2, viewModel.To);
            Assert.Equal(entity2.Model, model.To);

            Assert.Equal(tag2.Model, viewModel.Tags.Single().Model);
            Assert.Equal(tag2.Model, model.Tags.Single());
        }
    }
}