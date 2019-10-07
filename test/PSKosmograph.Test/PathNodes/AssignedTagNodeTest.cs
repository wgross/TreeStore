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

            var e = DefaultEntity();

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

            var e = DefaultEntity();

            // ACT

            var result = new AssignedTagNode(this.PersistenceMock.Object, e, e.Tags.Single()).GetNodeValue();

            // ASSERT

            Assert.Equal("t", result.Name);
            Assert.True(result.IsCollection);
        }

        [Fact]
        public void AssignedTagNodeValue_provides_Item()
        {
            // ARRANGE

            var e = DefaultEntity();

            // ACT

            var result = new AssignedTagNode(this.PersistenceMock.Object, e, e.Tags.Single()).GetNodeValue().Item as AssignedTagNode.Item;

            // ASSERT

            Assert.NotNull(result);
            Assert.Equal(e.Tags.Single().Name, result!.Name);
            Assert.Equal(e.Tags.Single().Id, result!.Id);
        }

        [Fact]
        public void AssignedTagNode_provides_assigned_properties_with_values()
        {
            // ARRANGE

            var e = DefaultEntity();
            e.SetFacetProperty(e.Tags.Single().Facet.Properties.Single(), 1);

            // ACT

            var result = new AssignedTagNode(this.PersistenceMock.Object, e, e.Tags.Single()).GetNodeValue().GetItemProperties(propertyNames: null);

            // ASSERT

            Assert.Equal("p", result.Single().Name);
            Assert.Equal(1, result.Single().Value);
        }

        [Fact]
        public void AssignedTagNode_provides_single_assigned_propertiy_with_value()
        {
            // ARRANGE

            var e = DefaultEntity();
            e.SetFacetProperty(e.Tags.Single().Facet.Properties.Single(), 1);

            // ACT

            var result = new AssignedTagNode(this.PersistenceMock.Object, e, e.Tags.Single()).GetNodeValue().GetItemProperties(propertyNames: new[] { "p" });

            // ASSERT

            Assert.Equal("p", result.Single().Name);
            Assert.Equal(1, result.Single().Value);
        }

        [Fact]
        public void AssignedTagNode_provides_assigned_properties_without_with_null_value()
        {
            // ARRANGE

            var e = DefaultEntity();

            // ACT

            var result = new AssignedTagNode(this.PersistenceMock.Object, e, e.Tags.Single()).GetNodeValue().GetItemProperties(propertyNames: null);

            // ASSERT

            Assert.Equal("p", result.Single().Name);
            Assert.Null(result.Single().Value);
        }

        [Fact]
        public void AssignedTagNodeValue_sets_facet_property_value()
        {
            // ARRANGE

            var e = DefaultEntity();

            this.PersistenceMock
                .Setup(p => p.Entities)
                .Returns(this.EntityRepositoryMock.Object);

            this.EntityRepositoryMock
                .Setup(r => r.Upsert(e))
                .Returns(e);

            // ACT

            new AssignedTagNode(this.PersistenceMock.Object, e, e.Tags.Single()).GetNodeValue().SetItemProperties(new PSNoteProperty("p", 2).Yield());

            // ASSERT

            Assert.Equal(2, e.TryGetFacetProperty(e.Tags.Single().Facet.Properties.Single()).value);
        }

        [Fact]
        public void AssignedTagNodeValue_setting_facet_property_value_rejects_wrong_type()
        {
            // ARRANGE

            var e = DefaultEntity(e => e.Tags.Single().Facet.Properties.Single().Type = FacetPropertyTypeValues.Bool);

            // ACT

            var result = Assert.Throws<InvalidOperationException>(
                () => new AssignedTagNode(this.PersistenceMock.Object, e, e.Tags.Single()).GetNodeValue().SetItemProperties(new PSNoteProperty("p", 2).Yield()));

            // ASSERT

            Assert.NotNull(result);
            Assert.Null(e.TryGetFacetProperty(e.Tags.Single().Facet.Properties.Single()).value);
        }
    }
}