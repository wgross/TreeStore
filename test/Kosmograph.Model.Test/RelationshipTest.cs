using System.Linq;
using Xunit;

namespace Kosmograph.Model.Test
{
    public class RelationshipTest
    {
        [Fact]
        public void Relationship_references_two_entites()
        {
            // ARRANGE

            var entity1 = new Entity();
            var entity2 = new Entity();

            // ACT

            var relationship = new Relationship(string.Empty, from: entity1, to: entity2);

            // ASSERT

            Assert.Equal(entity1, relationship.From);
            Assert.Equal(entity2, relationship.To);
        }

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

            var entity1 = new Entity();
            var entity2 = new Entity();
            var facet = new Facet("facet", new FacetProperty("name"));
            var tag = new Tag("tag", facet);
            var relationship = new Relationship("r", entity1, entity2, tag);

            // ACT

            relationship.SetFacetProperty(relationship.Tags.Single().Facet.Properties.Single(), 1);

            // ASSERT

            Assert.Equal(1, relationship.TryGetFacetProperty(facet.Properties.Single()).Item2);
        }

        [Fact]
        public void Relationship_getting_value_of_FacetProperty_returns_false_on_missng_value()
        {
            // ARRANGE

            var entity1 = new Entity();
            var entity2 = new Entity();
            var facet = new Facet("facet", new FacetProperty("prop"));
            var tag = new Tag("tag", facet);
            var relationship = new Relationship(string.Empty, entity1, entity2, tag);

            // ACT

            var (result, _) = relationship.TryGetFacetProperty(facet.Properties.Single());

            // ASSERT

            Assert.False(result);
        }
    }
}