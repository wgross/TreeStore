﻿using TreeStore.PsModule.PathNodes;
using Xunit;

namespace TreeStore.PsModule.Test.PathNodes
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
        }

        [Fact]
        public void RelationshipsNode_provides_Value()
        {
            // ACT

            var result = new RelationshipsNode();

            // ASSERT

            Assert.Equal("Relationships", result.Name);
            Assert.True(result.IsContainer);
        }

        [Fact]
        public void RelationshipsNode_provides_Item()
        {
            // ACT

            var result = new RelationshipsNode().GetItem();

            // ASSERT

            Assert.IsType<RelationshipsNode.Item>(result.ImmediateBaseObject);

            var resultValue = (RelationshipsNode.Item)result.ImmediateBaseObject;

            Assert.Equal("Relationships", resultValue.Name);
        }

        #endregion P2F node structure

        [Fact]
        public void RelationshipsNode_provides_RelationshipsNodeValue()
        {
            // ACT

            var result = new RelationshipsNode();

            // ASSERT

            Assert.Equal("Relationships", result.Name);
            Assert.True(result.IsContainer);
            Assert.IsType<RelationshipsNode.Item>(result.GetItem().ImmediateBaseObject);
        }
    }
}