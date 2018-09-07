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
        public void Entity_adds_Tag()
        {
            // ARRANGE

            var tag = new Tag();
            var entity = new Entity();

            // ACT

            entity.AddTag(tag);

            // ASSERT

            Assert.Equal(tag, entity.Tags.Single());
        }

        [Fact]
        public void Entity_adding_Tag_ignores_duplicate()
        {
            // ARRANGE

            var tag = new Tag();
            var entity = new Entity();
            entity.AddTag(tag);

            // ACT

            entity.AddTag(tag);

            // ASSERT

            Assert.Equal(tag, entity.Tags.Single());
        }

        [Fact]
        public void Entity_has_Facet_from_Category()
        {
            // ARRANGE

            var facet = new Facet();
            var category = new Category("cat", facet);
            var entity = new Entity();
            entity.SetCategory(category);

            // ACT

            var result = entity.Facets().ToArray();

            // ASSERT

            Assert.Equal(facet, result.Single());
        }

        [Fact]
        public void Entity_has_Facet_from_Tag()
        {
            // ARRANGE

            var tag = new Tag("tag", new Facet("facet", new FacetProperty()));

            var entity = new Entity();
            entity.AddTag(tag);

            // ACT

            var result = entity.Facets().ToArray();

            // ASSERT

            Assert.Equal(tag.Facet, result.Single());
        }

        [Fact]
        public void Entity_has_Facet_from_Category_and_Tag()
        {
            // ARRANGE

            var tag = new Tag("tag", new Facet("facet", new FacetProperty()));
            var category = new Category("cat", new Facet("facet", new FacetProperty()));

            var entity = new Entity();
            entity.AddTag(tag);
            entity.SetCategory(category);

            // ACT

            var result = entity.Facets().ToArray();

            // ASSERT

            Assert.Equal(new[] { category.Facet, tag.Facet }, result);
        }

        [Fact]
        public void Entity_has_Facet_from_Category_and_Tag_ignores_duplicate()
        {
            // ARRANGE

            var facet = new Facet("facet", new FacetProperty());
            var tag = new Tag("tag", facet);
            var category = new Category("cat", facet);

            var entity = new Entity();
            entity.AddTag(tag);
            entity.SetCategory(category);

            // ACT

            var result = entity.Facets().ToArray();

            // ASSERT

            Assert.Equal(facet, result.Single());
        }

        [Fact]
        public void Entity_set_value_of_FacetProperty()
        {
            // ARRANGE

            var facet = new Facet("facet", new FacetProperty("name"));
            var tag = new Tag("tag", facet);
            var entity = new Entity();
            entity.AddTag(tag);

            // ACT

            entity.SetFacetProperty(entity.Facets().Single().Properties.Single(), 1);

            // ASSERT

            Assert.Equal(1, entity.TryGetFacetProperty(facet.Properties.Single()).Item2);
        }

        [Fact]
        public void Entity_getting_value_of_FacetProperty_returns_false_on_missng_value()
        {
            // ARRANGE

            var facet = new Facet("facet", new FacetProperty("prop"));
            var tag = new Tag("tag", facet);
            var entity = new Entity();
            entity.AddTag(tag);

            // ACT

            var (result, _) = entity.TryGetFacetProperty(facet.Properties.Single());

            // ASSERT

            Assert.False(result);
        }
    }
}