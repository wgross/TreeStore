using Kosmograph.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Kosmograph.LiteDb.Test
{
    public class TagQueryTest : LiteDbTestBase
    {
        private readonly Tag tag;
        private readonly TagQuery tagQuery;

        public TagQueryTest()
        {
            this.tag = this.Persistence.Tags.Upsert(DefaultTag(t => t.Name = "query"));
            this.tagQuery = new TagQuery(new KosmographModel(this.Persistence), this.MessageBus, tag);
        }

        private Tag DefaultTag(Action<Tag> setup = null) => Setup(new Tag("t", new Facet("f", new FacetProperty("p"))), setup);

        private Entity DefaultEntity(Action<Entity> setup = null, params Tag[] tags) => Setup(new Entity("e", tags), setup);

        private Entity DefaultEntity(Action<Entity> setup = null) => Setup(new Entity("e", DefaultTag()), setup);

        private Relationship DefaultRelationship(Action<Relationship> setup = null, params Tag[] tags) => Setup(new Relationship("r", DefaultEntity(), DefaultEntity(), tags));

        #region Entities

        [Fact]
        public void TagQuery_retrieves_all_matching_entities()
        {
            // ARRANGE

            var tag2 = this.Persistence.Tags.Upsert(DefaultTag(t => t.Name = "tag2"));
            var entity1 = this.Persistence.Entities.Upsert(DefaultEntity(tags: this.tag));
            var entity2 = this.Persistence.Entities.Upsert(DefaultEntity(tags: tag2));

            // ACT

            var result = new List<Entity>();
            this.tagQuery.EntityAdded = result.Add;
            this.tagQuery.StartQuery();

            // ASSERT

            Assert.Equal(entity1, result.Single());
        }

        [Fact]
        public void TagQuery_adds_newly_tagged_entity()
        {
            // ARRANGE

            var result = new List<Entity>();
            this.tagQuery.EntityAdded = result.Add;
            this.tagQuery.StartQuery();

            // ACT

            var entity1 = this.Persistence.Entities.Upsert(DefaultEntity(tags: this.tag));

            // ASSERT

            Assert.Equal(entity1, result.Single());
        }

        [Fact]
        public void TagQuery_adds_entity_once()
        {
            // ARRANGE

            var result = new List<Entity>();
            this.tagQuery.EntityAdded = result.Add;
            this.tagQuery.StartQuery();

            // ACT

            var entity1 = this.Persistence.Entities.Upsert(DefaultEntity(tags: this.tag));
            this.Persistence.Entities.Upsert(entity1);

            // ASSERT

            Assert.Equal(entity1, result.Single());
        }

        [Fact]
        public void TagQuery_removes_untagged_entity()
        {
            // ARRANGE

            var entity1 = this.Persistence.Entities.Upsert(DefaultEntity(tags: this.tag));

            var result = new List<Guid>();
            this.tagQuery.EntityRemoved = result.Add;
            this.tagQuery.StartQuery();

            // ACT

            entity1.Tags.Clear();
            this.Persistence.Entities.Upsert(entity1);

            // ASSERT

            Assert.Equal(entity1.Id, result.Single());
        }

        [Fact]
        public void TagQuery_removes_untagged_entity_once()
        {
            // ARRANGE

            var entity1 = this.Persistence.Entities.Upsert(DefaultEntity(tags: this.tag));

            var result = new List<Guid>();
            this.tagQuery.EntityRemoved = result.Add;
            this.tagQuery.StartQuery();

            // ACT

            entity1.Tags.Clear();
            this.Persistence.Entities.Upsert(entity1);
            this.Persistence.Entities.Upsert(entity1);

            // ASSERT

            Assert.Equal(entity1.Id, result.Single());
        }

        [Fact]
        public void TagQuery_removes_deleted_entity()
        {
            // ARRANGE

            var entity1 = this.Persistence.Entities.Upsert(DefaultEntity(tags: this.tag));
            this.tagQuery.StartQuery();

            // ACT

            var result = new List<Guid>();
            this.tagQuery.EntityRemoved = result.Add;

            entity1.Tags.Clear();
            this.Persistence.Entities.Delete(entity1);

            // ASSERT

            Assert.Equal(entity1.Id, result.Single());
        }

        #endregion Entities

        #region Relationships

        [Fact]
        public void TagQuery_retrieves_all_matching_relationships()
        {
            // ARRANGE

            var relationship1 = DefaultRelationship(tags: this.tag);
            var entity1 = this.Persistence.Entities.Upsert(relationship1.From);
            var entity2 = this.Persistence.Entities.Upsert(relationship1.To);
            this.Persistence.Relationships.Upsert(relationship1);

            var relationship2 = DefaultRelationship(tags: DefaultTag());
            var entity3 = this.Persistence.Entities.Upsert(relationship2.From);
            var entity4 = this.Persistence.Entities.Upsert(relationship2.To);
            this.Persistence.Relationships.Upsert(relationship2);

            // ACT

            var result = this.tagQuery.GetRelationships().ToArray();

            // ASSERT

            Assert.Equal(relationship1, result.Single());
        }

        [Fact]
        public void TagQuery_adds_newly_tagged_relationship()
        {
            var relationship1 = DefaultRelationship(tags: this.tag);
            var entity1 = this.Persistence.Entities.Upsert(relationship1.From);
            var entity2 = this.Persistence.Entities.Upsert(relationship1.To);

            this.tagQuery.StartQuery();

            // ACT

            var result = new List<Relationship>();
            this.tagQuery.RelationshipAdded = result.Add;
            this.Persistence.Relationships.Upsert(relationship1);

            // ASSERT

            Assert.Equal(relationship1, result.Single());
        }

        [Fact]
        public void TagQuery_removes_untagged_relationship()
        {
            // ARRANGE

            var relationship1 = DefaultRelationship(tags: this.tag);
            var entity1 = this.Persistence.Entities.Upsert(relationship1.From);
            var entity2 = this.Persistence.Entities.Upsert(relationship1.To);
            this.Persistence.Relationships.Upsert(relationship1);

            this.tagQuery.StartQuery();

            // ACT

            var result = new List<Guid>();
            this.tagQuery.RelationshipRemoved = result.Add;

            relationship1.Tags.Clear();
            this.Persistence.Relationships.Upsert(relationship1);

            // ASSERT

            Assert.Equal(relationship1.Id, result.Single());
        }

        [Fact]
        public void TagQuery_removes_untagged_relationship_once()
        {
            // ARRANGE

            var relationship1 = DefaultRelationship(tags: this.tag);
            var entity1 = this.Persistence.Entities.Upsert(relationship1.From);
            var entity2 = this.Persistence.Entities.Upsert(relationship1.To);
            this.Persistence.Relationships.Upsert(relationship1);

            var result = new List<Guid>();
            this.tagQuery.RelationshipRemoved = result.Add;
            this.tagQuery.StartQuery();

            // ACT

            relationship1.Tags.Clear();
            this.Persistence.Relationships.Upsert(relationship1);
            this.Persistence.Relationships.Upsert(relationship1);

            // ASSERT

            Assert.Equal(relationship1.Id, result.Single());
        }

        [Fact]
        public void TagQuery_removes_deleted_relationship()
        {
            // ARRANGE

            var relationship1 = DefaultRelationship(tags: this.tag);
            var entity1 = this.Persistence.Entities.Upsert(relationship1.From);
            var entity2 = this.Persistence.Entities.Upsert(relationship1.To);
            this.Persistence.Relationships.Upsert(relationship1);
            this.tagQuery.StartQuery();

            // ACT

            var result = new List<Guid>();
            this.tagQuery.RelationshipRemoved = result.Add;

            entity1.Tags.Clear();
            this.Persistence.Relationships.Delete(relationship1);

            // ASSERT

            Assert.Equal(relationship1.Id, result.Single());
        }

        #endregion Relationships
    }
}