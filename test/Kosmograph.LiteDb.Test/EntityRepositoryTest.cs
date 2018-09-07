using Elementary.Compare;
using Kosmograph.Model;
using LiteDB;
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
            this.tagRepository = new TagRepository(this.liteDb);
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

            var readTag = this.entities.FindById(entity.Id);

            Assert.NotNull(readTag);
            Assert.Equal(entity.Id, readTag.AsDocument["_id"].AsGuid);
        }

        [Fact]
        public void EntityRepository_writes_entity_with_tag_to_collection()
        {
            // ARRANGE

            var tag = this.tagRepository.Upsert(new Tag("tag", new Facet("facet", new FacetProperty("prop"))));
            var entity = new Entity("entity");

            // ACT

            entity.AddTag(tag);
            this.entityRepository.Upsert(entity);

            // ASSERT

            var readTag = this.entities.FindById(entity.Id);

            Assert.NotNull(readTag);
            Assert.Equal(entity.Id, readTag.AsDocument["_id"].AsGuid);
            Assert.Equal(entity.Tags.Single().Id, readTag["Tags"].AsArray[0].AsDocument["$id"].AsGuid);
            Assert.Equal(TagRepository.CollectionName, readTag["Tags"].AsArray[0].AsDocument["$ref"].AsString);
        }

        [Fact]
        public void EntityRepository_creates_and_reads_Entity()
        {
            // ARRANGE

            var entity = new Entity("entity");
            this.entityRepository.Upsert(entity);

            // ACT

            var result = this.entityRepository.FindById(entity.Id);

            // ASSERT

            var comp = entity.DeepCompare(result);

            Assert.True(comp.AreEqual);
        }

        [Fact]
        public void EntityRepository_creates_and_reads_Entity_with_Tag()
        {
            // ARRANGE

            var tag = this.tagRepository.Upsert(new Tag("tag", new Facet("facet", new FacetProperty("prop"))));
            var entity = new Entity("entity");
            entity.AddTag(tag);

            this.entityRepository.Upsert(entity);

            // ACT

            var result = this.entityRepository.FindById(entity.Id);

            // ASSERT

            var comp = entity.DeepCompare(result);

            Assert.False(comp.Different.Values.Any());
            Assert.False(comp.Different.Types.Any());
            Assert.False(comp.Missing.Any());
        }

        [Fact]
        public void EntityRepository_creates_and_reads_Entity_with_FacetProperty_values()
        {
            // ARRANGE

            var tag = this.tagRepository.Upsert(new Tag("tag", new Facet("facet", new FacetProperty("prop"))));
            var entity = new Entity("entity");
            entity.AddTag(tag);

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