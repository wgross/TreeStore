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
    public class RelationshipRepositoryTest
    {
        private readonly MockRepository mocks = new MockRepository(MockBehavior.Strict);
        private readonly LiteRepository liteDb;
        private readonly Mock<IKosmographMessageBus> messageBus;
        private readonly EntityRepository entityRepository;
        private readonly TagRepository tagRepository;
        private readonly CategoryRepository categoryRepository;
        private readonly RelationshipRepository relationshipRepository;
        private readonly LiteCollection<BsonDocument> relationships;

        public RelationshipRepositoryTest()
        {
            this.liteDb = new LiteRepository(new MemoryStream());
            this.messageBus = this.mocks.Create<IKosmographMessageBus>();
            this.entityRepository = new EntityRepository(this.liteDb);
            this.tagRepository = new TagRepository(this.liteDb, this.messageBus.Object.Tags);
            this.categoryRepository = new CategoryRepository(this.liteDb);
            this.relationshipRepository = new RelationshipRepository(this.liteDb);
            this.relationships = this.liteDb.Database.GetCollection("relationships");
        }

        [Fact]
        public void RelationshipRepository_writes_relationship_with_entities_to_collection()
        {
            // ARRANGE

            var entity1 = this.entityRepository.Upsert(new Entity());
            var entity2 = this.entityRepository.Upsert(new Entity());

            // ACT

            var relationship = this.relationshipRepository.Upsert(new Relationship("r", entity1, entity2));

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
        public void RelationshipRepository_reads_relationship_with_entities_from_collection()
        {
            // ARRANGE

            var entity1 = this.entityRepository.Upsert(new Entity());
            var entity2 = this.entityRepository.Upsert(new Entity());
            var relationship = this.relationshipRepository.Upsert(new Relationship("r", entity1, entity2));

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
        public void EntityRepository_writes_relationship_with_tag_to_collection()
        {
            // ARRANGE

            var tag = this.tagRepository.Upsert(new Tag("tag", new Facet("facet", new FacetProperty("prop"))));
            var entity1 = this.entityRepository.Upsert(new Entity());
            var entity2 = this.entityRepository.Upsert(new Entity());

            // ACT

            var relationship = this.relationshipRepository.Upsert(new Relationship("r", entity1, entity2, tag));

            // ASSERT

            var readRelationship = this.relationships.FindAll().Single();

            Assert.NotNull(readRelationship);
            Assert.Equal(relationship.Id, readRelationship.AsDocument["_id"].AsGuid);
            Assert.Equal(relationship.Tags.Single().Id, readRelationship["Tags"].AsArray[0].AsDocument["$id"].AsGuid);
            Assert.Equal(TagRepository.CollectionName, readRelationship["Tags"].AsArray[0].AsDocument["$ref"].AsString);
        }

        [Fact]
        public void EntityRepository_reads_relationship_with_tag_from_collection()
        {
            // ARRANGE

            var tag = this.tagRepository.Upsert(new Tag("tag", new Facet("facet", new FacetProperty("prop"))));
            var entity1 = this.entityRepository.Upsert(new Entity());
            var entity2 = this.entityRepository.Upsert(new Entity());
            var relationship = this.relationshipRepository.Upsert(new Relationship("r", entity1, entity2, tag));

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

            this.relationshipRepository.Upsert(relationship);

            // ACT

            var result = this.relationshipRepository.FindById(relationship.Id);

            // ASSERT

            var comp = relationship.DeepCompare(result);

            Assert.True(comp.AreEqual);
            Assert.Equal(1, result.TryGetFacetProperty(tag.Facet.Properties.Single()).Item2);
        }
    }
}