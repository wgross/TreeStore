using Kosmograph.Desktop.ViewModel;
using Kosmograph.Model;
using System.Linq;
using Xunit;

namespace Kosmograph.Desktop.Test.ViewModel
{
    public class EntityViewModelTest
    {
        [Fact]
        public void EntitViewModel_mirrors_Entity()
        {
            // ARRANGE

            var tag1 = new Tag("tag1", new Facet("f", new FacetProperty("p1")));
            var tag2 = new Tag("tag2", new Facet("f", new FacetProperty("p2")));
            var entity = new Entity("entity", tag1, tag2);

            entity.SetFacetProperty(tag1.Facet.Properties.Single(), 1);

            // ACT

            var result = new EntityViewModel(entity);

            // ASSERT

            Assert.Equal("entity", result.Name);
            Assert.Equal("tag1", result.Tags.ElementAt(0).Name);
            Assert.Equal("tag2", result.Tags.ElementAt(1).Name);
            Assert.Equal("p1", result.Tags.ElementAt(0).Properties.Single().Name);
            Assert.Equal("p2", result.Tags.ElementAt(1).Properties.Single().Name);
            //Assert.Equal(1, result.Tags.ElementAt(0).Properties.Single().Value);
            //Assert.Null(result.Tags.ElementAt(1).Properties.Single().Value);
        }
    }
}