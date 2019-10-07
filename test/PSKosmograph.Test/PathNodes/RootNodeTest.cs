using PSKosmograph.PathNodes;
using System;
using System.Linq;
using Xunit;

namespace PSKosmograph.Test.PathNodes
{
    public class RootNodeTest : NodeTestBase
    {
        [Fact]
        public void RootNode_has_name_and_ItemMode()
        {
            // ACT

            var result = new RootNode();

            // ASSERT

            Assert.Equal("", result.Name);
            Assert.Equal("+", result.ItemMode);
            Assert.Null(result.GetNodeChildrenParameters);
        }

        [Fact]
        public void RootNode_provides_Value()
        {
            // ACT

            var result = new RootNode().GetNodeValue();

            // ASSERT

            Assert.Equal(string.Empty, result.Name);
            Assert.True(result.IsCollection);
            Assert.IsType<RootNode.Item>(result.Item);
        }

        [Fact]
        public void RootNodeValue_provides_Item()
        {
            // ACT

            var result = new RootNode().GetNodeValue().Item as RootNode.Item;

            // ASSERT

            Assert.IsType<RootNode.Item>(result);
        }

        [Fact]
        public void RootNode_retrives_top_level_child_nodes()
        {
            // ACT

            var result = new RootNode()
                .GetNodeChildren(this.ProviderContextMock.Object)
                .ToArray();

            // ASSERT

            Assert.Equal(3, result.Count());
            Assert.IsType<TagsNode>(result.ElementAt(0));
            Assert.IsType<EntitiesNode>(result.ElementAt(1));
            Assert.IsType<RelationshipsNode>(result.ElementAt(2));
        }

        [Fact]
        public void RootNode_resolves_null_name_as_all_child_nodes()
        {
            // ACT

            var result = new RootNode()
                .Resolve(this.ProviderContextMock.Object, null)
                .ToArray();

            // ASSERT

            Assert.Equal(3, result.Count());
            Assert.IsType<TagsNode>(result.ElementAt(0));
            Assert.IsType<EntitiesNode>(result.ElementAt(1));
            Assert.IsType<RelationshipsNode>(result.ElementAt(2));
        }

        [Theory]
        [InlineData("Tags", typeof(TagsNode))]
        [InlineData("Entities", typeof(EntitiesNode))]
        [InlineData("Relationships", typeof(RelationshipsNode))]
        public void RootNode_resolves_top_level_name_as_child_node(string name, Type type)
        {
            // ACT

            var result = new RootNode()
                .Resolve(this.ProviderContextMock.Object, name)
                .ToArray();

            // ASSERT

            Assert.IsType(type, result.Single());
        }

        [Fact]
        public void RootNode_resolves_unkown_top_level_name_as_empty()
        {
            // ACT

            var result = new RootNode()
                .Resolve(this.ProviderContextMock.Object, "unknown")
                .ToArray();

            // ASSERT

            Assert.Empty(result);
        }
    }
}