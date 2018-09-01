using Kosmograph.Model;
using LiteDB;
using System.IO;
using Xunit;

namespace Kosmograph.LiteDb.Test
{
    public class LiteDbRepositoryTest
    {
        private readonly LiteDatabase database;
        private readonly LiteDbRepositoryBase<TestEntity> repository;
        private readonly LiteCollection<TestEntity> entities;

        private class TestEntity : EntityBase
        {
        }

        private class TestRepository : LiteDbRepositoryBase<TestEntity>
        {
            public TestRepository(LiteDatabase db)
                : base(db, "entities")
            { }
        }

        public LiteDbRepositoryTest()
        {
            this.database = new LiteDatabase(new MemoryStream());
            this.repository = new TestRepository(this.database);
            this.entities = this.database.GetCollection<TestEntity>("entities");
        }

        [Fact]
        public void LiteDbRepositoryBase_creates_entity()
        {
            // ARRANGE

            var entity = new TestEntity();

            // ACT

            this.repository.Upsert(entity);

            // ASSERT

            Assert.NotNull(entity.Id);
            Assert.NotEqual(ObjectId.Empty, entity.Id);

            var read = this.entities.FindById(entity.Id);

            Assert.NotNull(read);
            Assert.Equal(entity.Id, read.Id);
        }

        [Fact]
        public void LiteDbRepositoryBase_reads_entity()
        {
            // ARRANGE

            var entity = new TestEntity();
            this.repository.Upsert(entity);

            // ACT

            var result = this.repository.FindById(entity.Id);

            // ASSERT

            Assert.Equal(entity.Id, result.Id);
        }

        [Fact]
        public void LiteDbRepositoryBase_deletes_entity()
        {
            // ARRANGE

            var entity = new TestEntity();
            this.repository.Upsert(entity);

            // ACT

            var result = this.repository.Delete(entity.Id);

            // ASSERT

            Assert.Null(this.entities.FindById(entity.Id));
            Assert.Empty(this.entities.FindAll());
        }
    }
}