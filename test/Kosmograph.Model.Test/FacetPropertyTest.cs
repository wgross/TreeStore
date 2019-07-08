using System.Linq;
using Xunit;

namespace Kosmograph.Model.Test
{
    public class FacetPropertyTest : ModelTestBase
    {
        public FacetPropertyTest()
        {
        }

        [Fact]
        public void FacetProperty_rejects_incompatible_value()
        {
            // ARRANGE

            var entity = DefaultEntity(e => e.Tags.Single().Facet.Properties.Single().Type = FacetPropertyTypeValues.Integer);

            // ACT

            var result = entity.Tags.Single().Facet.Properties.Single().CanAssignValue("text");

            // ASSERT

            Assert.False(result);
        }
    }
}