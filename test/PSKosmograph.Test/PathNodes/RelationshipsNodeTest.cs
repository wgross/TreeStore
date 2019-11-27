using PSKosmograph.PathNodes;
using Xunit;

namespace PSKosmograph.Test.PathNodes
{
    public class RelationshipsNodeTest
    {
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

            var result = new RelationshipsNode().GetNodeValue();

            // ASSERT

            Assert.Equal("Relationships", result.Name);
            Assert.True(result.IsCollection);
        }

        [Fact]
        public void RelationshipsNode_provides_Item()
        {
            // ACT

            var result = new RelationshipsNode().GetNodeValue().Item as RelationshipsNode.Item;

            // ASSERT

            Assert.Equal("Relationships", result!.Name);
            Assert.NotNull(result);
        }

        [Fact]
        public void RelationshipsNode_provides_RelationshipsNodeValue()
        {
            // ACT

            var result = new RelationshipsNode().GetNodeValue();

            // ASSERT

            Assert.Equal("Relationships", result.Name);
            Assert.True(result.IsCollection);
            Assert.IsType<RelationshipsNode.Item>(result.Item);
        }
    }
}