using Elementary.Compare;
using Kosmograph.Model;
using LiteDB;
using System.IO;
using System.Linq;
using Xunit;

namespace Kosmograph.LiteDb.Test
{
    public class TagRepositoryTest
    {
        private readonly LiteRepository liteDb;
        private readonly TagRepository repository;
        private readonly LiteCollection<BsonDocument> tags;

        public TagRepositoryTest()
        {
            this.liteDb = new LiteRepository(new MemoryStream());
            this.repository = new TagRepository(this.liteDb);
            this.tags = this.liteDb.Database.GetCollection("tags");
        }

        [Fact]
        public void TagRepository_writes_Tag_to_repository()
        {
            // ARRANGE

            var tag = new Tag("tag");

            // ACT

            this.repository.Upsert(tag);

            // ASSERT

            var readTag = this.tags.FindById(tag.Id);

            Assert.NotNull(readTag);
            Assert.Equal(tag.Id, readTag.AsDocument["_id"].AsGuid);
        }

        [Fact]
        public void TagRepository_writes_and_reads_Tag_with_Facet_from_repository()
        {
            // ARRANGE

            var tag = new Tag("tag", new Facet("facet", new FacetProperty("prop")));

            // ACT

            this.repository.Upsert(tag);
            var result = this.repository.FindById(tag.Id);

            // ASSERT

            var comp = tag.DeepCompare(result);

            Assert.True(tag.NoPropertyHasDefaultValue());
            Assert.False(comp.Different.Any());
        }

        [Fact]
        public void TagRepository_updates_and_reads_Tag_with_Facet_from_repository()
        {
            // ARRANGE

            var tag = new Tag("tag", new Facet("facet", new FacetProperty("prop")));
            this.repository.Upsert(tag);

            // ACT

            tag.Name = "name2";
            tag.AssignFacet(new Facet("facet2", new FacetProperty("prop2")));

            this.repository.Upsert(tag);

            // ASSERT

            Assert.True(tag.NoPropertyHasDefaultValue());
            Assert.False(tag.DeepCompare(this.repository.FindById(tag.Id)).Different.Any());
        }

        [Fact]
        public void TagRepository_rejects_duplicate_name()
        {
            // ARRANGE

            var tag = this.repository.Upsert(new Tag("tag"));

            // ACT

            var result = Assert.Throws<LiteException>(() => this.repository.Upsert(new Tag("TAG")));

            // ASSERT

            Assert.Equal("Cannot insert duplicate key in unique index 'Name'. The duplicate value is '\"tag\"'.", result.Message);
            Assert.Single(this.tags.FindAll());
        }

        [Fact]
        public void TagRepository_finds_all_tags()
        {
            // ARRANGE

            var tag = new Tag("tag", new Facet("facet", new FacetProperty("prop")));
            this.repository.Upsert(tag);

            // ACT

            var result = this.repository.FindAll();

            // ASSERT

            Assert.Equal(tag, result.Single());

            var comp = tag.DeepCompare(result.Single());

            Assert.True(tag.NoPropertyHasDefaultValue());
            Assert.False(comp.Different.Any());
        }
    }
}