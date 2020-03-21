using System;
using System.Linq;
using Xunit;

namespace TreeStore.Model.Test
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

            // ACT & ASSERT

            // as string
            Assert.False(entity.Tags.Single().Facet.Properties.Single().CanAssignValue("text"));
            Assert.False(entity.Tags.Single().Facet.Properties.Single().CanAssignValue(Guid.NewGuid().ToString()));
            Assert.False(entity.Tags.Single().Facet.Properties.Single().CanAssignValue(true.ToString()));
            Assert.False(entity.Tags.Single().Facet.Properties.Single().CanAssignValue(DateTime.Now.ToString()));

            // as object
            Assert.False(entity.Tags.Single().Facet.Properties.Single().CanAssignValue((object)"text"));
            Assert.False(entity.Tags.Single().Facet.Properties.Single().CanAssignValue((object)(decimal)1));
            Assert.False(entity.Tags.Single().Facet.Properties.Single().CanAssignValue((object)(double)1));
            Assert.False(entity.Tags.Single().Facet.Properties.Single().CanAssignValue((object)Guid.NewGuid()));
            Assert.False(entity.Tags.Single().Facet.Properties.Single().CanAssignValue((object)true));
            Assert.False(entity.Tags.Single().Facet.Properties.Single().CanAssignValue((object)DateTime.Now));
        }

        [Fact]
        public void FacetProperty_long_accepts_compatible_value()
        {
            // ARRANGE

            var entity = DefaultEntity(e => e.Tags.Single().Facet.Properties.Single().Type = FacetPropertyTypeValues.Long);

            // ACT

            // as string
            Assert.True(entity.Tags.Single().Facet.Properties.Single().CanAssignValue("1"));

            // as object
            Assert.True(entity.Tags.Single().Facet.Properties.Single().CanAssignValue((object)(long)1));
            Assert.True(entity.Tags.Single().Facet.Properties.Single().CanAssignValue((object)(int)1));
            Assert.True(entity.Tags.Single().Facet.Properties.Single().CanAssignValue((object)(short)1));
        }

        [Fact]
        public void FacetProperty_bool_rejects_incompatible_value()
        {
            // ARRANGE

            var entity = DefaultEntity(e => e.Tags.Single().Facet.Properties.Single().Type = FacetPropertyTypeValues.Bool);

            // ACT & ASSERT

            // as string
            Assert.False(entity.Tags.Single().Facet.Properties.Single().CanAssignValue(((decimal)1).ToString()));
            Assert.False(entity.Tags.Single().Facet.Properties.Single().CanAssignValue(((long)1).ToString()));
            Assert.False(entity.Tags.Single().Facet.Properties.Single().CanAssignValue(((double)1).ToString()));
            Assert.False(entity.Tags.Single().Facet.Properties.Single().CanAssignValue("text"));
            Assert.False(entity.Tags.Single().Facet.Properties.Single().CanAssignValue(Guid.NewGuid().ToString()));
            Assert.True(entity.Tags.Single().Facet.Properties.Single().CanAssignValue(true.ToString()));
            Assert.False(entity.Tags.Single().Facet.Properties.Single().CanAssignValue(DateTime.Now.ToString()));

            // as object
            Assert.False(entity.Tags.Single().Facet.Properties.Single().CanAssignValue("text"));
            Assert.False(entity.Tags.Single().Facet.Properties.Single().CanAssignValue((object)(decimal)1));
            Assert.False(entity.Tags.Single().Facet.Properties.Single().CanAssignValue((object)(double)1));
            Assert.False(entity.Tags.Single().Facet.Properties.Single().CanAssignValue((object)Guid.NewGuid()));
            Assert.True(entity.Tags.Single().Facet.Properties.Single().CanAssignValue((object)true));
            Assert.False(entity.Tags.Single().Facet.Properties.Single().CanAssignValue((object)DateTime.Now));
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

            // ACT & ASSERT

            // as string
            Assert.False(entity.Tags.Single().Facet.Properties.Single().CanAssignValue("text"));
            Assert.False(entity.Tags.Single().Facet.Properties.Single().CanAssignValue(Guid.NewGuid().ToString()));
            Assert.False(entity.Tags.Single().Facet.Properties.Single().CanAssignValue(true.ToString()));
            Assert.False(entity.Tags.Single().Facet.Properties.Single().CanAssignValue(DateTime.Now.ToString()));

            // as object
            Assert.False(entity.Tags.Single().Facet.Properties.Single().CanAssignValue("text"));
            Assert.False(entity.Tags.Single().Facet.Properties.Single().CanAssignValue((object)(decimal)1));
            Assert.False(entity.Tags.Single().Facet.Properties.Single().CanAssignValue((object)Guid.NewGuid()));
            Assert.False(entity.Tags.Single().Facet.Properties.Single().CanAssignValue((object)true));
            Assert.False(entity.Tags.Single().Facet.Properties.Single().CanAssignValue((object)DateTime.Now));
        }

        [Fact]
        public void FacetProperty_double_accepts_compatible_value()
        {
            // ARRANGE

            var entity = DefaultEntity(e => e.Tags.Single().Facet.Properties.Single().Type = FacetPropertyTypeValues.Double);

            // ACT & ASSERT

            // as string
            Assert.True(entity.Tags.Single().Facet.Properties.Single().CanAssignValue("1.1"));
            Assert.True(entity.Tags.Single().Facet.Properties.Single().CanAssignValue("1"));

            // as object
            Assert.True(entity.Tags.Single().Facet.Properties.Single().CanAssignValue((object)(double)1.1));
            Assert.True(entity.Tags.Single().Facet.Properties.Single().CanAssignValue((object)(float)1.1));
        }

        [Fact]
        public void FacetProperty_decimal_rejects_incompatible_value()
        {
            // ARRANGE

            var entity = DefaultEntity(e => e.Tags.Single().Facet.Properties.Single().Type = FacetPropertyTypeValues.Decimal);

            // ACT & ASSERT

            // as string
            Assert.False(entity.Tags.Single().Facet.Properties.Single().CanAssignValue("text"));
            Assert.False(entity.Tags.Single().Facet.Properties.Single().CanAssignValue(Guid.NewGuid().ToString()));
            Assert.False(entity.Tags.Single().Facet.Properties.Single().CanAssignValue(true.ToString()));
            Assert.False(entity.Tags.Single().Facet.Properties.Single().CanAssignValue(DateTime.Now.ToString()));

            // as object
            Assert.False(entity.Tags.Single().Facet.Properties.Single().CanAssignValue("text"));
            Assert.False(entity.Tags.Single().Facet.Properties.Single().CanAssignValue((object)Guid.NewGuid()));
            Assert.False(entity.Tags.Single().Facet.Properties.Single().CanAssignValue((object)true));
            Assert.False(entity.Tags.Single().Facet.Properties.Single().CanAssignValue((object)DateTime.Now));
        }

        [Fact]
        public void FacetProperty_decimal_accepts_compatible_value()
        {
            // ARRANGE

            var entity = DefaultEntity(e => e.Tags.Single().Facet.Properties.Single().Type = FacetPropertyTypeValues.Decimal);

            // ACT & ASSERT

            // as string
            Assert.True(entity.Tags.Single().Facet.Properties.Single().CanAssignValue("1.1"));

            // as object
            Assert.True(entity.Tags.Single().Facet.Properties.Single().CanAssignValue((object)(decimal)1));
            Assert.True(entity.Tags.Single().Facet.Properties.Single().CanAssignValue((object)(short)1));
            Assert.True(entity.Tags.Single().Facet.Properties.Single().CanAssignValue((object)(int)1));
            Assert.True(entity.Tags.Single().Facet.Properties.Single().CanAssignValue((object)(long)1));
            Assert.True(entity.Tags.Single().Facet.Properties.Single().CanAssignValue((object)(float)1));
            Assert.True(entity.Tags.Single().Facet.Properties.Single().CanAssignValue((object)(double)1));
        }

        [Fact]
        public void FacetProperty_Guid_rejects_incompatible_value()
        {
            // ARRANGE

            var entity = DefaultEntity(e => e.Tags.Single().Facet.Properties.Single().Type = FacetPropertyTypeValues.Guid);

            // ACT & ASSERT

            // as string
            Assert.False(entity.Tags.Single().Facet.Properties.Single().CanAssignValue(((decimal)1).ToString()));
            Assert.False(entity.Tags.Single().Facet.Properties.Single().CanAssignValue(((long)1).ToString()));
            Assert.False(entity.Tags.Single().Facet.Properties.Single().CanAssignValue(((double)1).ToString()));
            Assert.False(entity.Tags.Single().Facet.Properties.Single().CanAssignValue("text"));
            Assert.True(entity.Tags.Single().Facet.Properties.Single().CanAssignValue(Guid.NewGuid().ToString()));
            Assert.False(entity.Tags.Single().Facet.Properties.Single().CanAssignValue(true.ToString()));
            Assert.False(entity.Tags.Single().Facet.Properties.Single().CanAssignValue(DateTime.Now.ToString()));

            // as object
            Assert.False(entity.Tags.Single().Facet.Properties.Single().CanAssignValue("text"));
            Assert.False(entity.Tags.Single().Facet.Properties.Single().CanAssignValue((object)(decimal)1));
            Assert.False(entity.Tags.Single().Facet.Properties.Single().CanAssignValue((object)(double)1));
            Assert.True(entity.Tags.Single().Facet.Properties.Single().CanAssignValue((object)Guid.NewGuid()));
            Assert.False(entity.Tags.Single().Facet.Properties.Single().CanAssignValue((object)true));
            Assert.False(entity.Tags.Single().Facet.Properties.Single().CanAssignValue((object)DateTime.Now));
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

            // ACT & ASSERT

            // as string
            Assert.False(entity.Tags.Single().Facet.Properties.Single().CanAssignValue(((decimal)1).ToString()));
            Assert.False(entity.Tags.Single().Facet.Properties.Single().CanAssignValue(((long)1).ToString()));
            Assert.False(entity.Tags.Single().Facet.Properties.Single().CanAssignValue(((double)1).ToString()));
            Assert.False(entity.Tags.Single().Facet.Properties.Single().CanAssignValue("text"));
            Assert.False(entity.Tags.Single().Facet.Properties.Single().CanAssignValue(Guid.NewGuid().ToString()));
            Assert.False(entity.Tags.Single().Facet.Properties.Single().CanAssignValue(true.ToString()));
            Assert.True(entity.Tags.Single().Facet.Properties.Single().CanAssignValue(DateTime.Now.ToString()));

            // as object
            Assert.False(entity.Tags.Single().Facet.Properties.Single().CanAssignValue("text"));
            Assert.False(entity.Tags.Single().Facet.Properties.Single().CanAssignValue((object)(decimal)1));
            Assert.False(entity.Tags.Single().Facet.Properties.Single().CanAssignValue((object)(double)1));
            Assert.False(entity.Tags.Single().Facet.Properties.Single().CanAssignValue((object)Guid.NewGuid()));
            Assert.False(entity.Tags.Single().Facet.Properties.Single().CanAssignValue((object)true));
            Assert.True(entity.Tags.Single().Facet.Properties.Single().CanAssignValue((object)DateTime.Now));
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