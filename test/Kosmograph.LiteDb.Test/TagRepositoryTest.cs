using Elementary.Compare;
using Kosmograph.Model;
using LiteDB;
using System.IO;
using Xunit;

namespace Kosmograph.LiteDb.Test
{
    public class TagRepositoryTest
    {
        private readonly LiteRepository database;
        private readonly LiteDbRepositoryBase<Tag> repository;
        private readonly LiteCollection<BsonDocument> tags;
        private readonly LiteCollection<BsonDocument> facets;

        public TagRepositoryTest()
        {
            this.database = new LiteRepository(new MemoryStream());
            this.repository = new TagRepository(this.database);
            this.tags = this.database.Database.GetCollection("tags");
            this.facets = this.database.Database.GetCollection("facets");
        }

        [Fact]
        public void Tag_is_written_to_repository()
        {
            // ARRANGE

            var tag = new Tag("tag", Facet.Empty);

            // ACT

            this.repository.Upsert(tag);

            // ASSERT

            var readTag = this.tags.FindById(tag.Id);

            Assert.NotNull(readTag);
            Assert.Equal(tag.Id, readTag.AsDocument["_id"].AsGuid);
            Assert.Equal(tag.Name, readTag.AsDocument["Name"].AsString);
        }

        [Fact]
        public void Tag_with_Facet_is_created_and_read_from_repository()
        {
            // ARRANGE

            var tag = new Tag("tag", new Facet("facet", new FacetProperty("prop")));
            this.repository.Upsert(tag);

            // ACT

            var result = this.repository.FindById(tag.Id);

            // ASSERT

            var comp = tag.DeepCompare(result);

            Assert.True(tag.NoPropertyHasDefaultValue());
            Assert.False(comp.Different.Any());
        }

        [Fact]
        public void Tag_with_Facet_is_updated_and_read_from_repository()
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
    }
}