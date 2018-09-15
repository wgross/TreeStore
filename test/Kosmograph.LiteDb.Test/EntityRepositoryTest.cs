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

            var readEntity = this.entities.FindAll().Single();

            Assert.NotNull(readEntity);
            Assert.Equal(entity.Id, readEntity.AsDocument["_id"].AsGuid);
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
            var entity = new Entity("entity", tag);

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

        [Fact]
        public void EntityRepository_reads_all_Entities_with_Tags()
        {
            // ARRANGE

            var tag = this.tagRepository.Upsert(new Tag("tag", new Facet("facet", new FacetProperty("prop"))));
            var entity = new Entity("entity", tag);

            // set facet property value
            entity.SetFacetProperty(entity.Facets().Single().Properties.Single(), 1);

            entityRepository.Upsert(entity);

            // ACT

            var result = this.entityRepository.FindAll().ToArray();

            // ASSERT

            var comp = entity.DeepCompare(result[0]);

            Assert.True(comp.AreEqual);
            Assert.Equal(1, result[0].TryGetFacetProperty(result[0].Facets().Single().Properties.Single()).Item2);
        }
    }
}