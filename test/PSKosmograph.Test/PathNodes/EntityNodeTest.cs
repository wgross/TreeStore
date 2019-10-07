using Kosmograph.Model;
using PSKosmograph.PathNodes;
using System.Linq;
using System.Management.Automation;
using Xunit;

namespace PSKosmograph.Test.PathNodes
{
    public class EntityNodeTest : NodeTestBase
    {
        [Fact]
        public void EntityNode_has_name_and_ItemMode()
        {
            // ACT

            var result = new EntityNode(this.PersistenceMock.Object, DefaultEntity());

            // ASSERT

            Assert.Equal("e", result.Name);
            Assert.Equal("+", result.ItemMode);
            Assert.Null(result.GetNodeChildrenParameters);
        }

        [Fact]
        public void EntityNode_provides_Value()
        {
            // ACT

            var result = new EntityNode(this.PersistenceMock.Object, DefaultEntity()).GetNodeValue();

            // ASSERT

            Assert.Equal("e", result.Name);
            Assert.True(result.IsCollection);
        }

        [Fact]
        public void EntityNodeValue_provides_Item()
        {
            // ARRANGE

            var e = DefaultEntity();

            // ACT

            var result = new EntityNode(this.PersistenceMock.Object, e).GetNodeValue().Item as EntityNode.Item;

            // ASSERT

            Assert.Equal(e.Id, result!.Id);
            Assert.Equal(e.Name, result!.Name);
        }

        [Fact]
        public void EntityNode_retrieves_assigned_tags()
        {
            // ARRANGE

            var e = DefaultEntity();

            this.ProviderContextMock
               .Setup(p => p.Persistence)
               .Returns(this.PersistenceMock.Object);

            // ACT

            var result = new EntityNode(this.PersistenceMock.Object, e).GetNodeChildren(this.ProviderContextMock.Object);

            // ASSERT

            Assert.Single(result);
        }

        [Fact]
        public void EntityNode_resolves_tag_name_as_AssignedTagNode()
        {
            // ARRANGE

            var e = DefaultEntity();

            this.ProviderContextMock
                .Setup(c => c.Persistence)
                .Returns(this.PersistenceMock.Object);

            // ACT

            var result = new EntityNode(this.PersistenceMock.Object, e).Resolve(this.ProviderContextMock.Object, "t").Single();

            // ASSERT

            Assert.IsType<AssignedTagNode>(result);
        }

        [Fact]
        public void EntityNode_resolves_unkown_tag_name_as_empty_result()
        {
            // ARRANGE

            var e = DefaultEntity();

            // ACT

            var result = new EntityNode(this.PersistenceMock.Object, e).Resolve(this.ProviderContextMock.Object, "unknown");

            // ASSERT

            Assert.Empty(result);
        }

        [Fact]
        public void EntityNode_resolves_null_tag_name_as_all_child_nodes()
        {
            // ARRANGE

            var e = DefaultEntity();

            this.ProviderContextMock
                .Setup(p => p.Persistence)
                .Returns(this.PersistenceMock.Object);

            // ACT

            var result = new EntityNode(this.PersistenceMock.Object, e).Resolve(this.ProviderContextMock.Object, null);

            // ASSERT

            Assert.Single(result);
        }

        [Fact]
        public void EntityNodeValue_sets_entity_name()
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

            new EntityNode(this.PersistenceMock.Object, e).GetNodeValue().SetItemProperties(new PSNoteProperty("Name", "changed").Yield());

            // ASSERT

            Assert.Equal("changed", e.Name);
        }
    }
}