using Kosmograph.Messaging;
using Kosmograph.Model;
using LiteDB;
using Moq;
using System;
using System.IO;
using System.Linq;
using Xunit;

namespace Kosmograph.LiteDb.Test
{
    public class EntityRepositoryTest : IDisposable
    {
        private readonly LiteRepository liteDb;
        private readonly Mock<IChangedMessageBus<IEntity>> eventSource;
        private readonly EntityRepository entityRepository;
        private readonly TagRepository tagRepository;
        private readonly RelationshipRepository relationshipRepository;
        private readonly CategoryRepository categoryRepository;
        private readonly LiteCollection<BsonDocument> entities;
        private readonly MockRepository mocks = new MockRepository(MockBehavior.Strict);

        public EntityRepositoryTest()
        {
            this.liteDb = new LiteRepository(new MemoryStream());
            this.eventSource = this.mocks.Create<IChangedMessageBus<IEntity>>();
            this.entityRepository = new EntityRepository(this.liteDb, this.eventSource.Object);
            this.tagRepository = new TagRepository(this.liteDb, this.mocks.Create<IChangedMessageBus<ITag>>(MockBehavior.Loose).Object);
            this.relationshipRepository = new RelationshipRepository(this.liteDb, this.mocks.Create<IChangedMessageBus<IRelationship>>(MockBehavior.Loose).Object);
            this.categoryRepository = new CategoryRepository(this.liteDb);
            this.entities = this.liteDb.Database.GetCollection("entities");
        }

        public void Dispose()
        {
            this.mocks.VerifyAll();
        }

        [Fact]
        public void EntityRepository_writes_entity()
        {
            // ARRANGE

            var entity = new Entity("entity");

            this.eventSource
                .Setup(s => s.Modified(entity));

            // ACT

            this.entityRepository.Upsert(entity);

            // ASSERT

            var readEntity = this.entities.FindAll().Single();

            Assert.NotNull(readEntity);
            Assert.Equal(entity.Id, readEntity.AsDocument["_id"].AsGuid);
            Assert.Equal(entity.Name, readEntity.AsDocument["Name"].AsString);
        }

        [Fact]
        public void EntitiyRepository_rejects_duplicate_name()
        {
            // ARRANGE

            var entity = new Entity("entity");

            this.eventSource
                .Setup(s => s.Modified(entity));
            this.entityRepository.Upsert(entity);

            // ACT

            var result = Assert.Throws<LiteException>(() => this.entityRepository.Upsert(new Entity("entity")));

            // ASSERT
            // notification was sent only once

            this.eventSource.Verify(s => s.Modified(It.IsAny<Entity>()), Times.Once());

            Assert.Equal("Cannot insert duplicate key in unique index 'Name'. The duplicate value is '\"entity\"'.", result.Message);
            Assert.Single(this.entities.FindAll());
        }

        [Fact]
        public void EntityRepository_reads_entity_by_id()
        {
            // ARRANGE

            var entity = new Entity("entity");

            this.eventSource
                .Setup(s => s.Modified(entity));

            this.entityRepository.Upsert(entity);

            // ACT

            var readById = this.entityRepository.FindById(entity.Id);
            var readByAll = this.entityRepository.FindAll().Single();

            // ASSERT

            Assert.Equal(entity, readByAll);
            Assert.Equal(entity, readById);
        }

        [Fact]
        public void EntityRepository_writes_entity_with_tag()
        {
            // ARRANGE

            var tag = this.tagRepository.Upsert(new Tag("tag", new Facet("facet", new FacetProperty("prop"))));

            var entity = new Entity("entity", tag);

            this.eventSource
              .Setup(s => s.Modified(entity));

            // ACT

            this.entityRepository.Upsert(entity);

            // ASSERT

            var readEntity = this.entities.FindById(entity.Id);

            Assert.NotNull(readEntity);
            Assert.Equal(entity.Id, readEntity.AsDocument["_id"].AsGuid);
            Assert.Equal(entity.Tags.Single().Id, readEntity["Tags"].AsArray[0].AsDocument["$id"].AsGuid);
            Assert.Equal(TagRepository.CollectionName, readEntity["Tags"].AsArray[0].AsDocument["$ref"].AsString);
        }

        [Fact]
        public void EntityRepository_reads_entity_with_tag()
        {
            // ARRANGE

            var tag = this.tagRepository.Upsert(new Tag("tag", new Facet("facet", new FacetProperty("prop"))));

            var entity = new Entity("entity", tag);

            this.eventSource
                .Setup(s => s.Modified(entity));

            this.entityRepository.Upsert(entity);

            // ACT

            var readById = this.entityRepository.FindById(entity.Id);
            var readByAll = this.entityRepository.FindAll().Single();

            // ASSERT

            Assert.All(new[] { readByAll, readById }, e =>
            {
                Assert.Equal(entity, e);
                Assert.NotSame(entity, e);
                Assert.Equal(entity.Name, e.Name);
                Assert.Equal(entity.Tags.Single().Id, e.Tags.Single().Id);
                Assert.Equal(entity.Tags.Single().Name, e.Tags.Single().Name);
                Assert.Equal(entity.Tags.Single().Facet.Name, e.Tags.Single().Facet.Name);
                Assert.Equal(entity.Tags.Single().Facet.Properties.Single().Name, e.Tags.Single().Facet.Properties.Single().Name);
            });
        }

        [Fact]
        public void EntityRepository_writes_Entity_with_FacetProperty_values()
        {
            // ARRANGE

            var tag = this.tagRepository.Upsert(new Tag("tag", new Facet("facet", new FacetProperty("prop"))));

            var entity = new Entity("entity", tag);
            entity.SetFacetProperty(entity.Facets().Single().Properties.Single(), 1);

            this.eventSource
              .Setup(s => s.Modified(entity));

            // ACT

            this.entityRepository.Upsert(entity);

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

            var tag = this.tagRepository.Upsert(new Tag("tag", new Facet("facet", new FacetProperty("prop"))));
            var entity = new Entity("entity", tag);

            // set facet property value
            entity.SetFacetProperty(entity.Facets().Single().Properties.Single(), 1);

            this.eventSource
                .Setup(s => s.Modified(entity));

            entityRepository.Upsert(entity);

            // ACT

            var readById = this.entityRepository.FindById(entity.Id);
            var readByAll = this.entityRepository.FindAll().Single();

            // ASSERT

            Assert.All(new[] { readByAll, readById }, e =>
            {
                Assert.Equal(entity, e);
                Assert.NotSame(entity, e);
                Assert.Equal(entity.Name, e.Name);
                Assert.Equal(entity.Tags.Single().Id, e.Tags.Single().Id);
                Assert.Equal(entity.Tags.Single().Name, e.Tags.Single().Name);
                Assert.Equal(entity.Tags.Single().Facet.Name, e.Tags.Single().Facet.Name);
                Assert.Equal(entity.Tags.Single().Facet.Properties.Single().Name, e.Tags.Single().Facet.Properties.Single().Name);
                Assert.Equal(entity.Values[entity.Tags.Single().Facet.Properties.Single().Id.ToString()], e.Values[e.Tags.Single().Facet.Properties.Single().Id.ToString()]);
            });
        }

        [Fact]
        public void EntityRepository_removes_entity()
        {
            // ARRANGE

            var entity = new Entity("entity");

            this.eventSource
                .Setup(s => s.Modified(entity));

            this.entityRepository.Upsert(entity);

            this.eventSource
                .Setup(s => s.Removed(entity));

            // ACT

            var result = this.entityRepository.Delete(entity);

            // ASSERT

            Assert.True(result);
            Assert.Throws<InvalidOperationException>(() => this.entityRepository.FindById(entity.Id));
        }

        [Fact]
        public void EntityRepository_removing_unknown_entity_returns_false()
        {
            // ARRANGE

            var entity = new Entity("entity");

            // ACT

            var result = this.entityRepository.Delete(entity);

            // ASSERT

            Assert.False(result);
        }

        [Fact]
        public void EntityRepository_removing_entity_fails_if_used_in_relationship()
        {
            // ARRANGE

            var entity1 = new Entity("entity1");
            var entity2 = new Entity("entity2");

            this.eventSource
                .Setup(s => s.Modified(entity1));
            this.eventSource
                .Setup(s => s.Modified(entity2));

            this.entityRepository.Upsert(entity1);
            this.entityRepository.Upsert(entity2);

            var relationship = new Relationship("relationship1", entity1, entity2);

            this.relationshipRepository.Upsert(relationship);

            // ACT

            var result = this.entityRepository.Delete(entity1);

            // ASSERT

            Assert.False(result);
        }

        [Fact]
        public void EntityRepository_finds_entities_by_tag()
        {
            // ARRANGE

            this.eventSource
              .Setup(s => s.Modified(It.IsAny<Entity>()));

            var tag1 = this.tagRepository.Upsert(new Tag("t1"));
            var tag2 = this.tagRepository.Upsert(new Tag("t2"));
            var entity1 = this.entityRepository.Upsert(new Entity("entity1", tag1));
            var entity2 = this.entityRepository.Upsert(new Entity("entity2", tag2));

            // ACT

            var result = this.entityRepository.FindByTag(tag1);

            // ASSERT

            Assert.Single(result);
            Assert.Equal(entity1, result.Single());
        }

        [Fact]
        public void EntityRepository_finds_entity_by_name()
        {
            // ARRANGE

            this.eventSource
              .Setup(s => s.Modified(It.IsAny<Entity>()));

            var entity1 = this.entityRepository.Upsert(new Entity("entity"));

            // ACT

            var result = this.entityRepository.FindByName("entity");

            // ASSERT

            Assert.Equal(entity1.Id, result.Id);
        }
    }
}