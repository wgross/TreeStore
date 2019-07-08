using System;
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
        public void FacetProperty_int_rejects_incompatible_value()
        {
            // ARRANGE

            var entity = DefaultEntity(e => e.Tags.Single().Facet.Properties.Single().Type = FacetPropertyTypeValues.Integer);

            // ACT

            var result = entity.Tags.Single().Facet.Properties.Single().CanAssignValue("text");

            // ASSERT

            Assert.False(result);
        }

        [Fact]
        public void FacetProperty_int_accepts_compatible_value()
        {
            // ARRANGE

            var entity = DefaultEntity(e => e.Tags.Single().Facet.Properties.Single().Type = FacetPropertyTypeValues.Integer);

            // ACT

            var result = entity.Tags.Single().Facet.Properties.Single().CanAssignValue("1");

            // ASSERT

            Assert.True(result);
        }

        [Fact]
        public void FacetProperty_double_rejects_incompatible_value()
        {
            // ARRANGE

            var entity = DefaultEntity(e => e.Tags.Single().Facet.Properties.Single().Type = FacetPropertyTypeValues.Double);

            // ACT

            var result = entity.Tags.Single().Facet.Properties.Single().CanAssignValue("text");

            // ASSERT

            Assert.False(result);
        }

        [Fact]
        public void FacetProperty_double_accepts_compatible_value()
        {
            // ARRANGE

            var entity = DefaultEntity(e => e.Tags.Single().Facet.Properties.Single().Type = FacetPropertyTypeValues.Double);

            // ACT

            var result = entity.Tags.Single().Facet.Properties.Single().CanAssignValue("1.1");

            // ASSERT

            Assert.True(result);
        }

        [Fact]
        public void FacetProperty_decimal_rejects_incompatible_value()
        {
            // ARRANGE

            var entity = DefaultEntity(e => e.Tags.Single().Facet.Properties.Single().Type = FacetPropertyTypeValues.Decimal);

            // ACT

            var result = entity.Tags.Single().Facet.Properties.Single().CanAssignValue("text");

            // ASSERT

            Assert.False(result);
        }

        [Fact]
        public void FacetProperty_decimal_accepts_compatible_value()
        {
            // ARRANGE

            var entity = DefaultEntity(e => e.Tags.Single().Facet.Properties.Single().Type = FacetPropertyTypeValues.Decimal);

            // ACT

            var result = entity.Tags.Single().Facet.Properties.Single().CanAssignValue("1.1");

            // ASSERT

            Assert.True(result);
        }

        [Fact]
        public void FacetProperty_Guid_rejects_incompatible_value()
        {
            // ARRANGE

            var entity = DefaultEntity(e => e.Tags.Single().Facet.Properties.Single().Type = FacetPropertyTypeValues.Guid);

            // ACT

            var result = entity.Tags.Single().Facet.Properties.Single().CanAssignValue("text");

            // ASSERT

            Assert.False(result);
        }

        [Fact]
        public void FacetProperty_Guid_accepts_compatible_value()
        {
            // ARRANGE

            var entity = DefaultEntity(e => e.Tags.Single().Facet.Properties.Single().Type = FacetPropertyTypeValues.Guid);

            // ACT

            var result = entity.Tags.Single().Facet.Properties.Single().CanAssignValue(Guid.NewGuid().ToString());

            // ASSERT

            Assert.True(result);
        }

        [Fact]
        public void FacetProperty_DateTime_rejects_incompatible_value()
        {
            // ARRANGE

            var entity = DefaultEntity(e => e.Tags.Single().Facet.Properties.Single().Type = FacetPropertyTypeValues.DateTime);

            // ACT

            var result = entity.Tags.Single().Facet.Properties.Single().CanAssignValue("text");

            // ASSERT

            Assert.False(result);
        }

        [Fact]
        public void FacetProperty_DateTime_accepts_compatible_value()
        {
            // ARRANGE

            var entity = DefaultEntity(e => e.Tags.Single().Facet.Properties.Single().Type = FacetPropertyTypeValues.DateTime);

            // ACT

            var result = entity.Tags.Single().Facet.Properties.Single().CanAssignValue(DateTime.Now.ToString());

            // ASSERT

            Assert.True(result);
        }
    }
}