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
        public void FacetProperty_created_with_name_and_type()
        {
            // ACT

            var result = new FacetProperty("facet", FacetPropertyTypeValues.DateTime);

            // ASSERT

            Assert.Equal("facet", result.Name);
            Assert.Equal(FacetPropertyTypeValues.DateTime, result.Type);
        }

        [Theory]
        [InlineData(FacetPropertyTypeValues.Bool)]
        [InlineData(FacetPropertyTypeValues.DateTime)]
        [InlineData(FacetPropertyTypeValues.Decimal)]
        [InlineData(FacetPropertyTypeValues.Double)]
        [InlineData(FacetPropertyTypeValues.Guid)]
        [InlineData(FacetPropertyTypeValues.Long)]
        [InlineData(FacetPropertyTypeValues.String)]
        public void FacetProperty_accepts_null_value(FacetPropertyTypeValues facetPropertyType)
        {
            // ARRANGE

            var entity = DefaultEntity(e => e.Tags.Single().Facet.Properties.Single().Type = facetPropertyType);

            // ACT

            var result = entity.Tags.Single().Facet.Properties.Single().CanAssignValue(null);

            // ASSERT

            Assert.True(result);
        }

        [Fact]
        public void FacetProperty_long_rejects_incompatible_value()
        {
            // ARRANGE

            var entity = DefaultEntity(e => e.Tags.Single().Facet.Properties.Single().Type = FacetPropertyTypeValues.Long);

            // ACT

            var result = entity.Tags.Single().Facet.Properties.Single().CanAssignValue("text");

            // ASSERT

            Assert.False(result);
        }

        [Fact]
        public void FacetProperty_long_accepts_compatible_value()
        {
            // ARRANGE

            var entity = DefaultEntity(e => e.Tags.Single().Facet.Properties.Single().Type = FacetPropertyTypeValues.Long);

            // ACT

            var result = entity.Tags.Single().Facet.Properties.Single().CanAssignValue("1");

            // ASSERT

            Assert.True(result);
        }

        [Fact]
        public void FacetProperty_bool_rejects_incompatible_value()
        {
            // ARRANGE

            var entity = DefaultEntity(e => e.Tags.Single().Facet.Properties.Single().Type = FacetPropertyTypeValues.Bool);

            // ACT

            var result = entity.Tags.Single().Facet.Properties.Single().CanAssignValue("text");

            // ASSERT

            Assert.False(result);
        }

        [Fact]
        public void FacetProperty_bool_accepts_compatible_value()
        {
            // ARRANGE

            var entity = DefaultEntity(e => e.Tags.Single().Facet.Properties.Single().Type = FacetPropertyTypeValues.Bool);

            // ACT

            var result = entity.Tags.Single().Facet.Properties.Single().CanAssignValue(true.ToString());

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