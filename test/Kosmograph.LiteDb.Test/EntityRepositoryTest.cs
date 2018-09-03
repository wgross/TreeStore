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
        private readonly EntityRepository repository;
        private readonly LiteCollection<BsonDocument> entities;

        public EntityRepositoryTest()
        {
            this.liteDb = new LiteRepository(new MemoryStream());
            this.repository = new EntityRepository(this.liteDb);
            this.entities = this.liteDb.Database.GetCollection("entities");
        }

        [Fact]
        public void Entity_is_written_to_repository()
        {
            // ARRANGE

            var entity = new Entity("entity");

            // ACT

            this.repository.Upsert(entity);

            // ASSERT

            var readTag = this.entities.FindById(entity.Id);

            Assert.NotNull(readTag);
            Assert.Equal(entity.Id, readTag.AsDocument["_id"].AsGuid);
        }

        [Fact]
        public void Entity_is_created_and_read_from_repository()
        {
            // ARRANGE

            var tag = new Entity("entity");
            this.repository.Upsert(tag);

            // ACT

            var result = this.repository.FindById(tag.Id);

            // ASSERT

            var comp = tag.DeepCompare(result);

            Assert.Equal(nameof(Entity.Tags), comp.Different.Types.Single());
            Assert.Equal(nameof(Entity.Tags), comp.Different.Values.Single());
        }

        [Fact]
        public void Entity_is_updated_and_read_from_repository()
        {
            // ARRANGE

            var entity = new Entity("entity");

            // ACT

            this.repository.Upsert(entity);
            var result = this.repository.FindById(entity.Id);

            // ASSERT

            var comp = entity.DeepCompare(result);

            Assert.Equal(nameof(Entity.Tags), comp.Different.Types.Single());
            Assert.Equal(nameof(Entity.Tags), comp.Different.Values.Single());
        }
    }
}