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