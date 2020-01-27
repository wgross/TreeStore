using TreeStore.PsModule.PathNodes;
using System;
using System.Linq;
using Xunit;

namespace TreeStore.PsModule.Test.PathNodes
{
    public class RootNodeTest : NodeTestBase
    {
        #region P2F node structure

        [Fact]
        public void RootNode_has_name_and_ItemMode()
        {
            // ACT

            var result = new RootNode();

            // ASSERT

            Assert.Equal("", result.Name);
            Assert.Equal("+", result.ItemMode);
        }

        [Fact]
        public void RootNode_provides_Value()
        {
            // ACT

            var result = new RootNode().GetItemProvider();

            // ASSERT

            Assert.Equal(string.Empty, result.Name);
            Assert.True(result.IsContainer);
            Assert.IsType<RootNode.Item>(result.GetItem());
        }

        [Fact]
        public void RootNodeValue_provides_Item()
        {
            // ACT

            var result = new RootNode().GetItemProvider().GetItem() as RootNode.Item;

            // ASSERT

            Assert.IsType<RootNode.Item>(result);
        }

        #endregion P2F node structure

        [Fact]
        public void RootNode_retrives_top_level_child_nodes()
        {
            // ACT

            var result = new RootNode()
                .GetChildNodes(this.ProviderContextMock.Object)
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