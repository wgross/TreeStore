using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Kosmograph.Model.Test
{
    public class TagQueryTest : ModelTestBase
    {
        private readonly Tag queryTag;
        private readonly TagQuery tagQuery;

        public TagQueryTest()
        {
            this.queryTag = DefaultTag(t => t.Name = "query");
            this.tagQuery = new TagQuery(new KosmographModel(this.Persistence.Object), this.MessageBus, queryTag);
        }

        [Fact]
        public void TagQuery_publishes_underlying_tag()
        {
            // ASSERT

            Assert.Same(this.queryTag, this.tagQuery.Tag);

        }

        [Fact]
        public void TagQuery_starts_with_retrieving_matching_entities_and_relationships()
        {
            // ARRANGE

            var entity = DefaultEntity(setup: null, tags: this.queryTag);
            var entityRepository = this.Mocks.Create<IEntityRepository>();
            entityRepository
                .Setup(r => r.FindByTag(this.queryTag))
                .Returns(entity.Yield());

            this.Persistence
                .Setup(p => p.Entities)
                .Returns(entityRepository.Object);

            var relationship = DefaultRelationship(setup: null, tags: this.queryTag);
            var relationshipRepository = this.Mocks.Create<IRelationshipRepository>();
            relationshipRepository
                .Setup(r => r.FindByTag(this.queryTag))
                .Returns(relationship.Yield());

            this.Persistence
                .Setup(p => p.Relationships)
                .Returns(relationshipRepository.Object);

            // ACT

            var result = (new List<Entity>(), new List<Relationship>());
            this.tagQuery.EntityAdded = result.Item1.Add;
            this.tagQuery.RelationshipAdded = result.Item2.Add;
            this.tagQuery.StartQuery();

            // ASSERT

            Assert.Equal(entity, result.Item1.Single());
            Assert.Equal(relationship, result.Item2.Single());
        }

        [Fact]
        public void TagQuery_ignores_untagged_entity()
        {
            // ARRANGE

            var result = new List<Entity>();
            this.tagQuery.EntityAdded = result.Add;

            // ACT
            // send entity withouut the right tag

            this.MessageBus.Entities.Modified(DefaultEntity(tags: DefaultTag()));

            // ASSERT
            // entity was ignored

            Assert.Empty(result);
        }

        [Fact]
        public void TagQuery_removes_untagged_entity()
        {
            // ARRANGE

            var result = new List<Guid>();
            this.tagQuery.EntityRemoved = result.Add;
            var entity = DefaultEntity(tags: this.queryTag);
            this.MessageBus.Entities.Modified(entity);

            // ACT
            // send entity for the secod time without the tag

            entity.Tags.Clear();
            this.MessageBus.Entities.Modified(entity);

            // ASSERT
            // entity was removed

            Assert.Equal(entity.Id, result.Single());
        }

        [Fact]
        public void TagQuery_adds_newly_tagged_entity()
        {
            // ARRANGE

            var result = new List<Entity>();
            this.tagQuery.EntityAdded = result.Add;
            var entity = DefaultEntity(tags: DefaultTag());
            this.MessageBus.Entities.Modified(entity);

            // ACT
            // send entity for the secod time without the tag

            entity.Tags.Add(this.queryTag);
            this.MessageBus.Entities.Modified(entity);

            // ASSERT
            // entity was removed

            Assert.Equal(entity, result.Single());
        }

        [Fact]
        public void TagQuery_ignores_untagged_relationship()
        {
            // ARRANGE

            var result = new List<Relationship>();
            this.tagQuery.RelationshipAdded = result.Add;

            // ACT
            // send relationship without the right tag

            this.MessageBus.Relationships.Modified(DefaultRelationship(tags: DefaultTag()));

            // ASSERT
            // relationship was ignored

            Assert.Empty(result);
        }

        [Fact]
        public void TagQuery_removes_untagged_relationship()
        {
            // ARRANGE

            var result = new List<Guid>();
            this.tagQuery.RelationshipRemoved = result.Add;
            var relationship = DefaultRelationship(tags: this.queryTag);
            this.MessageBus.Relationships.Modified(relationship);

            // ACT
            // send entity for the second time without the tag

            relationship.Tags.Clear();
            this.MessageBus.Relationships.Modified(relationship);

            // ASSERT
            // entity was removed

            Assert.Equal(relationship.Id, result.Single());
        }
        
        [Fact]
        public void TagQuery_adds_newly_tagged_relationship()
        {
            // ARRANGE

            var result = new List<Relationship>();
            this.tagQuery.RelationshipAdded = result.Add;
            var relationship = DefaultRelationship(tags: DefaultTag());
            this.MessageBus.Relationships.Modified(relationship);

            // ACT
            // send entity for the secod time without the tag

            relationship.Tags.Add(this.queryTag);
            this.MessageBus.Relationships.Modified(relationship);

            // ASSERT
            // entity was removed

            Assert.Equal(relationship, result.Single());
        }
    }
}