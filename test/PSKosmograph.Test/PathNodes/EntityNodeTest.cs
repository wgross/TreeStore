using Kosmograph.Model;
using Moq;
using PSKosmograph.PathNodes;
using System;
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
            Assert.Equal(KosmographItemType.Entity, result!.ItemType);
        }

        [Fact]
        public void EntityNode_retrieves_assigned_tags()
        {
            // ARRANGE

            var e = DefaultEntity(WithDefaultTag);

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

            var e = DefaultEntity(WithDefaultTag);

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

            var e = DefaultEntity(WithDefaultTag);

            this.ProviderContextMock
                .Setup(p => p.Persistence)
                .Returns(this.PersistenceMock.Object);

            // ACT

            var result = new EntityNode(this.PersistenceMock.Object, e).Resolve(this.ProviderContextMock.Object, null);

            // ASSERT

            Assert.Single(result);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void EntityNode_removes_itself(bool recurse)
        {
            // ARRANGE

            var entity = DefaultEntity(WithoutTags);

            this.ProviderContextMock
              .Setup(c => c.Persistence)
              .Returns(this.PersistenceMock.Object);

            this.PersistenceMock
                .Setup(m => m.Entities)
                .Returns(this.EntityRepositoryMock.Object);

            this.EntityRepositoryMock
                .Setup(r => r.Delete(entity))
                .Returns(true);

            // ACT

            new EntityNode(this.PersistenceMock.Object, entity).RemoveItem(this.ProviderContextMock.Object, "t", recurse);
        }

        [Fact]
        public void EntityNode_removes_itself_with_assigend_tags()
        {
            // ARRANGE

            var entity = DefaultEntity(WithDefaultTag);

            this.ProviderContextMock
              .Setup(c => c.Persistence)
              .Returns(this.PersistenceMock.Object);

            this.PersistenceMock
                .Setup(m => m.Entities)
                .Returns(this.EntityRepositoryMock.Object);

            this.EntityRepositoryMock
                .Setup(r => r.Delete(entity))
                .Returns(true);

            // ACT

            new EntityNode(this.PersistenceMock.Object, entity)
                .RemoveItem(this.ProviderContextMock.Object, "t", recurse: true);
        }

        [Fact]
        public void EntityNode_removing_itself_with_assigned_tags_fails_gracefully_if_recurse_false()
        {
            // ARRANGE

            var entity = DefaultEntity(WithDefaultTag);

            // ACT

            new EntityNode(this.PersistenceMock.Object, entity)
                .RemoveItem(this.ProviderContextMock.Object, "t", recurse: false);
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

        [Fact]
        public void EntityNode_provides_NewItemTypesNames()
        {
            // ARRANGE

            var e = DefaultEntity();

            // ACT

            var result = new EntityNode(this.PersistenceMock.Object, e).NewItemTypeNames;

            // ASSERT

            Assert.Equal("AssignedTag".Yield(), result);
        }

        [Fact]
        public void EntityNode_adds_tag_by_name()
        {
            // ARRANGE

            var e = DefaultEntity(WithoutTags);

            this.ProviderContextMock
                .Setup(c => c.Persistence)
                .Returns(this.PersistenceMock.Object);

            this.PersistenceMock
                .Setup(p => p.Entities)
                .Returns(this.EntityRepositoryMock.Object);

            this.EntityRepositoryMock
                .Setup(r => r.Upsert(e))
                .Returns(e);

            this.PersistenceMock
                .Setup(p => p.Tags)
                .Returns(this.TagRepositoryMock.Object);

            this.TagRepositoryMock
                .Setup(r => r.FindByName("t"))
                .Returns(DefaultTag());

            // ACT

            var result = new EntityNode(this.PersistenceMock.Object, e).NewItem(this.ProviderContextMock.Object, "t", itemTypeName: null, newItemValue: null);

            // ASSERT

            Assert.IsType<AssignedTagNode.Value>(result);
        }

        [Fact]
        public void EntityNode_adding_tag_by_name_fails_for_unknown_tag()
        {
            // ARRANGE

            var e = DefaultEntity(WithoutTags);

            this.ProviderContextMock
                .Setup(c => c.Persistence)
                .Returns(this.PersistenceMock.Object);

            this.PersistenceMock
                .Setup(p => p.Tags)
                .Returns(this.TagRepositoryMock.Object);

            this.TagRepositoryMock
                .Setup(r => r.FindByName("t"))
                .Returns((Tag?)null);

            // ACT

            var result = Assert.Throws<InvalidOperationException>(() => new EntityNode(this.PersistenceMock.Object, e).NewItem(this.ProviderContextMock.Object, "t", itemTypeName: null, newItemValue: null));

            // ASSERT

            Assert.Equal($"tag(name='t') doesn't exist.", result.Message);
        }

        [Fact]
        public void EntityNode_adding_tag_twice_by_name_fails_gracefully()
        {
            // ARRANGE

            var e = DefaultEntity(WithDefaultTag);
            var tag = e.Tags.Single();

            this.ProviderContextMock
                .Setup(c => c.Persistence)
                .Returns(this.PersistenceMock.Object);

            this.PersistenceMock
                .Setup(p => p.Tags)
                .Returns(this.TagRepositoryMock.Object);

            this.TagRepositoryMock
                .Setup(r => r.FindByName("t"))
                .Returns(tag);

            // ACT

            var result = new EntityNode(this.PersistenceMock.Object, e).NewItem(this.ProviderContextMock.Object, "t", itemTypeName: null, newItemValue: null);

            // ASSERT

            Assert.IsType<AssignedTagNode.Value>(result);
        }

        [Fact]
        public void EntityNode_copies_itself_as_new_entity()
        {
            // ARRANGE

            var entity = DefaultEntity(WithDefaultTag);
            entity.SetFacetProperty(entity.Tags.Single().Facet.Properties.Single(), 1);

            this.ProviderContextMock
                .Setup(c => c.Persistence)
                .Returns(this.PersistenceMock.Object);

            this.PersistenceMock
                .Setup(m => m.Entities)
                .Returns(this.EntityRepositoryMock.Object);

            Entity? createdEntity = null;
            this.EntityRepositoryMock
                .Setup(r => r.Upsert(It.IsAny<Entity>()))
                .Callback<Entity>(e => createdEntity = e)
                .Returns(entity);

            var entityContainer = new EntitiesNode();

            // ACT

            new EntityNode(this.PersistenceMock.Object, entity)
                .CopyItem(this.ProviderContextMock.Object, "e", "ee", entityContainer.GetNodeValue(), recurse: false);

            // ASSERT

            Assert.Equal("ee", createdEntity!.Name);
            Assert.Equal(entity.Tags.Single(), createdEntity!.Tags.Single());
            Assert.Equal(entity.Values.Single().Value, createdEntity!.Values.Single().Value);
            Assert.Equal(entity.Values.Single().Key, createdEntity!.Values.Single().Key);
        }

        [Fact]
        public void EntityNode_renames_itself()
        {
            // ARRANGE

            var entity = DefaultEntity();

            this.ProviderContextMock
                .Setup(c => c.Persistence)
                .Returns(this.PersistenceMock.Object);

            this.PersistenceMock
                .Setup(m => m.Entities)
                .Returns(this.EntityRepositoryMock.Object);

            Entity? renamedEntity = null;
            this.EntityRepositoryMock
                .Setup(r => r.Upsert(It.IsAny<Entity>()))
                .Callback<Entity>(t => renamedEntity = t)
                .Returns(entity);

            // ACT

            new EntityNode(this.PersistenceMock.Object, entity).RenameItem(this.ProviderContextMock.Object, "e", "ee");

            // ASSERT

            Assert.Equal("ee", renamedEntity!.Name);
        }

        [Fact]
        public void EntityNode_renaming_doesnt_store_identical_name()
        {
            // ARRANGE

            var entity = DefaultEntity();

            // ACT

            new EntityNode(this.PersistenceMock.Object, entity).RenameItem(this.ProviderContextMock.Object, "e", "e");

            // ASSERT

            Assert.Equal("e", entity.Name);
        }
    }
}