using Kosmograph.Model.Base;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Kosmograph.Model.Test.Base
{
    public class FacetingEntityTest
    {
        public static IEnumerable<object[]> GetFactingInstances()
        {
            yield return new Tag().Yield().ToArray();
            yield return new Category().Yield().ToArray();
        }

        [Theory]
        [MemberData(nameof(GetFactingInstances))]
        public void FacetingEntity_has_empty_facet_at_beginning(FacetingEntityBase faceted)
        {
            // ACT

            var result = faceted.Facet;

            // ASSERT

            Assert.Equal(Facet.Empty, result);
        }

        [Theory]
        [MemberData(nameof(GetFactingInstances))]
        public void FacetingEntity_adds_facets(FacetingEntityBase faceted)
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