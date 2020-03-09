using System;
using System.Linq;
using System.Management.Automation;
using TreeStore.Model;
using TreeStore.PsModule.PathNodes;
using Xunit;

namespace TreeStore.PsModule.Test.PathNodes
{
    public class AssignedTagNodeTest : NodeTestBase
    {
        #region P2F node structure

        [Fact]
        public void AssignedTagNode_has_name_and_ItemMode()
        {
            // ARRANGE

            var e = DefaultEntity(WithDefaultTag);

            // ACT

            var result = new AssignedTagNode(this.PersistenceMock.Object, e, e.Tags.Single());

            // ASSERT

            Assert.Equal("t", result.Name);
        }

        [Fact]
        public void AssignedTagNode_provides_Value()
        {
            // ARRANGE

            var e = DefaultEntity(WithDefaultTag);

            // ACT

            var result = new AssignedTagNode(this.PersistenceMock.Object, e, e.Tags.Single());

            // ASSERT

            Assert.Equal("t", result.Name);
            Assert.True(result.IsContainer);
        }

        [Fact]
        public void AssignedTagNode_retrieves_assigned_facet_properties_as_child_nodes()
        {
            // ARRANGE

            var e = DefaultEntity(WithDefaultTag);

            // ACT

            var node = new AssignedTagNode(this.PersistenceMock.Object, e, e.Tags.Single());
            var result = node.GetChildNodes(this.ProviderContextMock.Object);

            // ASSERT

            Assert.Single(result);
        }

        #endregion P2F node structure

        #region IGetItem

        [Fact]
        public void AssignedTagNode_provides_Item()
        {
            // ARRANGE

            var e = DefaultEntity(WithDefaultTag);
            e.SetFacetProperty(e.Tags.Single().Facet.Properties.Single(), 1);

            // ACT

            var result = new AssignedTagNode(this.PersistenceMock.Object, e, e.Tags.Single()).GetItem(this.ProviderContextMock.Object);

            // ASSERT

            Assert.Equal("t", result.Property<string>("Name"));
            Assert.Equal(TreeStoreItemType.AssignedTag, result.Property<TreeStoreItemType>("ItemType"));
            Assert.Equal(1, result.Property<int>("p"));
            Assert.Equal("p", result.Property<string[]>("Properties").Single());
            Assert.IsType<AssignedTagNode.Item>(result.ImmediateBaseObject);
        }

        #endregion IGetItem

        [Theory]
        [InlineData("p")]
        [InlineData("P")]
        public void AssignedTagNode_resolves_property_name_as_AssignedFacetPropertyNode(string name)
        {
            // ARRANGE

            var e = DefaultEntity(WithDefaultTag);

            this.ProviderContextMock
                .Setup(p => p.Persistence)
                .Returns(this.PersistenceMock.Object);

            // ACT

            var result = new AssignedTagNode(this.PersistenceMock.Object, e, e.Tags.Single()).Resolve(this.ProviderContextMock.Object, name).Single();

            // ASSERT

            Assert.IsType<AssignedFacetPropertyNode>(result);
        }

        [Fact]
        public void AssignedTagNode_resolves_unknown_property_name_as_empty_result()
        {
            // ARRANGE

            var e = DefaultEntity(WithDefaultTag);

            // ACT

            var result = new AssignedTagNode(this.PersistenceMock.Object, e, e.Tags.Single()).Resolve(this.ProviderContextMock.Object, "unknown");

            // ASSERT

            Assert.Empty(result);
        }

        [Fact]
        public void AssignedTagNode_resolves_null_tag_name_as_all_child_nodes()
        {
            // ARRANGE

            var e = DefaultEntity(WithDefaultTag);

            // ACT

            var result = new AssignedTagNode(this.PersistenceMock.Object, e, e.Tags.Single()).Resolve(this.ProviderContextMock.Object, null);

            // ASSERT

            Assert.Single(result);
        }

        #region IRemoveItem

        [Fact]
        public void AssignedTagNode_removes_itself()
        {
            // ARRANGE

            var e = DefaultEntity(WithDefaultTag);

            this.ProviderContextMock
                .Setup(p => p.Persistence)
                .Returns(this.PersistenceMock.Object);

            this.PersistenceMock
                .Setup(p => p.Entities)
                .Returns(this.EntityRepositoryMock.Object);

            this.EntityRepositoryMock
                .Setup(r => r.Upsert(e))
                .Returns(e);

            // ACT

            new AssignedTagNode(this.PersistenceMock.Object, e, e.Tags.Single()).RemoveItem(this.ProviderContextMock.Object, "t");

            // ARRANGE

            Assert.Empty(e.Tags);
        }

        [Fact]
        public void AssignedTagNode_removes_itself_with_values()
        {
            // ARRANGE

            var e = DefaultEntity(WithDefaultTag, WithDefaultPropertySet(value: "test"));

            this.ProviderContextMock
                .Setup(p => p.Persistence)
                .Returns(this.PersistenceMock.Object);

            this.PersistenceMock
                .Setup(p => p.Entities)
                .Returns(this.EntityRepositoryMock.Object);

            this.EntityRepositoryMock
                .Setup(r => r.Upsert(e))
                .Returns(e);

            // ACT

            new AssignedTagNode(this.PersistenceMock.Object, e, e.Tags.Single()).RemoveItem(this.ProviderContextMock.Object, "t");

            // ARRANGE
            // property values aren't child items.

            Assert.Empty(e.Tags);
        }

        #endregion IRemoveItem

        #region IGetItemProperties

        [Fact]
        public void AssignedTagNode_retrieves_properties_with_values()
        {
            // ARRANGE

            var e = DefaultEntity(WithDefaultTag);
            e.SetFacetProperty(e.Tags.Single().Facet.Properties.Single(), 1);

            // ACT

            var result = new AssignedTagNode(this.PersistenceMock.Object, e, e.Tags.Single()).GetItemProperties(this.ProviderContextMock.Object, propertyNames: Enumerable.Empty<string>());

            // ASSERT

            Assert.Equal(new[] { "p", "Name", "ItemType", "Properties" }, result.Select(p => p.Name));
            Assert.Equal(new object[] { 1, "t", TreeStoreItemType.AssignedTag, new string[] { "p" } }, result.Select(p => p.Value));
        }

        [Theory]
        [InlineData("NAME")]
        [InlineData("P")]
        public void AssignedTagNode_retrieves_specified_property_with_value(string propertyName)
        {
            // ARRANGE

            var e = DefaultEntity(WithDefaultTag);
            e.SetFacetProperty(e.Tags.Single().Facet.Properties.Single(), 1);

            // ACT

            var result = new AssignedTagNode(this.PersistenceMock.Object, e, e.Tags.Single()).GetItemProperties(this.ProviderContextMock.Object, propertyNames: new[] { propertyName });

            // ASSERT

            Assert.Single(result);
        }

        [Fact]
        public void AssignedTagNodeValue_rertrieving_property_provides_parameter_with_completer()
        {
            // ARRANGE

            var e = DefaultEntity(WithDefaultTag);

            // ACT

            var result = (RuntimeDefinedParameterDictionary)new AssignedTagNode(this.PersistenceMock.Object, e, e.Tags.Single()).GetItemPropertyParameters;

            // ASSERT

            Assert.True(result.TryGetValue("TreeStorePropertyName", out var parameter));
            Assert.Single(parameter!.Attributes.Where(a => a.GetType().Equals(typeof(ParameterAttribute))));

            var resultValidateSet = (ValidateSetAttribute)parameter!.Attributes.Single(a => a.GetType().Equals(typeof(ValidateSetAttribute)));

            Assert.Equal(new[] { "p", "Name" }, resultValidateSet.ValidValues);
        }

        #endregion IGetItemProperties

        #region ISetItemProperties

        [Theory]
        [InlineData("p")]
        [InlineData("P")]
        public void AssignedTagNodeValue_sets_facet_property_value(string propertyName)
        {
            // ARRANGE

            var e = DefaultEntity(WithDefaultTag);

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

            new AssignedTagNode(this.PersistenceMock.Object, e, e.Tags.Single())
                .SetItemProperties(this.ProviderContextMock.Object, new PSNoteProperty(propertyName, 2).Yield());

            // ASSERT

            Assert.Equal(2, e.TryGetFacetProperty(e.Tags.Single().Facet.Properties.Single()).value);
        }

        [Fact]
        public void AssignedTagNodeValue_setting_facet_property_value_rejects_wrong_type()
        {
            // ARRANGE

            var e = DefaultEntity(WithDefaultTag, e => e.Tags.Single().Facet.Properties.Single().Type = FacetPropertyTypeValues.Bool);

            // ACT

            var result = Assert.Throws<InvalidOperationException>(
                () => new AssignedTagNode(this.PersistenceMock.Object, e, e.Tags.Single()).SetItemProperties(this.ProviderContextMock.Object, new PSNoteProperty("p", 2).Yield()));

            // ASSERT

            Assert.NotNull(result);
            Assert.False(e.TryGetFacetProperty(e.Tags.Single().Facet.Properties.Single()).exists);
        }

        [Fact]
        public void AssignedTagNodeValue_setting_facet_property_provides_parameter_with_completer()
        {
            // ARRANGE

            var e = DefaultEntity(WithDefaultTag);

            // ACT

            var result = (RuntimeDefinedParameterDictionary)new AssignedTagNode(this.PersistenceMock.Object, e, e.Tags.Single()).SetItemPropertyParameters;

            // ASSERT

            Assert.True(result.TryGetValue("TreeStorePropertyName", out var parameter));
            Assert.Single(parameter!.Attributes.Where(a => a.GetType().Equals(typeof(ParameterAttribute))));

            var resultValidateSet = (ValidateSetAttribute)parameter!.Attributes.Single(a => a.GetType().Equals(typeof(ValidateSetAttribute)));

            Assert.Equal("p", resultValidateSet.ValidValues.Single());
        }

        #endregion ISetItemProperties

        #region IClearItemProperty

        [Theory]
        [InlineData("p")]
        [InlineData("P")]
        public void AssignedTagNode_clears_facet_property_value(string propertyName)
        {
            // ARRANGE

            var e = DefaultEntity(WithDefaultTag);
            e.SetFacetProperty(e.Tags.Single().Facet.Properties.Single(), 1);

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

            new AssignedTagNode(this.PersistenceMock.Object, e, e.Tags.Single())
                .ClearItemProperty(this.ProviderContextMock.Object, propertyName.Yield());

            // ASSERT

            Assert.Empty(e.Values);
        }

        [Fact]
        public void AssignedTagNode_clearing_facet_property_value_ignores_unknown_name()
        {
            // ARRANGE

            var e = DefaultEntity(WithDefaultTag);
            e.SetFacetProperty(e.Tags.Single().Facet.Properties.Single(), 1);

            // ACT

            new AssignedTagNode(this.PersistenceMock.Object, e, e.Tags.Single())
                .ClearItemProperty(this.ProviderContextMock.Object, "unknown".Yield());

            // ASSERT

            Assert.Single(e.Values);
        }

        [Fact]
        public void AssignedTagNodeValue_clearing_facet_property_provides_parameter_with_completer()
        {
            // ARRANGE

            var e = DefaultEntity(WithDefaultTag);

            // ACT

            var result = (RuntimeDefinedParameterDictionary)new AssignedTagNode(this.PersistenceMock.Object, e, e.Tags.Single()).ClearItemPropertyParameters;

            // ASSERT

            Assert.True(result.TryGetValue("TreeStorePropertyName", out var parameter));
            Assert.Single(parameter!.Attributes.Where(a => a.GetType().Equals(typeof(ParameterAttribute))));

            var resultValidateSet = (ValidateSetAttribute)parameter!.Attributes.Single(a => a.GetType().Equals(typeof(ValidateSetAttribute)));

            Assert.Equal("p", resultValidateSet.ValidValues.Single());
        }

        #endregion IClearItemProperty
    }
}