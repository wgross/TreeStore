using System.Linq;
using Xunit;

namespace Kosmograph.Model.Test
{
    public class FacetTest
    {
        [Fact]
        public void Facet_hasnt_properties_at_the_beginning()
        {
            // ACT

            var facet = new Facet();

            // ASSERT

            Assert.Empty(facet.Properties);
        }

        [Fact]
        public void Facet_adds_Property()
        {
            // ARRANGE

            var facet = new Facet();
            var property = new FacetProperty();

            // ACT

            facet.AddProperty(property);

            // ASSERT

            Assert.Equal(property, facet.Properties.Single());
        }

        [Fact]
        public void Facet_adding_Property_ignores_duplicate()
        {
            // ARRANGE

            var facet = new Facet();
            var property = new FacetProperty();

            facet.AddProperty(property);

            // ACT

            facet.AddProperty(property);

            // ASSERT

            Assert.Equal(property, facet.Properties.Single());
        }

        [Fact]
        public void Facet_removes_Property()
        {
            // ARRANGE

            var property1 = new FacetProperty();
            var property2 = new FacetProperty();
            var facet = new Facet("facet", property1, property2);

            // ACT

            facet.RemoveProperty(property2);

            // ASSERT

            Assert.Equal(property1, facet.Properties.Single());
        }
    }
}