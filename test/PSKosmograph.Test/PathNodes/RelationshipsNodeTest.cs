using PSKosmograph.PathNodes;
using Xunit;

namespace PSKosmograph.Test.PathNodes
{
    public class RelationshipsNodeTest
    {
        #region P2F node structure

        [Fact]
        public void RelationshipsNode_has_name_amd_ItemMode()
        {
            // ACT

            var result = new RelationshipsNode();

            // ASSERT

            Assert.Equal("Relationships", result.Name);
            Assert.Equal("+", result.ItemMode);
        }

        [Fact]
        public void RelationshipsNode_provides_Value()
        {
            // ACT

            var result = new RelationshipsNode().GetItemProvider();

            // ASSERT

            Assert.Equal("Relationships", result.Name);
            Assert.True(result.IsContainer);
        }

        [Fact]
        public void RelationshipsNode_provides_Item()
        {
            // ACT

            var result = new RelationshipsNode().GetItemProvider().GetItem() as RelationshipsNode.Item;

            // ASSERT

            Assert.Equal("Relationships", result!.Name);
            Assert.NotNull(result);
        }

        #endregion P2F node structure

        [Fact]
        public void RelationshipsNode_provides_RelationshipsNodeValue()
        {
            // ACT

            var result = new RelationshipsNode().GetItemProvider();

            // ASSERT

            Assert.Equal("Relationships", result.Name);
            Assert.True(result.IsContainer);
            Assert.IsType<RelationshipsNode.Item>(result.GetItem());
        }
    }
}