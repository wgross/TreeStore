using System;
using System.Linq;
using System.Management.Automation;
using TreeStore.Model;
using TreeStore.PsModule.PathNodes;
using Xunit;

namespace TreeStore.PsModule.Test.PathNodes
{
    public class AssignedFacetPropertyNodeTest : NodeTestBase
    {
        #region P2F node structure

        [Fact]
        public void AssignedFacetPropertyNode_has_name_and_ItemMode()
        {
            // ARRANGE

            var e = DefaultEntity(WithAssignedDefaultTag);

            // ACT

            var result = new AssignedFacetPropertyNode(this.PersistenceMock.Object, e, e.Tags.Single().Facet.Properties.Single());

            // ASSERT

            Assert.Equal("p", result.Name);
        }

        [Fact]
        public void AssignedFacetPropertyNode_provides_Value()
        {
            // ARRANGE

            var e = DefaultEntity(WithAssignedDefaultTag);

            // ACT

            var result = new AssignedFacetPropertyNode(this.PersistenceMock.Object, e, e.Tags.Single().Facet.Properties.Single());

            // ASSERT

            Assert.Equal("p", result.Name);
            Assert.False(result.IsContainer);
        }

        #endregion P2F node structure

        #region IGetItem

        [Fact]
        public void AssignedFacetPropertyNode_provides_Item()
        {
            // ARRANGE

            var e = DefaultEntity(WithAssignedDefaultTag);
            e.SetFacetProperty(e.Tags.Single().Facet.Properties.Single(), "2");

            // ACT

            var result = new AssignedFacetPropertyNode(this.PersistenceMock.Object, e, e.Tags.Single().Facet.Properties.Single()).GetItem(this.ProviderContextMock.Object);

            // ASSERT

            Assert.Equal("p", result.Property<string>("Name"));
            Assert.Equal(TreeStoreItemType.AssignedFacetProperty, result.Property<TreeStoreItemType>("ItemType"));
            Assert.Equal("2", result.Property<string>("Value"));
            Assert.Equal(FacetPropertyTypeValues.String, result.Property<FacetPropertyTypeValues>("ValueType"));
            Assert.IsType<AssignedFacetPropertyNode.Item>(result.ImmediateBaseObject);

            var resultValue = (AssignedFacetPropertyNode.Item)result.ImmediateBaseObject;

            Assert.Equal(TreeStoreItemType.AssignedFacetProperty, resultValue.ItemType);
            Assert.Equal("p", resultValue.Name);
            Assert.Equal("2", resultValue.Value);
            Assert.Equal(e.Tags.Single().Facet.Properties.Single().Type, resultValue.ValueType);
        }

        #endregion IGetItem

        #region ISetItemProperty

        [Theory]
        [InlineData("value")]
        [InlineData("VALUE")]
        public void AssignedFacetPropertyNode_sets_value_property(string propertyName)
        {
            // ARRANGE

            var e = DefaultEntity(WithAssignedDefaultTag);
            e.SetFacetProperty(e.Tags.Single().Facet.Properties.Single(), "2");

            this.ProviderContextMock
                .Setup(c => c.Persistence)
                .Returns(this.PersistenceMock.Object);

            this.PersistenceMock
                .Setup(p => p.Entities)
                .Returns(this.EntityRepositoryMock.Object);

            this.EntityRepositoryMock
                .Setup(r => r.Upsert(e))
                .Returns(e);

            // ACT

            new AssignedFacetPropertyNode(this.PersistenceMock.Object, e, e.Tags.Single().Facet.Properties.Single())
                .SetItemProperties(this.ProviderContextMock.Object, new PSNoteProperty(propertyName, "3").Yield());

            // ASSERT
            // value has changed, entity was stored

            var (has, value) = e.TryGetFacetProperty(e.Tags.Single().Facet.Properties.Single());

            Assert.True(has);
            Assert.Equal("3", value);
        }

        [Fact]
        public void AssignedFacetPropertyNode_set_value_property_rejects_wrong_type()
        {
            // ARRANGE

            var e = DefaultEntity(WithAssignedDefaultTag, e => e.Tags.Single().Facet.Properties.Single().Type = FacetPropertyTypeValues.Bool);

            // ACT

            var result = Assert.Throws<InvalidOperationException>(
                () => new AssignedFacetPropertyNode(this.PersistenceMock.Object, e, e.Tags.Single().Facet.Properties.Single())
                    .SetItemProperties(this.ProviderContextMock.Object, new PSNoteProperty("Value", "wrong").Yield()));

            // ASSERT
            // value hasn't changed

            Assert.NotNull(result);
            Assert.False(e.TryGetFacetProperty(e.Tags.Single().Facet.Properties.Single()).exists);
        }

        #endregion ISetItemProperty

        #region IGetItemProperty

        [Fact]
        public void AssignedFacetPropertyNode_retrieves_properties_with_value()
        {
            // ARRANGE

            var e = DefaultEntity(WithAssignedDefaultTag);
            e.SetFacetProperty(e.Tags.Single().Facet.Properties.Single(), "2");

            // ACT

            var result = new AssignedFacetPropertyNode(this.PersistenceMock.Object, e, e.Tags.Single().Facet.Properties.Single())
               .GetItemProperties(this.ProviderContextMock.Object, Enumerable.Empty<string>());

            // ASSERT
            // name and value are returned

            Assert.Equal(new[] { "Name", "Value", "ValueType", "ItemType" }, result.Select(p => p.Name));
            Assert.Equal(new object[] { "p", "2", FacetPropertyTypeValues.String, TreeStoreItemType.AssignedFacetProperty }, result.Select(p => p.Value));
        }

        [Theory]
        [InlineData("value")]
        [InlineData("VALUE")]
        public void AssignedFacetPropertyNode_retrieves_specified_properties_with_value(string propertyName)
        {
            // ARRANGE

            var e = DefaultEntity(WithAssignedDefaultTag);
            e.SetFacetProperty(e.Tags.Single().Facet.Properties.Single(), "2");

            // ACT

            var result = new AssignedFacetPropertyNode(this.PersistenceMock.Object, e, e.Tags.Single().Facet.Properties.Single())
               .GetItemProperties(this.ProviderContextMock.Object, propertyName.Yield());

            // ASSERT
            // value propert is returned

            Assert.Equal("Value", result.Single().Name);
            Assert.Equal("2", result.Single().Value);
        }

        #endregion IGetItemProperty

        #region IClearItemProperty

        [Theory]
        [InlineData("value")]
        [InlineData("VALUE")]
        public void AssignedFacetPropertyNode_clears_Value_property(string propertyName)
        {
            // ARRANGE

            var e = DefaultEntity(WithAssignedDefaultTag);
            e.SetFacetProperty(e.Tags.Single().Facet.Properties.Single(), "1");

            this.ProviderContextMock
                .Setup(c => c.Persistence)
                .Returns(this.PersistenceMock.Object);

            this.PersistenceMock
              .Setup(m => m.Entities)
              .Returns(this.EntityRepositoryMock.Object);

            this.EntityRepositoryMock
                .Setup(r => r.Upsert(e))
                .Returns<Entity>(e => e);

            // ACT

            new AssignedFacetPropertyNode(this.PersistenceMock.Object, e, e.Tags.Single().Facet.Properties.Single())
                .ClearItemProperty(this.ProviderContextMock.Object, propertyName.Yield());

            // ASSERT

            Assert.Empty(e.Values);
        }

        #endregion IClearItemProperty
    }
}