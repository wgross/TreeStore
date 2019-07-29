using System;
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
        public void Facet_adds_property()
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
        public void Facet_removes_property()
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

        [Fact]
        public void Facet_rejects_duplicate_property_name()
        {
            // ARRANGE

            var facet = new Facet(string.Empty, new FacetProperty("name"));

            // ACT

            var result = Assert.Throws<InvalidOperationException>(() => facet.AddProperty(new FacetProperty("name")));

            // ASSERT

            Assert.Equal("duplicate property name: name", result.Message);
        }
    }
}