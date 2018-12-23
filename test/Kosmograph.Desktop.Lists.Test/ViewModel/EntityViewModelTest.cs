using Kosmograph.Desktop.Lists.ViewModel;
using Kosmograph.Model;
using System.Linq;
using Xunit;

namespace Kosmograph.Desktop.Lists.Test.ViewModel
{
    public class EntityViewModelTest
    {
        public Tag DefaultTag() => new Tag("tag", new Facet("facet", new FacetProperty("p")));

        public Entity DefaultEntity() => new Entity("entity", DefaultTag());

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
            Assert.Equal("tag1", result.Tags.ElementAt(0).Tag.Name);
            Assert.Equal("p1", result.Tags.ElementAt(0).Properties.Single().Property.Name);
            Assert.Equal(1, result.Tags.ElementAt(0).Properties.Single().Value);
        }

        [Fact]
        public void EnttyViewModel_are_equal_if_Entities_are_equal()
        {
            // ARRANGE

            var entity = DefaultEntity();

            // ASSART

            Assert.Equal(new EntityViewModel(entity), new EntityViewModel(entity));
            Assert.Equal(new EntityViewModel(entity).GetHashCode(), new EntityViewModel(entity).GetHashCode());
        }

        //[Fact]
        //public void EntityViewModel_removes_Tag_from_Model()
        //{
        //    // ARRANGE

        //    var tag1 = new Tag("tag1", new Facet("f", new FacetProperty("p1")));
        //    var model = new Entity("entity", tag1);
        //    var viewModel = new EntityViewModel(model, model.Tags.Single().ToViewModel());

        //    // ACT

        //    viewModel.Tags.Remove(viewModel.Tags.Single());

        //    // ASSERT

        //    Assert.Empty(viewModel.Tags);
        //    Assert.Empty(model.Tags);
        //}
    }
}