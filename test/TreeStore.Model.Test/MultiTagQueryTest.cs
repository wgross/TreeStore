﻿using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace TreeStore.Model.Test
{
    public class MultiTagQueryTest : ModelTestBase
    {
        private readonly Mock<IEntityRepository> entityRepository;
        private readonly Mock<IRelationshipRepository> relationshipRepository;
        private readonly MultiTagQuery multiTagQuery;

        public MultiTagQueryTest()
        {
            this.entityRepository = this.Mocks.Create<IEntityRepository>();
            this.relationshipRepository = this.Mocks.Create<IRelationshipRepository>();
            this.multiTagQuery = new MultiTagQuery(this.NewModel());
        }

        #region Adding TagQueries

        [Fact]
        public void MultiTagQuery_adding_starts_query_and_sends_model_items()
        {
            // ARRANGE

            var queryTag = DefaultTag();
            var entity1 = DefaultEntity(e => e.AddTag(queryTag));
            var entity2 = DefaultEntity();
            var relationship = DefaultRelationship(entity1, entity2, r => r.AddTag(queryTag));

            entityRepository
                .Setup(r => r.FindByTag(queryTag))
                .Returns(entity1.Yield());

            this.Persistence
                .Setup(p => p.Entities)
                .Returns(entityRepository.Object);

            relationshipRepository
                .Setup(r => r.FindByTag(queryTag))
                .Returns(relationship.Yield());

            this.Persistence
                .Setup(p => p.Relationships)
                .Returns(relationshipRepository.Object);

            // ACT

            Entity result_entity = null;
            this.multiTagQuery.EntityAdded = e => result_entity = e;
            Relationship result_relationship = null;
            this.multiTagQuery.RelationshipAdded = r => result_relationship = r;
            this.multiTagQuery.Add(queryTag);

            // ASSERT

            Assert.Equal(entity1, result_entity);
            Assert.Equal(relationship, result_relationship);
            Assert.True(multiTagQuery.ContainsEntity(entity1.Id));
            Assert.False(multiTagQuery.ContainsEntity(entity2.Id));
            Assert.True(multiTagQuery.ContainsRelationship(relationship.Id));
        }

        [Fact]
        public void MultiTagQuery_adding_starts_query_and_sends_model_items_once()
        {
            // ARRANGE
            // entity1 and relatinship are in both queries

            var queryTag1 = DefaultTag();
            var queryTag2 = DefaultTag();
            var entity1 = DefaultEntity(e => { e.AddTag(queryTag1); e.AddTag(queryTag2); });
            var entity2 = DefaultEntity();
            var relationship = DefaultRelationship(entity1, entity2, r => { r.AddTag(queryTag1); r.AddTag(queryTag2); });

            entityRepository
                .Setup(r => r.FindByTag(queryTag1))
                .Returns(entity1.Yield());

            entityRepository
                .Setup(r => r.FindByTag(queryTag2))
                .Returns(entity1.Yield());

            this.Persistence
                .Setup(p => p.Entities)
                .Returns(entityRepository.Object);

            relationshipRepository
                .Setup(r => r.FindByTag(queryTag1))
                .Returns(relationship.Yield());

            relationshipRepository
                .Setup(r => r.FindByTag(queryTag2))
                .Returns(relationship.Yield());

            this.Persistence
                .Setup(p => p.Relationships)
                .Returns(relationshipRepository.Object);

            // ACT

            var result = (entities: new List<Entity>(), relationships: new List<Relationship>());
            this.multiTagQuery.EntityAdded = result.entities.Add;
            this.multiTagQuery.RelationshipAdded = result.relationships.Add;
            this.multiTagQuery.Add(queryTag1);
            this.multiTagQuery.Add(queryTag2);

            // ASSERT

            Assert.Equal(entity1, result.entities.Single());
            Assert.Equal(relationship, result.relationships.Single());
        }

        #endregion Adding TagQueries

        #region Remove TagQueries

        [Fact]
        public void MultiTagQuery_remove_TagQuery_removes_entities_and_relationships()
        {
            // ARRANGE

            var queryTag = DefaultTag();
            var entity1 = DefaultEntity(e => e.AddTag(queryTag));
            var entity2 = DefaultEntity();
            var relationship = DefaultRelationship(entity1, entity2, r => r.AddTag(queryTag));

            entityRepository
                .Setup(r => r.FindByTag(queryTag))
                .Returns(entity1.Yield());

            this.Persistence
                .Setup(p => p.Entities)
                .Returns(entityRepository.Object);

            relationshipRepository
                .Setup(r => r.FindByTag(queryTag))
                .Returns(relationship.Yield());

            this.Persistence
                .Setup(p => p.Relationships)
                .Returns(relationshipRepository.Object);

            this.multiTagQuery.Add(queryTag);

            // ACT

            var result = (entities: new List<Guid>(), relationships: new List<Guid>());
            this.multiTagQuery.EntityRemoved = result.entities.Add;
            var result_relationships = new List<Guid>();
            this.multiTagQuery.RelationshipRemoved = result.relationships.Add;
            this.multiTagQuery.Remove(this.multiTagQuery.TagQueries.Single());

            // ASSERT

            Assert.Equal(entity1.Id, result.entities.Single());
            Assert.Equal(relationship.Id, result.relationships.Single());
            Assert.False(multiTagQuery.ContainsEntity(entity1.Id));
            Assert.False(multiTagQuery.ContainsRelationship(relationship.Id));
        }

        #endregion Remove TagQueries

        #region Removing model items from TagQueries

        [Fact]
        public void MultiTagQuery_notifies_removal()
        {
            // ARRANGE

            var queryTag = DefaultTag();
            var entity1 = DefaultEntity(e => e.AddTag(queryTag));
            var entity2 = DefaultEntity();
            var relationship = DefaultRelationship(entity1, entity2, r => r.AddTag(queryTag));

            entityRepository
                .Setup(r => r.FindByTag(queryTag))
                .Returns(entity1.Yield());

            this.Persistence
                .Setup(p => p.Entities)
                .Returns(entityRepository.Object);

            relationshipRepository
                .Setup(r => r.FindByTag(queryTag))
                .Returns(relationship.Yield());

            this.Persistence
                .Setup(p => p.Relationships)
                .Returns(relationshipRepository.Object);

            this.multiTagQuery.Add(queryTag);

            entity1.RemoveTag(queryTag);
            relationship.RemoveTag(queryTag);

            // ACT

            var result_entity = new List<Guid>();
            this.multiTagQuery.EntityRemoved = result_entity.Add;

            var result_relationship = new List<Guid>();
            this.multiTagQuery.RelationshipRemoved = result_relationship.Add;

            this.MessageBus.Entities.Modified(entity1);
            this.MessageBus.Relationships.Modified(relationship);

            // ASSERT

            Assert.Equal(entity1.Id, result_entity.Single());
            Assert.Equal(relationship.Id, result_relationship.Single());
        }

        [Fact]
        public void MultiTagQuery_notifies_changed_on_tagging_changed_by_removing()
        {
            // ARRANGE
            // entity1 and relationship are in both queries

            var queryTag1 = DefaultTag();
            var queryTag2 = DefaultTag();
            var entity1 = DefaultEntity(e => { e.AddTag(queryTag1); e.AddTag(queryTag2); });
            var entity2 = DefaultEntity();
            var relationship = DefaultRelationship(entity1, entity2, r => { r.AddTag(queryTag1); r.AddTag(queryTag2); });

            entityRepository
                .Setup(r => r.FindByTag(queryTag1))
                .Returns(entity1.Yield());

            entityRepository
                .Setup(r => r.FindByTag(queryTag2))
                .Returns(entity1.Yield());

            entityRepository
                .Setup(r => r.FindById(entity1.Id))
                .Returns(entity1);

            this.Persistence
                .Setup(p => p.Entities)
                .Returns(entityRepository.Object);

            relationshipRepository
                .Setup(r => r.FindByTag(queryTag1))
                .Returns(relationship.Yield());

            relationshipRepository
                .Setup(r => r.FindByTag(queryTag2))
                .Returns(relationship.Yield());

            relationshipRepository
                .Setup(r => r.FindById(relationship.Id))
                .Returns(relationship);

            this.Persistence
                .Setup(p => p.Relationships)
                .Returns(relationshipRepository.Object);

            this.multiTagQuery.Add(queryTag1);
            this.multiTagQuery.Add(queryTag2);

            // ACT
            // Remove one query tag from entity and relationship to change their tagging

            entity1.RemoveTag(queryTag1);
            relationship.RemoveTag(queryTag1);

            var result_entity_removed = new List<Guid>();
            this.multiTagQuery.EntityRemoved = result_entity_removed.Add;
            var result_entity_changed = new List<Entity>();
            this.multiTagQuery.EntityChanged = result_entity_changed.Add;

            var result_relationship_removed = new List<Guid>();
            this.multiTagQuery.RelationshipRemoved = result_relationship_removed.Add;
            var result_relationship_changed = new List<Relationship>();
            this.multiTagQuery.RelationshipChanged = result_relationship_changed.Add;

            // notifying once instead of twice makes no difference because the source of the
            // duplicate event are the two TagQueries not the message bus.
            this.MessageBus.Entities.Modified(entity1);
            this.MessageBus.Relationships.Modified(relationship);

            // ASSERT
            // both remain in a single tyge query and thefore in the multi tag query
            // botzh have been changed in semantics of the multi tag query

            Assert.Empty(result_entity_removed);
            Assert.Equal(entity1, result_entity_changed.Single());
            Assert.Empty(result_relationship_removed);
            Assert.Equal(relationship, result_relationship_changed.Single());
        }

        [Fact]
        public void MultiTagQuery_notifies_changed_on_tagging_changed_by_adding()
        {
            // ARRANGE
            // entity1 and relationship are in singl query each

            var queryTag1 = DefaultTag();
            var queryTag2 = DefaultTag();
            var entity1 = DefaultEntity(e => { e.AddTag(queryTag1); });
            var entity2 = DefaultEntity();
            var relationship = DefaultRelationship(entity1, entity2, r => { r.AddTag(queryTag2); });

            entityRepository
                .Setup(r => r.FindByTag(queryTag1))
                .Returns(entity1.Yield());

            entityRepository
                .Setup(r => r.FindByTag(queryTag2))
                .Returns(Enumerable.Empty<Entity>());

            this.Persistence
                .Setup(p => p.Entities)
                .Returns(entityRepository.Object);

            relationshipRepository
                .Setup(r => r.FindByTag(queryTag1))
                .Returns(Enumerable.Empty<Relationship>());

            relationshipRepository
                .Setup(r => r.FindByTag(queryTag2))
                .Returns(relationship.Yield());

            this.Persistence
                .Setup(p => p.Relationships)
                .Returns(relationshipRepository.Object);

            this.multiTagQuery.Add(queryTag1);
            this.multiTagQuery.Add(queryTag2);

            // ACT
            // Remove one query tag from entity and relationship to change their tagging

            entity1.AddTag(queryTag2);
            relationship.AddTag(queryTag1);

            var result_entity_added = new List<Entity>();
            this.multiTagQuery.EntityAdded = result_entity_added.Add;
            var result_entity_changed = new List<Entity>();
            this.multiTagQuery.EntityChanged = result_entity_changed.Add;

            var result_relationship_added = new List<Relationship>();
            this.multiTagQuery.RelationshipAdded = result_relationship_added.Add;
            var result_relationship_changed = new List<Relationship>();
            this.multiTagQuery.RelationshipChanged = result_relationship_changed.Add;

            // notifying once instead of twice makes no difference because the source of the
            // duplicate event are the two TagQueries not the message bus.
            this.MessageBus.Entities.Modified(entity1);
            this.MessageBus.Relationships.Modified(relationship);

            // ASSERT
            // both remain in a single tyge query and thefore in the multi tag query
            // botzh have been changed in semantics of the multi tag query

            Assert.Empty(result_entity_added);
            Assert.Equal(entity1, result_entity_changed.Single());
            Assert.Empty(result_relationship_added);
            Assert.Equal(relationship, result_relationship_changed.Single());
        }

        [Fact]
        public void MultiTagQuery_notifies_removal_once()
        {
            // ARRANGE
            // entity1 and relatinship are in both queries

            var queryTag1 = DefaultTag();
            var queryTag2 = DefaultTag();
            var entity1 = DefaultEntity(e => { e.AddTag(queryTag1); e.AddTag(queryTag2); });
            var entity2 = DefaultEntity();
            var relationship = DefaultRelationship(entity1, entity2, r => { r.AddTag(queryTag1); r.AddTag(queryTag2); });

            entityRepository
                .Setup(r => r.FindByTag(queryTag1))
                .Returns(entity1.Yield());

            entityRepository
                .Setup(r => r.FindByTag(queryTag2))
                .Returns(entity1.Yield());

            this.Persistence
                .Setup(p => p.Entities)
                .Returns(entityRepository.Object);

            relationshipRepository
                .Setup(r => r.FindByTag(queryTag1))
                .Returns(relationship.Yield());

            relationshipRepository
                .Setup(r => r.FindByTag(queryTag2))
                .Returns(relationship.Yield());

            this.Persistence
                .Setup(p => p.Relationships)
                .Returns(relationshipRepository.Object);

            this.multiTagQuery.Add(queryTag1);
            this.multiTagQuery.Add(queryTag2);

            // ACT
            // Remove both tags from entity and relationship

            entity1.RemoveTag(queryTag1);
            entity1.RemoveTag(queryTag2);
            relationship.RemoveTag(queryTag1);
            relationship.RemoveTag(queryTag2);

            var result_entity = new List<Guid>();
            this.multiTagQuery.EntityRemoved = result_entity.Add;

            var result_relationship = new List<Guid>();
            this.multiTagQuery.RelationshipRemoved = result_relationship.Add;

            // notifying once instead of twice makes no difference because the source of the
            // duplicate event are the two TagQueries not the message bus.
            this.MessageBus.Entities.Modified(entity1);
            this.MessageBus.Relationships.Modified(relationship);

            // ASSERT

            Assert.Equal(entity1.Id, result_entity.Single());
            Assert.Equal(relationship.Id, result_relationship.Single());
        }

        #endregion Removing model items from TagQueries

        #region Changing model item in TagQueries

        [Fact]
        public void MultiTagQuery_ignores_general_changes()
        {
            // ARRANGE

            var queryTag1 = DefaultTag();
            var queryTag2 = DefaultTag();
            var entity1 = DefaultEntity(e => { e.AddTag(queryTag1); e.AddTag(queryTag2); });
            var entity2 = DefaultEntity();
            var relationship = DefaultRelationship(entity1, entity2, r => { r.AddTag(queryTag1); r.AddTag(queryTag2); });

            entityRepository
                .Setup(r => r.FindByTag(queryTag1))
                .Returns(entity1.Yield());

            entityRepository
                .Setup(r => r.FindByTag(queryTag2))
                .Returns(entity1.Yield());

            this.Persistence
                .Setup(p => p.Entities)
                .Returns(entityRepository.Object);

            relationshipRepository
                .Setup(r => r.FindByTag(queryTag1))
                .Returns(relationship.Yield());

            relationshipRepository
                .Setup(r => r.FindByTag(queryTag2))
                .Returns(relationship.Yield());

            this.Persistence
                .Setup(p => p.Relationships)
                .Returns(relationshipRepository.Object);

            this.multiTagQuery.Add(queryTag1);
            this.multiTagQuery.Add(queryTag2);

            // ACT

            var result_entity = new List<Entity>();
            this.multiTagQuery.EntityChanged = result_entity.Add;

            var result_relationship = new List<Relationship>();
            this.multiTagQuery.RelationshipChanged = result_relationship.Add;

            this.MessageBus.Entities.Modified(entity1);
            this.MessageBus.Relationships.Modified(relationship);

            // ASSERT

            Assert.Empty(result_entity);
            Assert.Empty(result_relationship);
        }

        #endregion Changing model item in TagQueries
    }
}