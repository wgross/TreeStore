using Elementary.Compare;
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
        private readonly CategoryRepository categoryRepository;
        private readonly LiteCollection<BsonDocument> entities;
        private readonly MockRepository mocks = new MockRepository(MockBehavior.Strict);

        public EntityRepositoryTest()
        {
            this.liteDb = new LiteRepository(new MemoryStream());
            this.eventSource = this.mocks.Create<IChangedMessageBus<IEntity>>();
            this.entityRepository = new EntityRepository(this.liteDb, this.eventSource.Object);
            this.tagRepository = new TagRepository(this.liteDb, this.mocks.Create<IChangedMessageBus<ITag>>(MockBehavior.Loose).Object);
            this.categoryRepository = new CategoryRepository(this.liteDb);
            this.entities = this.liteDb.Database.GetCollection("entities");
        }

        public void Dispose()
        {
            this.mocks.VerifyAll();
        }

        [Fact]
        public void EntityRepository_writes_entity_to_repository()
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
        }

        [Fact]
        public void EntityRepository_reads_entity_from_repository()
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
        public void EntityRepository_writes_entity_with_tag_to_repository()
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
        public void EntityRepository_reads_entity_with_tag_from_repository()
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

            Assert.Equal(entity, readByAll);
            Assert.NotSame(entity, readByAll);
            Assert.Equal(entity, readById);
            Assert.NotSame(entity, readById);
        }

        [Fact]
        public void EntityRepository_writes_and_reads_Entity_with_FacetProperty_values()
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

            var result = this.entityRepository.FindById(entity.Id);

            // ASSERT

            var comp = entity.DeepCompare(result);

            Assert.True(comp.AreEqual);
            Assert.Equal(1, result.TryGetFacetProperty(result.Facets().Single().Properties.Single()).Item2);
        }

        [Fact]
        public void EntityRepository_removes_entity_from_repository()
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
        public void EntityRepository_removing_unknown_entity_from_repository_returns_false()
        {
            // ARRANGE

            var entity = new Entity("entity");

            // ACT

            var result = this.entityRepository.Delete(entity);

            // ASSERT

            Assert.False(result);
        }
    }
}