using Kosmograph.Model.Base;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Kosmograph.Model.Test
{
    public class FacetedEntityTest
    {
        public static IEnumerable<object[]> GetFactedInstances()
        {
            yield return new Tag().Yield().ToArray();
            yield return new Category().Yield().ToArray();
        }

        [Theory]
        [MemberData(nameof(GetFactedInstances))]
        public void FacetedEntity_has_no_facet_at_beginning(FacetedEntityBase faceted)
        {
            // ACT

            var result = faceted.Facet;

            // ASSERT

            Assert.Null(result);
        }

        [Theory]
        [MemberData(nameof(GetFactedInstances))]
        public void FacetedEntity_adds_facets(FacetedEntityBase faceted)
        {
            // ARRANGE

            var facet = new Facet();

            // ACT

            faceted.AssignFacet(facet);

            // ASSERT

            Assert.Equal(facet, faceted.Facet);
        }
    }
}