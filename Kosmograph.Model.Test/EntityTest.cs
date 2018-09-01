using System.Linq;
using Xunit;

namespace Kosmograph.Model.Test
{
    public class EntityTest
    {
        [Fact]
        public void Entity_has_Category()
        {
            // ARRANGE

            var category = new Category();
            var entity = new Entity();

            // ACT

            entity.SetCategory(category);

            // ASSERT

            Assert.Same(category, entity.Category);
        }

        [Fact]
        public void Entity_has_Facet_from_Category()
        {
            // ARRANGE

            var facet = new Facet();
            var category = new Category();
            category.AssignFacet(facet);

            var entity = new Entity();

            // ACT

            entity.SetCategory(category);

            // ASSERT

            Assert.Equal(facet, entity.Facets.Single());
        }
    }
}