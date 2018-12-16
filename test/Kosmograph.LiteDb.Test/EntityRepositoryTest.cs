using Elementary.Compare;
using Kosmograph.Messaging;
using Kosmograph.Model;
using LiteDB;
using Moq;
using System.IO;
using System.Linq;
using Xunit;

namespace Kosmograph.LiteDb.Test
{
    public class EntityRepositoryTest
    {
        private readonly LiteRepository liteDb;
        private readonly EntityRepository entityRepository;
        private readonly TagRepository tagRepository;
        private readonly CategoryRepository categoryRepository;
        private readonly LiteCollection<BsonDocument> entities;

        public EntityRepositoryTest()
        {
            this.liteDb = new LiteRepository(new MemoryStream());
            this.entityRepository = new EntityRepository(this.liteDb);
            this.tagRepository = new TagRepository(this.liteDb, Mock.Of<IChangedMessageBus<ITag>>());
            this.categoryRepository = new CategoryRepository(this.liteDb);
            this.entities = this.liteDb.Database.GetCollection("entities");
        }

        [Fact]
        public void EntityRepository_writes_entity_to_collection()
        {
            // ARRANGE

            var entity = new Entity("entity");

            // ACT

            this.entityRepository.Upsert(entity);

            // ASSERT

            var readEntity = this.entities.FindAll().Single();

            Assert.NotNull(readEntity);
            Assert.Equal(entity.Id, readEntity.AsDocument["_id"].AsGuid);
        }

        [Fact]
        public void EntityRepository_reads_entity_from_collection()
        {
            // ARRANGE

            var entity = this.entityRepository.Upsert(new Entity("entity")); ;

            // ACT

            var readById = this.entityRepository.FindById(entity.Id);
            var readByAll = this.entityRepository.FindAll().Single();

            // ASSERT

            Assert.Equal(entity, readByAll);
            Assert.Equal(entity, readById);
        }

        [Fact]
        public void EntityRepository_writes_entity_with_tag_to_collection()
        {
            // ARRANGE

            var tag = this.tagRepository.Upsert(new Tag("tag", new Facet("facet", new FacetProperty("prop"))));
            var entity = new Entity("entity", tag);

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
        public void EntityRepository_reads_entity_with_tag_from_collection()
        {
            // ARRANGE

            var tag = this.tagRepository.Upsert(new Tag("tag", new Facet("facet", new FacetProperty("prop"))));
            var entity = this.entityRepository.Upsert(new Entity("entity", tag));

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

            entityRepository.Upsert(entity);

            // ACT

            var result = this.entityRepository.FindById(entity.Id);

            // ASSERT

            var comp = entity.DeepCompare(result);

            Assert.True(comp.AreEqual);
            Assert.Equal(1, result.TryGetFacetProperty(result.Facets().Single().Properties.Single()).Item2);
        }
    }
}