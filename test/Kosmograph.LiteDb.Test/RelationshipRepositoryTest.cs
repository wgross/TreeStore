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
    public class RelationshipRepositoryTest : IDisposable
    {
        private readonly MockRepository mocks = new MockRepository(MockBehavior.Strict);
        private readonly LiteRepository liteDb;
        private readonly Mock<IChangedMessageBus<IRelationship>> eventSource;
        private readonly EntityRepository entityRepository;
        private readonly TagRepository tagRepository;
        private readonly CategoryRepository categoryRepository;
        private readonly RelationshipRepository relationshipRepository;
        private readonly LiteCollection<BsonDocument> relationships;

        public RelationshipRepositoryTest()
        {
            this.liteDb = new LiteRepository(new MemoryStream());
            this.eventSource = this.mocks.Create<IChangedMessageBus<IRelationship>>();
            this.entityRepository = new EntityRepository(this.liteDb, this.mocks.Create<IChangedMessageBus<IEntity>>(MockBehavior.Loose).Object);
            this.tagRepository = new TagRepository(this.liteDb, this.mocks.Create<IChangedMessageBus<ITag>>(MockBehavior.Loose).Object);
            this.categoryRepository = new CategoryRepository(this.liteDb);
            this.relationshipRepository = new RelationshipRepository(this.liteDb, this.eventSource.Object);
            this.relationships = this.liteDb.Database.GetCollection("relationships");
        }

        public void Dispose()
        {
            this.mocks.VerifyAll();
        }

        [Fact]
        public void RelationshipRepository_writes_relationship_with_entities()
        {
            // ARRANGE

            var entity1 = this.entityRepository.Upsert(new Entity());
            var entity2 = this.entityRepository.Upsert(new Entity());
            var relationship = new Relationship("r", entity1, entity2);

            this.eventSource
                .Setup(s => s.Modified(relationship));

            // ACT

            this.relationshipRepository.Upsert(relationship);

            // ASSERT

            var readRelationship = this.relationships.FindAll().Single();

            Assert.NotNull(readRelationship);
            Assert.Equal(relationship.Id, readRelationship.AsDocument["_id"].AsGuid);
            Assert.Equal(relationship.From.Id, readRelationship["From"].AsDocument["$id"].AsGuid);
            Assert.Equal(EntityRepository.CollectionName, readRelationship["From"].AsDocument["$ref"].AsString);
            Assert.Equal(relationship.To.Id, readRelationship["To"].AsDocument["$id"].AsGuid);
            Assert.Equal(EntityRepository.CollectionName, readRelationship["To"].AsDocument["$ref"].AsString);
        }

        [Fact]
        public void RelationshipRepository_reads_relationship_with_entities()
        {
            // ARRANGE

            var entity1 = this.entityRepository.Upsert(new Entity());
            var entity2 = this.entityRepository.Upsert(new Entity());
            var relationship = new Relationship("r", entity1, entity2);

            this.eventSource
               .Setup(s => s.Modified(relationship));

            this.relationshipRepository.Upsert(relationship);

            // ACT

            var resultById = this.relationshipRepository.FindById(relationship.Id);
            var resultByAll = this.relationshipRepository.FindAll().Single();

            // ASSERT

            Assert.Equal(entity1, resultById.From);
            Assert.NotSame(entity1, resultById.From);
            Assert.Equal(entity2, resultById.To);
            Assert.NotSame(entity2, resultById.To);

            Assert.Equal(entity1, resultByAll.From);
            Assert.NotSame(entity1, resultByAll.From);
            Assert.Equal(entity2, resultByAll.To);
            Assert.NotSame(entity2, resultByAll.To);
        }

        [Fact]
        public void RelationshipRepository_writes_relationship_with_tag()
        {
            // ARRANGE

            var tag = this.tagRepository.Upsert(new Tag("tag", new Facet("facet", new FacetProperty("prop"))));
            var entity1 = this.entityRepository.Upsert(new Entity());
            var entity2 = this.entityRepository.Upsert(new Entity());
            var relationship = new Relationship("r", entity1, entity2, tag);

            this.eventSource
                .Setup(s => s.Modified(relationship));

            // ACT

            this.relationshipRepository.Upsert(relationship);

            // ASSERT

            var readRelationship = this.relationships.FindAll().Single();

            Assert.NotNull(readRelationship);
            Assert.Equal(relationship.Id, readRelationship.AsDocument["_id"].AsGuid);
            Assert.Equal(relationship.Tags.Single().Id, readRelationship["Tags"].AsArray[0].AsDocument["$id"].AsGuid);
            Assert.Equal(TagRepository.CollectionName, readRelationship["Tags"].AsArray[0].AsDocument["$ref"].AsString);
        }

        [Fact]
        public void RelationshipRepository_reads_relationship_with_tag()
        {
            // ARRANGE

            var tag = this.tagRepository.Upsert(new Tag("tag", new Facet("facet", new FacetProperty("prop"))));
            var entity1 = this.entityRepository.Upsert(new Entity());
            var entity2 = this.entityRepository.Upsert(new Entity());
            var relationship = new Relationship("r", entity1, entity2, tag);

            this.eventSource
                .Setup(s => s.Modified(relationship));

            this.relationshipRepository.Upsert(relationship);

            // ACT

            var resultById = this.relationshipRepository.FindById(relationship.Id);
            var resultByAll = this.relationshipRepository.FindAll().Single();

            // ASSERT

            Assert.Equal(tag, resultById.Tags.Single());
            Assert.NotSame(tag, resultById.Tags.Single());
            Assert.Equal(tag, resultByAll.Tags.Single());
            Assert.NotSame(tag, resultByAll.Tags.Single());
        }

        [Fact]
        public void RelationshipRepository_writes_and_reads_Relationship_with_FacetProperty_values()
        {
            // ARRANGE

            var tag = this.tagRepository.Upsert(new Tag("tag", new Facet("facet", new FacetProperty("prop"))));
            var entity1 = this.entityRepository.Upsert(new Entity());
            var entity2 = this.entityRepository.Upsert(new Entity());
            var relationship = new Relationship("r", entity1, entity2, tag);

            // set facet property value
            relationship.SetFacetProperty(tag.Facet.Properties.Single(), 1);

            this.eventSource
                .Setup(s => s.Modified(relationship));

            this.relationshipRepository.Upsert(relationship);

            // ACT

            var result = this.relationshipRepository.FindById(relationship.Id);

            // ASSERT

            var comp = relationship.DeepCompare(result);

            Assert.True(comp.AreEqual);
            Assert.Equal(1, result.TryGetFacetProperty(tag.Facet.Properties.Single()).Item2);
        }

        [Fact]
        public void RelationshipRepository_removes_relationship()
        {
            // ARRANGE

            var entity1 = this.entityRepository.Upsert(new Entity());
            var entity2 = this.entityRepository.Upsert(new Entity());
            var relationship = new Relationship("r", entity1, entity2);

            this.eventSource
                .Setup(s => s.Modified(relationship));

            this.relationshipRepository.Upsert(relationship);

            this.eventSource
                .Setup(s => s.Removed(relationship));

            // ACT

            var result = this.relationshipRepository.Delete(relationship);

            // ASSERT

            Assert.True(result);
            Assert.Throws<InvalidOperationException>(() => this.relationshipRepository.FindById(relationship.Id));
        }

        [Fact]
        public void RelationshipRepository_removing_unknown_entity_returns_false()
        {
            // ARRANGE

            var entity1 = this.entityRepository.Upsert(new Entity());
            var entity2 = this.entityRepository.Upsert(new Entity());
            var relationship = new Relationship("r", entity1, entity2);

            // ACT

            var result = this.relationshipRepository.Delete(relationship);

            // ASSERT

            Assert.False(result);
        }

        [Fact]
        public void RelationshipRepository_finds_relationship_by_entity()
        {
        }
    }
}