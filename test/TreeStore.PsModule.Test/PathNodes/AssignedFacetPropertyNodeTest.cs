using TreeStore.PsModule.PathNodes;
using System.Linq;
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

            var e = DefaultEntity(WithDefaultTag);

            // ACT

            var result = new AssignedFacetPropertyNode(this.PersistenceMock.Object, e, e.Tags.Single().Facet.Properties.Single());

            // ASSERT

            Assert.Equal("p", result.Name);
            Assert.Equal(".", result.ItemMode);
        }

        [Fact]
        public void AssignedFacetPropertyNode_provides_Value()
        {
            // ARRANGE

            var e = DefaultEntity(WithDefaultTag);

            // ACT

            var result = new AssignedFacetPropertyNode(this.PersistenceMock.Object, e, e.Tags.Single().Facet.Properties.Single()).GetItemProvider();

            // ASSERT

            Assert.Equal("p", result.Name);
            Assert.False(result.IsContainer);
        }

        [Fact]
        public void AssignedFacetPropertyNodeValue_provides_Item()
        {
            // ARRANGE

            var e = DefaultEntity(WithDefaultTag);
            e.SetFacetProperty(e.Tags.Single().Facet.Properties.Single(), 2);

            // ACT

            var result = new AssignedFacetPropertyNode(this.PersistenceMock.Object, e, e.Tags.Single().Facet.Properties.Single())
                .GetItemProvider()
                .GetItem() as AssignedFacetPropertyNode.Item;

            // ASSERT

            Assert.Equal(TreeStoreItemType.AssignedFacetProperty, result!.ItemType);
            Assert.Equal("p", result!.Name);
            Assert.Equal(2, result!.Value);
            Assert.Equal(e.Tags.Single().Facet.Properties.Single().Type, result.ValueType);
        }

        #endregion P2F node structure

    }
}