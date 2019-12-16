using Kosmograph.Model;
using PSKosmograph.PathNodes;
using System;
using System.Linq;
using System.Management.Automation;
using Xunit;

namespace PSKosmograph.Test.PathNodes
{
    public class AssignedTagNodeTest : NodeTestBase
    {
        [Fact]
        public void AssignedTagNode_has_name_and_ItemMode()
        {
            // ARRANGE

            var e = DefaultEntity(WithDefaultTag);

            // ACT

            var result = new AssignedTagNode(this.PersistenceMock.Object, e, e.Tags.Single());

            // ASSERT

            Assert.Equal("t", result.Name);
            Assert.Equal("+", result.ItemMode);
        }

        [Fact]
        public void AssignedTagNode_provides_Value()
        {
            // ARRANGE

            var e = DefaultEntity(WithDefaultTag);

            // ACT

            var result = new AssignedTagNode(this.PersistenceMock.Object, e, e.Tags.Single()).GetItemProvider();

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
            var result = node.GetNodeChildren(this.ProviderContextMock.Object);

            // ASSERT

            Assert.Single(result);
        }

        #region IGetItemProperties

        [Fact]
        public void AssignedTagNode_provides_assigned_properties_with_values()
        {
            // ARRANGE

            var e = DefaultEntity(WithDefaultTag);
            e.SetFacetProperty(e.Tags.Single().Facet.Properties.Single(), 1);

            // ACT

            var result = new AssignedTagNode(this.PersistenceMock.Object, e, e.Tags.Single()).GetItemProvider().GetItemProperties(propertyNames: null);

            // ASSERT

            Assert.Equal("p", result.Single().Name);
            Assert.Equal(1, result.Single().Value);
        }

        [Fact]
        public void AssignedTagNode_provides_single_assigned_property_with_value()
        {
            // ARRANGE

            var e = DefaultEntity(WithDefaultTag);
            e.SetFacetProperty(e.Tags.Single().Facet.Properties.Single(), 1);

            // ACT

            var result = new AssignedTagNode(this.PersistenceMock.Object, e, e.Tags.Single()).GetItemProvider().GetItemProperties(propertyNames: new[] { "p" });

            // ASSERT

            Assert.Equal("p", result.Single().Name);
            Assert.Equal(1, result.Single().Value);
        }

        [Fact]
        public void AssignedTagNode_provides_assigned_properties_without_value()
        {
            // ARRANGE

            var e = DefaultEntity(WithDefaultTag);

            // ACT

            var result = new AssignedTagNode(this.PersistenceMock.Object, e, e.Tags.Single()).GetItemProvider().GetItemProperties(propertyNames: null);

            // ASSERT

            Assert.Equal("p", result.Single().Name);
            Assert.Null(result.Single().Value);
        }

        #endregion IGetItemProperties

        [Theory]
        [InlineData("p")]
        [InlineData("P")]
        public void AssignedTageNode_resolves_property_name_as_AssignedFacetPropertyNode(string name)
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
        public void AssignedTagNode_resolves_unkown_property_name_as_empty_result()
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

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void AssignedTagNode_removes_itself(bool recurse)
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

            var node = new AssignedTagNode(this.PersistenceMock.Object, e, e.Tags.Single());
            node.RemoveItem(this.ProviderContextMock.Object, "t", recurse);

            // ARRANGE

            Assert.Empty(e.Tags);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void AssignedTagNode_removes_itself_with_values(bool recurse)
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

            var node = new AssignedTagNode(this.PersistenceMock.Object, e, e.Tags.Single());
            node.RemoveItem(this.ProviderContextMock.Object, "t", recurse);

            // ARRANGE
            // property values aren't child items.

            Assert.Empty(e.Tags);
        }

        [Fact]
        public void AssignedTagNodeValue_provides_Item()
        {
            // ARRANGE

            var e = DefaultEntity(WithDefaultTag);

            // ACT

            var result = new AssignedTagNode(this.PersistenceMock.Object, e, e.Tags.Single()).GetItemProvider().GetItem() as AssignedTagNode.Item;

            // ASSERT

            Assert.NotNull(result);
            Assert.Equal(e.Tags.Single().Name, result!.Name);
            Assert.Equal(e.Tags.Single().Id, result!.Id);
            Assert.Equal(KosmographItemType.AssignedTag, result!.ItemType);
        }

        [Fact]
        public void AssignedTagNodeValue_sets_facet_property_value()
        {
            // ARRANGE

            var e = DefaultEntity(WithDefaultTag);

            this.PersistenceMock
                .Setup(p => p.Entities)
                .Returns(this.EntityRepositoryMock.Object);

            this.EntityRepositoryMock
                .Setup(r => r.Upsert(e))
                .Returns(e);

            // ACT

            new AssignedTagNode(this.PersistenceMock.Object, e, e.Tags.Single()).GetItemProvider().SetItemProperties(new PSNoteProperty("p", 2).Yield());

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
                () => new AssignedTagNode(this.PersistenceMock.Object, e, e.Tags.Single()).GetItemProvider().SetItemProperties(new PSNoteProperty("p", 2).Yield()));

            // ASSERT

            Assert.NotNull(result);
            Assert.Null(e.TryGetFacetProperty(e.Tags.Single().Facet.Properties.Single()).value);
        }

        #region ClearItemProperty

        [Theory]
        [InlineData("p")]
        [InlineData("P")]
        public void ÁssignedTagNode_clears_assigned_tag_properties_by_name(string propertyName)
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
        public void AssignedTagNode_clearing_assigned_tag_properties_ignores_unknown_name()
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

        #endregion ClearItemProperty
    }
}