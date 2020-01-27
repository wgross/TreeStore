﻿using Elementary.Compare;
using TreeStore.Messaging;
using TreeStore.Model;
using LiteDB;
using Moq;
using System;
using System.IO;
using System.Linq;
using Xunit;

namespace TreeStore.LiteDb.Test
{
    public class TagRepositoryTest : IDisposable
    {
        private readonly LiteRepository liteDb;
        private readonly Mock<IChangedMessageBus<ITag>> eventSource;
        private readonly TagRepository repository;
        private readonly LiteCollection<BsonDocument> tags;
        private readonly MockRepository mocks = new MockRepository(MockBehavior.Strict);

        public TagRepositoryTest()
        {
            this.liteDb = new LiteRepository(new MemoryStream());
            this.eventSource = this.mocks.Create<IChangedMessageBus<ITag>>();
            this.repository = new TagRepository(this.liteDb, this.eventSource.Object);
            this.tags = this.liteDb.Database.GetCollection("tags");
        }

        public void Dispose()
        {
            this.mocks.VerifyAll();
        }

        [Fact]
        public void TagRepository_writes_Tag_to_repository()
        {
            // ARRANGE

            var tag = new Tag("tag");

            this.eventSource
                .Setup(s => s.Modified(tag));

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

            var tag = new Tag("tag", new Facet("facet", new FacetProperty("prop", FacetPropertyTypeValues.Long)));

            this.eventSource
                .Setup(s => s.Modified(tag));

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

            var tag = new Tag("tag", new Facet("facet", new FacetProperty("prop", FacetPropertyTypeValues.Guid)));

            this.eventSource
                .Setup(s => s.Modified(tag));

            this.repository.Upsert(tag);

            // ACT

            tag.Name = "name2";
            tag.AssignFacet(new Facet("facet2", new FacetProperty("prop2", FacetPropertyTypeValues.Double)));

            this.repository.Upsert(tag);

            // ASSERT

            Assert.True(tag.NoPropertyHasDefaultValue());
            Assert.False(tag.DeepCompare(this.repository.FindById(tag.Id)).Different.Any());
        }

        [Fact]
        public void TagRepository_rejects_duplicate_name()
        {
            // ARRANGE

            var tag = new Tag("tag", new Facet("facet", new FacetProperty("prop")));

            this.eventSource
                .Setup(s => s.Modified(tag));

            this.repository.Upsert(tag);

            // ACT

            var result = Assert.Throws<LiteException>(() => this.repository.Upsert(new Tag("TAG")));

            // ASSERT
            // notification was sent only once

            this.eventSource.Verify(s => s.Modified(It.IsAny<Tag>()), Times.Once());

            Assert.Equal("Cannot insert duplicate key in unique index 'Name'. The duplicate value is '\"tag\"'.", result.Message);
            Assert.Single(this.tags.FindAll());
        }

        [Fact]
        public void TagRepository_finds_all_tags()
        {
            // ARRANGE

            var tag = new Tag("tag", new Facet("facet", new FacetProperty("prop", FacetPropertyTypeValues.Bool)));

            this.eventSource
                .Setup(s => s.Modified(tag));

            this.repository.Upsert(tag);

            // ACT

            var result = this.repository.FindAll();

            // ASSERT

            Assert.Equal(tag, result.Single());

            var comp = tag.DeepCompare(result.Single());

            Assert.True(tag.NoPropertyHasDefaultValue());
            Assert.False(comp.Different.Any());
        }

        [Fact]
        public void TagRepository_finding_tag_by_id_throws_on_missing_tag()
        {
            // ACT

            var result = Assert.Throws<InvalidOperationException>(() => this.repository.FindById(Guid.NewGuid()));

            // ASSERT

            Assert.NotNull(result);
        }

        [Fact]
        public void TagRepository_removes_tag_from_repository()
        {
            // ARRANGE

            var tag = new Tag("tag");

            this.eventSource
                .Setup(s => s.Modified(tag));

            this.repository.Upsert(tag);

            this.eventSource
                .Setup(s => s.Removed(tag));

            // ACT

            var result = this.repository.Delete(tag);

            // ASSERT

            Assert.True(result);
            Assert.Null(this.tags.FindById(tag.Id));
        }

        [Fact]
        public void TagRepository_removing_unknown_tag_returns_false()
        {
            // ARRANGE

            var tag = new Tag("tag");

            // ACT

            var result = this.repository.Delete(tag);

            // ASSERT

            Assert.False(result);
        }

        [Fact]
        public void TagRepository_finds_by_name()
        {
            // ARRANGE

            var tag = new Tag("tag", new Facet("facet", new FacetProperty("prop", FacetPropertyTypeValues.Decimal)));

            this.eventSource
                .Setup(s => s.Modified(tag));

            this.repository.Upsert(tag);

            // ACT

            var result = this.repository.FindByName("tag");

            // ASSERT

            Assert.Equal(tag, result);

            var comp = tag.DeepCompare(result);

            Assert.True(tag.NoPropertyHasDefaultValue());
            Assert.False(comp.Different.Any());
        }

        [Fact]
        public void TagRepository_finding_by_name_returns_null_on_missing_tag()
        {
            // ARRANGE

            var tag = new Tag("tag", new Facet("facet", new FacetProperty("prop")));

            this.eventSource
                .Setup(s => s.Modified(tag));

            this.repository.Upsert(tag);

            // ACT

            var result = this.repository.FindByName("unknown");

            // ASSERT

            Assert.Null(result);
        }
    }
}