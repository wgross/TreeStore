using Kosmograph.Model.Base;
using LiteDB;
using System;
using System.IO;
using Xunit;

namespace Kosmograph.LiteDb.Test
{
    public class LiteDbRepositoryTest
    {
        private readonly LiteRepository database;
        private readonly LiteDbRepositoryBase<TestEntity> repository;
        private readonly LiteCollection<TestEntity> entities;

        private class TestEntity : NamedBase
        {
            public TestEntity() : base()
            { }

            public TestEntity(string name) : base(name)
            {
            }
        }

        private class TestRepository : LiteDbRepositoryBase<TestEntity>
        {
            public TestRepository(LiteRepository db)
                : base(db, "entities")
            { }
        }

        public LiteDbRepositoryTest()
        {
            this.database = new LiteRepository(new MemoryStream());
            this.repository = new TestRepository(this.database);
            this.entities = this.database.Database.GetCollection<TestEntity>("entities");
        }

        [Fact]
        public void LiteDbRepositoryBase_creates_entity()
        {
            // ARRANGE

            var entity = new TestEntity("name");

            // ACT

            var result = this.repository.Upsert(entity);

            // ASSERT

            Assert.Same(entity, result);
            Assert.NotEqual(Guid.Empty, entity.Id);

            var read = this.entities.FindById(entity.Id);

            Assert.NotNull(read);
            Assert.Equal(entity.Id, read.Id);
            Assert.Equal("name", read.Name);
        }

        [Fact]
        public void LiteDbRepositoryBase_updates_entity()
        {
            // ARRANGE

            var entity = new TestEntity("name");
            this.repository.Upsert(entity);

            // ACT

            entity.Name = "name2";
            var result = this.repository.Upsert(entity);

            // ASSERT

            Assert.Same(entity, result);

            var read = this.entities.FindById(entity.Id);

            Assert.NotNull(read);
            Assert.Equal(entity.Id, read.Id);
            Assert.Equal("name2", read.Name);
        }

        [Fact]
        public void LiteDbRepositoryBase_reads_entity()
        {
            // ARRANGE

            var entity = new TestEntity("name");
            this.repository.Upsert(entity);

            // ACT

            var result = this.repository.FindById(entity.Id);

            // ASSERT

            Assert.Equal(entity.Id, result.Id);
            Assert.Equal("name", result.Name);
        }

        [Fact]
        public void LiteDbRepositoryBase_deletes_entity()
        {
            // ARRANGE

            var entity = new TestEntity("name");
            this.repository.Upsert(entity);

            // ACT

            var result = this.repository.Delete(entity);

            // ASSERT

            Assert.Null(this.entities.FindById(entity.Id));
            Assert.Empty(this.entities.FindAll());
        }
    }
}