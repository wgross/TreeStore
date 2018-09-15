using System.Linq;
using Xunit;

namespace Kosmograph.Model.Test
{
    public class RelationshipTest
    {
        [Fact]
        public void Relationship_adds_Tag()
        {
            // ARRANGE

            var tag = new Tag("tag", new Facet("facet", new FacetProperty()));

            var relationship = new Relationship();
            relationship.AddTag(tag);

            // ACT

            var result = relationship.Tags;

            // ASSERT

            Assert.Equal(tag, result.Single());
        }

        [Fact]
        public void Relationship_sets_value_of_FacetProperty()
        {
            // ARRANGE

            var facet = new Facet("facet", new FacetProperty("name"));
            var tag = new Tag("tag", facet);
            var relationship = new Relationship("r", tag);

            // ACT

            relationship.SetFacetProperty(relationship.Tags.Single().Facet.Properties.Single(), 1);

            // ASSERT

            Assert.Equal(1, relationship.TryGetFacetProperty(facet.Properties.Single()).Item2);
        }

        [Fact]
        public void Relationship_getting_value_of_FacetProperty_returns_false_on_missng_value()
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