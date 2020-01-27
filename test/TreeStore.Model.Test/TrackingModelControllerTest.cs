using System;
using Xunit;

namespace TreeStore.Model.Test
{
    public class TrackingModelControllerTest : ModelTestBase
    {
        private readonly TrackingModelController modelController;

        public TrackingModelControllerTest()
        {
            this.modelController = new TrackingModelController(this.NewModel());
        }

        [Fact]
        public void TrackingModelController_raises_TagAdded_on_first_changed_message()
        {
            // ARRANGE

            var tag = DefaultTag();

            // ACT

            var result = default(Tag);
            this.modelController.TagAdded = t => result = t;
            this.MessageBus.Tags.Modified(tag);

            // ASSERT

            Assert.Same(tag, result);
            Assert.True(this.modelController.ContainsTag(tag));
        }

        [Fact]
        public void TrackingModelController_raises_TagChanged_on_second_changed_message()
        {
            // ARRANGE

            var tag = DefaultTag();
            this.MessageBus.Tags.Modified(tag);

            // ACT

            var result = default(Tag);
            this.modelController.TagChanged = t => result = t;
            this.MessageBus.Tags.Modified(tag);

            // ASSERT

            Assert.Same(tag, result);
        }

        [Fact]
        public void TrackingModelController_doesnt_contain_tag_after_TagRemoved()
        {
            // ARRANGE

            var tag = DefaultTag();
            this.MessageBus.Tags.Modified(tag);

            // ACT

            var result = default(Guid?);
            this.modelController.TagRemoved = id => result = id;
            this.MessageBus.Tags.Removed(tag);

            // ASSERT

            Assert.Equal(tag.Id, result);
            Assert.False(this.modelController.ContainsTag(tag));
        }

        [Fact]
        public void TrackingModelController_doesnt_raise_TagRemoved_on_second_removed_message()
        {
            // ARRANGE

            var tag = DefaultTag();
            this.MessageBus.Tags.Modified(tag);
            this.MessageBus.Tags.Removed(tag);

            // ACT

            var result = default(Guid?);
            this.modelController.TagRemoved = id => result = id;
            this.MessageBus.Tags.Removed(tag);

            // ASSERT

            Assert.Null(result);
        }

        [Fact]
        public void TrackingModelController_raises_EntityAdded_on_first_changed_message()
        {
            // ARRANGE

            var entity = DefaultEntity();

            // ACT

            var result = default(Entity);
            this.modelController.EntityAdded = e => result = e;
            this.MessageBus.Entities.Modified(entity);

            // ASSERT

            Assert.Same(entity, result);
            Assert.True(this.modelController.ContainsEntity(entity.Id));
        }

        [Fact]
        public void TrackingModelController_raises_EntityChanged_on_second_changed_message()
        {
            // ARRANGE

            var entity = DefaultEntity();
            this.MessageBus.Entities.Modified(entity);

            // ACT

            var result = default(Entity);
            this.modelController.EntityChanged = t => result = t;
            this.MessageBus.Entities.Modified(entity);

            // ASSERT

            Assert.Same(entity, result);
        }

        [Fact]
        public void TrackingModelController_doesnt_raise_EntityRemoved_on_second_removed_message()
        {
            // ARRANGE

            var entity = DefaultEntity();
            this.MessageBus.Entities.Modified(entity);
            this.MessageBus.Entities.Removed(entity);

            // ACT

            var result = default(Guid?);
            this.modelController.EntityRemoved = id => result = id;
            this.MessageBus.Entities.Removed(entity);

            // ASSERT

            Assert.Null(result);
        }

        [Fact]
        public void TrackingModelController_raises_RelationshipAdded_on_first_changed_message()
        {
            // ARRANGE

            var relationship = DefaultRelationship();

            // ACT

            var result = default(Relationship);
            this.modelController.RelationshipAdded = t => result = t;
            this.MessageBus.Relationships.Modified(relationship);

            // ASSERT

            Assert.Same(relationship, result);
            Assert.True(this.modelController.ContainsRelationship(relationship.Id));
        }

        [Fact]
        public void TrackingModelController_raises_RelationshipChanged_on_second_changed_message()
        {
            // ARRANGE

            var entity = DefaultRelationship();
            this.MessageBus.Relationships.Modified(entity);

            // ACT

            var result = default(Relationship);
            this.modelController.RelationshipChanged = t => result = t;
            this.MessageBus.Relationships.Modified(entity);

            // ASSERT

            Assert.Same(entity, result);
        }

        [Fact]
        public void TrackingModelController_doesnt_raise_RelationshipRemoved_on_second_removed_message()
        {
            // ARRANGE

            var relationship = DefaultRelationship();
            this.MessageBus.Relationships.Modified(relationship);
            this.MessageBus.Relationships.Removed(relationship);

            // ACT

            var result = default(Guid?);
            this.modelController.RelationshipRemoved = id => result = id;
            this.MessageBus.Relationships.Removed(relationship);

            // ASSERT

            Assert.Null(result);
        }
    }
}