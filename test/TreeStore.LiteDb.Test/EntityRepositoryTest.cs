using TreeStore.Messaging;
using TreeStore.Model;
using LiteDB;
using Moq;
using System;
using System.Linq;
using Xunit;
using static TreeStore.LiteDb.Test.TestDataSources;

namespace TreeStore.LiteDb.Test
{
    public class EntityRepositoryTest : LiteDbTestBase, IDisposable
    {
        private readonly RelationshipRepository relationshipRepository;
        private readonly LiteCollection<BsonDocument> entities;

        public EntityRepositoryTest()
        {
            this.relationshipRepository = new RelationshipRepository(this.LiteDb, this.Mocks.Create<IChangedMessageBus<IRelationship>>(MockBehavior.Loose).Object);
            this.entities = this.LiteDb.Database.GetCollection("entities");
        }

        [Fact]
        public void EntityRepository_writes_entity()
        {
            // ARRANGE

            var entity = DefaultEntity();

            this.EntityEventSource
                .Setup(s => s.Modified(entity));

            // ACT

            this.EntityRepository.Upsert(entity);

            // ASSERT

            var readEntity = this.entities.FindAll().Single();

            Assert.NotNull(readEntity);

            // name and id are there
            Assert.Equal(entity.Id, readEntity.AsDocument["_id"].AsGuid);
            Assert.Equal(entity.Name, readEntity.AsDocument["Name"].AsString);

            // unique identifier form category and root is stored
            Assert.Equal($"{entity.Name.ToLowerInvariant()}_{this.CategoryRepository.Root().Id}", readEntity.AsDocument["UniqueName"].AsString);

            // entity is in root category
            Assert.Equal(this.CategoryRepository.Root().Id, readEntity.AsDocument["Category"].AsDocument["$id"].AsGuid);
            Assert.Equal("categories", readEntity.AsDocument["Category"].AsDocument["$ref"].AsString);
            Assert.Equal("categories", readEntity.AsDocument["Category"].AsDocument["$ref"].AsString);

            // no tags
            Assert.Empty(readEntity.AsDocument["Tags"].AsArray);
        }

        [Fact]
        public void EntitiyRepository_writing_entity_rejects_duplicate_name_in_same_category()
        {
            // ARRANGE

            this.EntityEventSource
                .Setup(s => s.Modified(It.IsAny<Entity>()));

            var entity = this.EntityRepository.Upsert(DefaultEntity());

            // ACT

            var secondEntity = DefaultEntity();

            var result = Assert.Throws<LiteException>(() => this.EntityRepository.Upsert(secondEntity));

            // ASSERT

            // duplicate was rejected
            Assert.Equal("Cannot insert duplicate key in unique index 'UniqueName'. The duplicate value is '\"e_00000000-0000-0000-0000-000000000001\"'.", result.Message);

            // notification was sent only once
            this.EntityEventSource.Verify(s => s.Modified(It.IsAny<Entity>()), Times.Once());

            Assert.Single(this.entities.FindAll());
        }

        [Fact]
        public void EntitiyRepository_write_entity_with_duplicate_name_to_different_categories()
        {
            // ARRANGE

            this.EntityEventSource
                .Setup(s => s.Modified(It.IsAny<Entity>()));

            var entity = this.EntityRepository.Upsert(DefaultEntity());
            var category = this.CategoryRepository.Upsert(DefaultCategory());
            var secondEntity = DefaultEntity(WithEntityCategory(category));

            this.EntityEventSource
                .Setup(s => s.Modified(secondEntity));

            // ACT

            var result = this.EntityRepository.Upsert(secondEntity);

            // ASSERT
            // notification was sent only once

            this.EntityEventSource.Verify(s => s.Modified(It.IsAny<Entity>()), Times.Exactly(2));

            Assert.Equal(2, this.entities.FindAll().Count());
        }

        [Fact]
        public void EntityRepository_reads_entity_by_id()
        {
            // ARRANGE

            this.EntityEventSource
                .Setup(s => s.Modified(It.IsAny<Entity>()));

            var entity = this.EntityRepository.Upsert(DefaultEntity());

            // ACT

            var result = this.EntityRepository.FindById(entity.Id);

            // ASSERT

            Assert.Equal(entity, result);
        }

        [Fact]
        public void EntityRepository_removes_entity()
        {
            // ARRANGE

            this.EntityEventSource
                .Setup(s => s.Modified(It.IsAny<Entity>()));

            var entity = this.EntityRepository.Upsert(DefaultEntity());

            this.EntityEventSource
                .Setup(s => s.Removed(entity));

            // ACT

            var result = this.EntityRepository.Delete(entity);

            // ASSERT

            Assert.True(result);
            Assert.Throws<InvalidOperationException>(() => this.EntityRepository.FindById(entity.Id));
        }

        [Fact]
        public void EntityRepository_removing_unknown_entity_returns_false()
        {
            // ARRANGE

            var entity = new Entity("entity");

            // ACT

            var result = this.EntityRepository.Delete(entity);

            // ASSERT

            Assert.False(result);
        }

        [Fact]
        public void EntityRepository_removing_entity_fails_if_used_in_relationship()
        {
            // ARRANGE
            this.EntityEventSource
                .Setup(s => s.Modified(It.IsAny<Entity>()));

            var entity1 = this.EntityRepository.Upsert(DefaultEntity(e => e.Name = "entity1"));
            var entity2 = this.EntityRepository.Upsert(DefaultEntity(e => e.Name = "entity2"));

            this.relationshipRepository.Upsert(new Relationship("relationship1", entity1, entity2));

            // ACT

            var result = this.EntityRepository.Delete(entity1);

            // ASSERT

            Assert.False(result);
        }

        #region Entity -1:*-> Tag

        [Fact]
        public void EntityRepository_writes_entity_with_tag()
        {
            // ARRANGE

            this.TagEventSource.Setup(s => s.Modified(It.IsAny<Tag>()));

            var tag = this.TagRepository.Upsert(DefaultTag());

            var entity = DefaultEntity(e => e.AddTag(tag));

            this.EntityEventSource.Setup(s => s.Modified(entity));

            // ACT

            this.EntityRepository.Upsert(entity);

            // ASSERT

            var readEntity = this.entities.FindById(entity.Id);

            Assert.NotNull(readEntity);
            Assert.Equal(entity.Id, readEntity.AsDocument["_id"].AsGuid);
            Assert.Equal(entity.Tags.Single().Id, readEntity["Tags"].AsArray[0].AsDocument["$id"].AsGuid);
            Assert.Equal(TagRepository.CollectionName, readEntity["Tags"].AsArray[0].AsDocument["$ref"].AsString);
        }

        [Fact]
        public void EntityRepository_reads_entity_with_tag_by_Id()
        {
            // ARRANGE

            this.TagEventSource.Setup(s => s.Modified(It.IsAny<Tag>()));
            var tag = this.TagRepository.Upsert(DefaultTag());

            this.EntityEventSource.Setup(s => s.Modified(It.IsAny<Entity>()));
            var entity = this.EntityRepository.Upsert(DefaultEntity(e => e.AddTag(tag)));

            // ACT

            var result = this.EntityRepository.FindById(entity.Id);

            // ASSERT

            Assert.Equal(entity, result);
            Assert.NotSame(entity, result);
            Assert.Equal(entity.Name, result.Name);
            Assert.Equal(entity.Tags.Single().Id, result.Tags.Single().Id);
            Assert.Equal(entity.Tags.Single().Name, result.Tags.Single().Name);
            Assert.Equal(entity.Tags.Single().Facet.Name, result.Tags.Single().Facet.Name);
            Assert.Equal(entity.Tags.Single().Facet.Properties.Single().Name, result.Tags.Single().Facet.Properties.Single().Name);
        }

        [Fact]
        public void EntityRepository_reads_entity_with_tag_by_Name()
        {
            // ARRANGE

            this.TagEventSource.Setup(s => s.Modified(It.IsAny<Tag>()));
            var tag = this.TagRepository.Upsert(DefaultTag());

            this.EntityEventSource.Setup(s => s.Modified(It.IsAny<Entity>()));

            var entity = this.EntityRepository.Upsert(DefaultEntity(WithoutTags, WithEntityCategory(this.CategoryRepository.Root()), e => e.AddTag(tag)));

            // ACT

            var result = this.EntityRepository.FindAll().Single();

            // ASSERT

            Assert.Equal(entity, result);
            Assert.NotSame(entity, result);
            Assert.Equal(entity.Name, result.Name);
            Assert.Equal(entity.Tags.Single().Id, result.Tags.Single().Id);
            Assert.Equal(entity.Tags.Single().Name, result.Tags.Single().Name);
            Assert.Equal(entity.Tags.Single().Facet.Name, result.Tags.Single().Facet.Name);
            Assert.Equal(entity.Tags.Single().Facet.Properties.Single().Name, result.Tags.Single().Facet.Properties.Single().Name);
        }

        [Fact]
        public void EntityRepository_finds_entities_by_tag()
        {
            // ARRANGE

            this.TagEventSource.Setup(s => s.Modified(It.IsAny<Tag>()));
            var tag1 = this.TagRepository.Upsert(DefaultTag(t => t.Name = "t1"));
            var tag2 = this.TagRepository.Upsert(DefaultTag(t => t.Name = "t2"));

            this.EntityEventSource.Setup(s => s.Modified(It.IsAny<Entity>()));
            var entity1 = this.EntityRepository.Upsert(DefaultEntity(e =>
            {
                e.Name = "entity1";
                e.AddTag(tag1);
            }));

            var entity2 = this.EntityRepository.Upsert(DefaultEntity(e =>
            {
                e.Name = "entity2";
                e.AddTag(tag2);
            }));

            // ACT

            var result = this.EntityRepository.FindByTag(tag1);

            // ASSERT

            Assert.Single(result);
            Assert.Equal(entity1, result.Single());
        }

        #endregion Entity -1:*-> Tag

        #region Entity -0:*-> PropertyValues

        [Fact]
        public void EntityRepository_writes_Entity_with_FacetProperty_values()
        {
            // ARRANGE

            this.TagEventSource.Setup(s => s.Modified(It.IsAny<Tag>()));
            var tag = this.TagRepository.Upsert(DefaultTag());

            this.EntityEventSource.Setup(s => s.Modified(It.IsAny<Entity>()));
            var entity = this.EntityRepository.Upsert(DefaultEntity(e =>
            {
                e.AddTag(tag);
                e.SetFacetProperty(tag.Facet.Properties.Single(), 1);
            }));

            // ACT

            this.EntityRepository.Upsert(entity);

            // ASSERT

            var readEntity = this.entities.FindById(entity.Id);

            Assert.NotNull(readEntity);
            Assert.Equal(entity.Id, readEntity.AsDocument["_id"].AsGuid);
            Assert.Equal(entity.Values[entity.Tags.Single().Facet.Properties.Single().Id.ToString()], readEntity["Values"].AsDocument[entity.Tags.Single().Facet.Properties.Single().Id.ToString()].RawValue);
            Assert.Equal(TagRepository.CollectionName, readEntity["Tags"].AsArray[0].AsDocument["$ref"].AsString);
        }

        [Fact]
        public void EntityRepository_reads_Entity_with_FacetProperty_values()
        {
            // ARRANGE

            this.TagEventSource.Setup(s => s.Modified(It.IsAny<Tag>()));
            var tag = this.TagRepository.Upsert(DefaultTag());

            this.EntityEventSource.Setup(s => s.Modified(It.IsAny<Entity>()));
            var entity = this.EntityRepository.Upsert(DefaultEntity(WithEntityCategory(this.CategoryRepository.Root()), WithoutTags, e =>
             {
                 e.AddTag(tag);
             }));

            // set facet property value
            entity.SetFacetProperty(entity.Tags.Single().Facet.Properties.Single(), 1);

            this.EntityEventSource.Setup(s => s.Modified(entity));
            this.EntityRepository.Upsert(entity);

            // ACT

            var result = this.EntityRepository.FindById(entity.Id);

            // ASSERT

            Assert.Equal(entity, result);
            Assert.NotSame(entity, result);
            Assert.Equal(entity.Name, result.Name);
            Assert.Equal(entity.Tags.Single().Id, result.Tags.Single().Id);
            Assert.Equal(entity.Tags.Single().Name, result.Tags.Single().Name);
            Assert.Equal(entity.Tags.Single().Facet.Name, result.Tags.Single().Facet.Name);
            Assert.Equal(entity.Tags.Single().Facet.Properties.Single().Name, result.Tags.Single().Facet.Properties.Single().Name);
            Assert.Equal(entity.Values[entity.Tags.Single().Facet.Properties.Single().Id.ToString()], result.Values[result.Tags.Single().Facet.Properties.Single().Id.ToString()]);
        }

        #endregion Entity -0:*-> PropertyValues

        #region Entity -0:1-> Category

        [Fact]
        public void EntityRespository_writes_entity_with_category()
        {
            // ARRANGE

            this.EntityEventSource
              .Setup(s => s.Modified(It.IsAny<Entity>()));

            var category = this.CategoryRepository.Upsert(DefaultCategory());
            var entity = DefaultEntity(WithEntityCategory(category));

            // ACT

            this.EntityRepository.Upsert(entity);

            // ASSERT

            var readEntity = this.entities.FindById(entity.Id);

            Assert.NotNull(readEntity);
            Assert.Equal(entity.Id, readEntity.AsDocument["_id"].AsGuid);
            Assert.Equal(entity.Category.Id, readEntity["Category"].AsDocument["$id"].AsGuid);
            Assert.Equal(this.CategoryRepository.CollectionName, readEntity["Category"].AsDocument["$ref"].AsString);
        }

        [Fact]
        public void EntityRespository_reads_entity_with_category_by_id()
        {
            // ARRANGE

            this.EntityEventSource
              .Setup(s => s.Modified(It.IsAny<Entity>()));

            var category = this.CategoryRepository.Upsert(DefaultCategory());
            var entity = this.EntityRepository.Upsert(DefaultEntity(WithEntityCategory(category)));

            // ACT

            var result = this.EntityRepository.FindById(entity.Id);

            // ASSERT

            Assert.Equal(category.Id, result.Category.Id);
            Assert.Equal(category, result.Category);
            Assert.NotSame(entity, result);
            Assert.NotSame(entity.Category, result.Category);
        }

        [Fact]
        public void EntityRepository_finds_entity_by_category()
        {
            // ARRANGE

            this.EntityEventSource
              .Setup(s => s.Modified(It.IsAny<Entity>()));

            var category = this.CategoryRepository.Upsert(DefaultCategory());
            var entity = this.EntityRepository.Upsert(DefaultEntity(WithEntityCategory(category)));

            // ACT

            var result = this.EntityRepository.FindByCategory(category);

            // ASSERT

            Assert.Equal(entity.Id, result.Single().Id);
        }

        [Fact]
        public void EntityRepository_finds_entity_by_category_and_name()
        {
            // ARRANGE

            this.EntityEventSource
              .Setup(s => s.Modified(It.IsAny<Entity>()));

            var entity = this.EntityRepository.Upsert(DefaultEntity(e => e.Category = this.CategoryRepository.Root()));

            // ACT

            var result = this.EntityRepository.FindByCategoryAndName(this.CategoryRepository.Root(), entity.Name);

            // ASSERT

            Assert.Equal(entity.Id, result.Id);
        }

        #endregion Entity -0:1-> Category
    }
}